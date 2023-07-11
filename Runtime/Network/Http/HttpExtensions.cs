using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.Crypto;
using DapperLabs.Flow.Sdk.DataObjects;

namespace DapperLabs.Flow.Sdk.Network
{
    internal static class HttpExtensions
    {
        internal static List<FlowBlock> ToFlowBlocks(this HttpBlock[] src)
        {
            List<FlowBlock> ret = new List<FlowBlock>();

            foreach (HttpBlock httpBlock in src)
            {
                FlowBlock block = new FlowBlock
                {
                    Id = httpBlock.header.id,
                    ParentId = httpBlock.header.parent_id,
                    Height = ulong.Parse(httpBlock.header.height),
                    Timestamp = DateTimeOffset.Parse(httpBlock.header.timestamp),
                    Signatures = new List<string>()
                };

                if (httpBlock.payload != null)
                {
                    block.CollectionGuarantees = httpBlock.payload.collection_guarantees.ToFlowCollectionGuarantees();
                    block.BlockSeals = httpBlock.payload.block_seals.ToFlowBlockSeals();
                }

                ret.Add(block);
            }

            return ret;
        }

        internal static List<FlowCollectionGuarantee> ToFlowCollectionGuarantees(this HttpCollectionGuarantee[] src)
        {
            List<FlowCollectionGuarantee> ret = new List<FlowCollectionGuarantee>();

            foreach (HttpCollectionGuarantee collGuarantee in src)
            {
                ret.Add(new FlowCollectionGuarantee
                {
                    CollectionId = collGuarantee.collection_id
                });
            }

            return ret;
        }

        internal static List<FlowBlockSeal> ToFlowBlockSeals(this HttpBlockSeal[] src)
        {
            List<FlowBlockSeal> ret = new List<FlowBlockSeal>();

            foreach (HttpBlockSeal blockSeal in src)
            {
                ret.Add(new FlowBlockSeal
                {
                    BlockId = blockSeal.block_id,
                });
            }

            return ret;
        }

        internal static FlowCollection ToFlowCollection(this HttpCollection src)
        {
            return new FlowCollection
            {
                Id = src.id,
                TransactionIds = src.transactions.ToTransactionIds()
            };
        }

        internal static List<string> ToTransactionIds(this HttpTransaction[] src)
        {
            List<string> ret = new List<string>();

            foreach (HttpTransaction tx in src)
            {
                ret.Add(tx.id);
            }

            return ret;
        }

        internal static List<FlowEventGroup> ToFlowEventGroups(this HttpEventGroup[] src)
        {
            List<FlowEventGroup> ret = new List<FlowEventGroup>();

            foreach (HttpEventGroup ev in src)
            {
                ret.Add(new FlowEventGroup
                {
                    BlockId = ev.block_id,
                    BlockHeight = ulong.Parse(ev.block_height),
                    BlockTimestamp = DateTimeOffset.Parse(ev.block_timestamp),
                    Events = ev.events.ToFlowEvents()
                });
            }

            return ret;
        }

        internal static List<FlowEvent> ToFlowEvents(this HttpEvent[] src)
        {
            List<FlowEvent> ret = new List<FlowEvent>();

            foreach (HttpEvent ev in src)
            {
                FlowEvent flowEvent = new FlowEvent
                {
                    Type = ev.type,
                    TransactionId = ev.transaction_id,
                    TransactionIndex = uint.Parse(ev.transaction_index),
                    EventIndex = uint.Parse(ev.event_index)
                };

                byte[] payloadData = System.Convert.FromBase64String(ev.payload);
                string decodedPayload = Encoding.UTF8.GetString(payloadData);

                flowEvent.Payload = JsonConvert.DeserializeObject<CadenceBase>(decodedPayload, new CadenceCreationConverter());

                ret.Add(flowEvent);
            }

            return ret;
        }

        internal static FlowAccount ToFlowAccount(this HttpAccount src)
        {
            return new FlowAccount
            {
                Address = src.address,
                Balance = ulong.Parse(src.balance),
                Keys = src.keys.ToFlowAccountKeys(),
                Contracts = src.contracts.ToFlowContracts()
            };
        }

        internal static List<FlowAccountKey> ToFlowAccountKeys(this HttpAccountPublicKey[] src)
        {
            List<FlowAccountKey> ret = new List<FlowAccountKey>();

            foreach (HttpAccountPublicKey key in src)
            {
                ret.Add(new FlowAccountKey
                {
                    Id = uint.Parse(key.index),
                    PublicKey = key.public_key,
                    SignAlgo = (uint)(SignatureAlgo)Enum.Parse(typeof(SignatureAlgo), key.signing_algorithm),
                    HashAlgo = (uint)(HashAlgo)Enum.Parse(typeof(HashAlgo), key.hashing_algorithm),
                    SequenceNumber = uint.Parse(key.sequence_number),
                    Weight = uint.Parse(key.weight),
                    Revoked = key.revoked
                });
            }

            return ret;
        }

        internal static List<FlowContract> ToFlowContracts(this Dictionary<string, string> src)
        {
            List<FlowContract> ret = new List<FlowContract>();

            foreach (KeyValuePair<string, string> contract in src)
            {
                ret.Add(new FlowContract
                {
                    Name = contract.Key,
                    Code = contract.Value
                });
            }

            return ret;
        }

        internal static HttpScriptRequest ToHttpScriptRequest(this FlowScriptRequest src)
        {
            HttpScriptRequest ret = new HttpScriptRequest();

            byte[] scriptBytes = Encoding.UTF8.GetBytes(src.Script);
            ret.script = System.Convert.ToBase64String(scriptBytes);

            List<string> temp = new List<string>();
            foreach (CadenceBase arg in src.Arguments)
            {
                string jsonArg = JsonConvert.SerializeObject(arg);
                byte[] argBytes = Encoding.UTF8.GetBytes(jsonArg);
                temp.Add(System.Convert.ToBase64String(argBytes));
            }

            ret.arguments = temp.ToArray();

            return ret;
        }

        internal static HttpTransactionRequest ToHttpTransactionRequest(this FlowTransaction src)
        {
            HttpTransactionRequest ret = new HttpTransactionRequest();

            byte[] scriptBytes = Encoding.UTF8.GetBytes(src.Script);
            ret.script = System.Convert.ToBase64String(scriptBytes);

            List<string> temp = new List<string>();
            foreach (CadenceBase arg in src.Arguments)
            {
                string jsonArg = JsonConvert.SerializeObject(arg);
                byte[] argBytes = Encoding.UTF8.GetBytes(jsonArg);
                temp.Add(System.Convert.ToBase64String(argBytes));
            }

            ret.arguments = temp.ToArray();

            ret.reference_block_id = src.ReferenceBlockId;
            ret.gas_limit = src.GasLimit.ToString();
            ret.payer = src.Payer;
            ret.proposal_key = src.ProposalKey.ToHttpProposalKey();
            ret.authorizers = src.Authorizers.ToArray();
            ret.payload_signatures = src.PayloadSignatures.ToHttpTransactionSignatures();
            ret.envelope_signatures = src.EnvelopeSignatures.ToHttpTransactionSignatures();

            return ret;
        }

        internal static HttpProposalKey ToHttpProposalKey(this FlowTransactionProposalKey src)
        {
            return new HttpProposalKey
            {
                address = src.Address,
                key_index = src.KeyId.ToString(),
                sequence_number = src.SequenceNumber.ToString()
            };
        }

        internal static HttpTransactionSignature[] ToHttpTransactionSignatures(this List<FlowTransactionSignature> src)
        {
            List<HttpTransactionSignature> ret = new List<HttpTransactionSignature>();

            foreach (FlowTransactionSignature sig in src)
            {
                ret.Add(new HttpTransactionSignature
                {
                    address = sig.Address,
                    key_index = sig.KeyId.ToString(),
                    signature = System.Convert.ToBase64String(sig.Signature)
                });
            }

            return ret.ToArray();
        }

        internal static FlowTransaction ToFlowTransaction(this HttpTransaction src)
        {
            FlowTransaction tx = new FlowTransaction
            {
                Arguments = new List<CadenceBase>(),
                ReferenceBlockId = src.reference_block_id,
                GasLimit = ulong.Parse(src.gas_limit),
                Payer = src.payer,
                ProposalKey = src.proposal_key.ToFlowTransactionProposalKey(),
                Authorizers = new List<string>(src.authorizers),
                PayloadSignatures = src.payload_signatures.ToFlowTransactionSignatures(),
                EnvelopeSignatures = src.envelope_signatures.ToFlowTransactionSignatures()
            };

            byte[] scriptBytes = System.Convert.FromBase64String(src.script);
            tx.Script = Encoding.UTF8.GetString(scriptBytes);

            foreach (string arg in src.arguments)
            {
                byte[] argBytes = System.Convert.FromBase64String(arg);
                string jsonArg = Encoding.UTF8.GetString(argBytes);
                CadenceBase cadenceArg = JsonConvert.DeserializeObject<CadenceBase>(jsonArg, new CadenceCreationConverter());
                tx.Arguments.Add(cadenceArg);
            }

            return tx;
        }

        internal static FlowTransactionProposalKey ToFlowTransactionProposalKey(this HttpProposalKey src)
        {
            return new FlowTransactionProposalKey
            {
                Address = src.address,
                KeyId = uint.Parse(src.key_index),
                SequenceNumber = ulong.Parse(src.sequence_number)
            };
        }

        internal static List<FlowTransactionSignature> ToFlowTransactionSignatures(this HttpTransactionSignature[] src)
        {
            List<FlowTransactionSignature> ret = new List<FlowTransactionSignature>();

            foreach (HttpTransactionSignature sig in src)
            {
                ret.Add(new FlowTransactionSignature
                {
                    Address = sig.address,
                    KeyId = uint.Parse(sig.key_index),
                    Signature = System.Convert.FromBase64String(sig.signature)
                });
            }

            return ret;
        }

        internal static FlowTransactionResult ToFlowTransactionResult(this HttpTransactionResult src)
        {
            FlowTransactionStatus status = FlowTransactionStatus.UNKNOWN;
            Enum.TryParse(src.status.ToUpper(), out status);

            return new FlowTransactionResult
            {
                Status = status,
                StatusCode = (uint)src.status_code,
                ErrorMessage = src.error_message,
                Events = src.events.ToFlowEvents()
            };
        }

        internal static FlowExecutionResult ToFlowExecutionResult(this HttpExecutionResult src)
        {
            return new FlowExecutionResult
            {
                BlockId = src.block_id,
                PreviousResultId = src.previous_result_id,
                ServiceEvents = src.events.ToFlowServiceEvents(),
                Chunks = src.chunks.ToFlowChunks()
            };
        }

        internal static List<FlowServiceEvent> ToFlowServiceEvents(this HttpEvent[] src)
        {
            List<FlowServiceEvent> ret = new List<FlowServiceEvent>();

            foreach (HttpEvent ev in src)
            {
                FlowServiceEvent flowEvent = new FlowServiceEvent
                {
                    Type = ev.type,
                    Payload = ev.payload
                };

                byte[] payloadData = System.Convert.FromBase64String(ev.payload);
                flowEvent.Payload = Encoding.UTF8.GetString(payloadData);

                ret.Add(flowEvent);
            }

            return ret;
        }

        internal static List<FlowChunk> ToFlowChunks(this HttpChunk[] src)
        {
            List<FlowChunk> ret = new List<FlowChunk>();

            foreach (HttpChunk chunk in src)
            {
                ret.Add(new FlowChunk
                {
                    BlockId = chunk.block_id,
                    StartState = chunk.start_state,
                    EndState = chunk.end_state,
                    EventCollection = chunk.event_collection,
                    Index = ulong.Parse(chunk.index),
                    NumberOfTransactions = ulong.Parse(chunk.number_of_transactions),
                    TotalComputationUsed = ulong.Parse(chunk.total_computation_used)
                });
            }

            return ret;
        }
    }
}
