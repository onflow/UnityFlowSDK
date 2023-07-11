using System;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.Network
{
    [Serializable]
    internal class HttpBlock
    {
        public HttpBlockHeader header;
        public HttpBlockPayload payload;
        public HttpExecutionResult execution_result;
    }

    [Serializable]
    internal class HttpBlockHeader
    {
        public string id;
        public string parent_id;
        public string height;
        public string timestamp;
        public string parent_voter_signature;
    }

    [Serializable]
    internal class HttpBlockPayload
    {
        public HttpCollectionGuarantee[] collection_guarantees;
        public HttpBlockSeal[] block_seals;
    }

    [Serializable]
    internal class HttpExecutionResult
    {
        public string id;
        public string block_id;
        public HttpEvent[] events;
        public HttpChunk[] chunks;
        public string previous_result_id;
    }

    [Serializable]
    internal class HttpCollectionGuarantee
    {
        public string collection_id;
        public string[] signer_ids;
        public string signature;
    }

    [Serializable]
    internal class HttpBlockSeal
    {
        public string block_id;
        public string result_id;
        public string final_state;
        public HttpAggregatedSignature[] aggregated_approval_signatures;
    }

    [Serializable]
    internal class HttpAggregatedSignature
    {
        public string[] verifier_signatures;
        public string[] signer_ids;
    }

    [Serializable]
    internal class HttpEvent
    {
        public string type;
        public string transaction_id;
        public string transaction_index;
        public string event_index;
        public string payload;
    }

    [Serializable]
    internal class HttpChunk
    {
        public string block_id;
        public string collection_index;
        public string start_state;
        public string end_state;
        public string event_collection;
        public string index;
        public string number_of_transactions;
        public string total_computation_used;
    }

    [Serializable]
    internal class HttpCollection
    {
        public string id;
        public HttpTransaction[] transactions;
    }

    [Serializable]
    internal class HttpTransaction
    {
        public string id;
        public string script;
        public string[] arguments;
        public string reference_block_id;
        public string gas_limit;
        public string payer;
        public HttpProposalKey proposal_key;
        public string[] authorizers;
        public HttpTransactionSignature[] payload_signatures;
        public HttpTransactionSignature[] envelope_signatures;
        public HttpTransactionResult result;
    }

    [Serializable]
    internal class HttpTransactionRequest
    {
        public string script;
        public string[] arguments;
        public string reference_block_id;
        public string gas_limit;
        public string payer;
        public HttpProposalKey proposal_key;
        public string[] authorizers;
        public HttpTransactionSignature[] payload_signatures;
        public HttpTransactionSignature[] envelope_signatures;
    }

    [Serializable]
    internal class HttpProposalKey
    {
        public string address;
        public string key_index;
        public string sequence_number;
    }

    [Serializable]
    internal class HttpTransactionSignature
    {
        public string address;
        public string key_index;
        public string signature;
    }

    [Serializable]
    internal class HttpTransactionResult
    {
        public string block_id;
        public string execution;
        public string status;
        public int status_code;
        public string error_message;
        public string computation_used;
        public HttpEvent[] events;
    }

    [Serializable]
    internal class HttpEventGroup
    {
        public string block_id;
        public string block_height;
        public string block_timestamp;
        public HttpEvent[] events;
    }

    [Serializable]
    internal class HttpAccount
    {
        public string address;
        public string balance;
        public HttpAccountPublicKey[] keys;
        public Dictionary<string, string> contracts = new Dictionary<string, string>();
    }

    [Serializable]
    internal class HttpAccountPublicKey
    {
        public string index;
        public string public_key;
        public string signing_algorithm;
        public string hashing_algorithm;
        public string sequence_number;
        public string weight;
        public bool revoked;
    }

    [Serializable]
    internal class HttpScriptRequest
    {
        public string script;
        public string[] arguments;
    }
}