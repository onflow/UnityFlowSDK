/*
MIT License

Copyright (c) 2021 Tyron Brand

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DapperLabs.Flow.Sdk.DataObjects;

namespace DapperLabs.Flow.Sdk.DevWallet
{
    internal class Rlp
    {
        internal static byte[] EncodedAccountKey(FlowAccountKey flowAccountKey)
        {
            var accountElements = new List<byte[]>
            {
                RlpUtil.EncodeElement(flowAccountKey.PublicKey.FromHexToBytes()),
                RlpUtil.EncodeElement(ConvertorForRLPEncodingExtensions.ToBytesFromNumber(BitConverter.GetBytes(flowAccountKey.SignAlgo))),
                RlpUtil.EncodeElement(ConvertorForRLPEncodingExtensions.ToBytesFromNumber(BitConverter.GetBytes(flowAccountKey.HashAlgo))),
                RlpUtil.EncodeElement(ConvertorForRLPEncodingExtensions.ToBytesFromNumber(BitConverter.GetBytes(flowAccountKey.Weight)))
            };

            return RlpUtil.EncodeList(accountElements.ToArray());
        }

        internal static byte[] EncodedCanonicalPayload(FlowTransaction txRequest)
        {
            var payloadElements = new List<byte[]>
            {
                RlpUtil.EncodeElement(txRequest.Script.ToBytesForRLPEncoding()),
                RlpUtil.EncodeList(txRequest.Arguments.Select(argument => RlpUtil.EncodeElement(JsonConvert.SerializeObject(argument).ToBytesForRLPEncoding())).ToArray()),
                RlpUtil.EncodeElement(Extensions.Pad(txRequest.ReferenceBlockId.FromHexToBytes(), 32)),
                RlpUtil.EncodeElement(ConvertorForRLPEncodingExtensions.ToBytesFromNumber(BitConverter.GetBytes(txRequest.GasLimit))),
                RlpUtil.EncodeElement(Extensions.Pad(txRequest.ProposalKey.Address.FromHexToBytes(), 8)),
                RlpUtil.EncodeElement(ConvertorForRLPEncodingExtensions.ToBytesFromNumber(BitConverter.GetBytes(txRequest.ProposalKey.KeyId))),
                RlpUtil.EncodeElement(ConvertorForRLPEncodingExtensions.ToBytesFromNumber(BitConverter.GetBytes(txRequest.ProposalKey.SequenceNumber))),
                RlpUtil.EncodeElement(Extensions.Pad(txRequest.Payer.FromHexToBytes(), 8)),
                RlpUtil.EncodeList(txRequest.Authorizers.Select(authorizer => RlpUtil.EncodeElement(Extensions.Pad(authorizer.FromHexToBytes(), 8))).ToArray())
            };

            return RlpUtil.EncodeList(payloadElements.ToArray());
        }

        private static byte[] EncodedSignatures(IReadOnlyList<FlowTransactionSignature> signatures, FlowTransaction txRequest)
        {
            var signatureElements = new List<byte[]>();
            for (var i = 0; i < signatures.Count; i++)
            {
                var index = i;
                if (txRequest.SignerList.ContainsKey(signatures[i].Address))
                {
                    index = txRequest.SignerList[signatures[i].Address];
                }
                else
                {
                    txRequest.SignerList.Add(signatures[i].Address, i);
                }

                var signatureEncoded = EncodedSignature(signatures[i], index);
                signatureElements.Add(signatureEncoded);
            }

            return RlpUtil.EncodeList(signatureElements.ToArray());
        }

        private static byte[] EncodedSignature(FlowTransactionSignature signature, int index)
        {
            var signatureArray = new List<byte[]>
            {
                RlpUtil.EncodeElement(index.ToBytesForRLPEncoding()),
                RlpUtil.EncodeElement(ConvertorForRLPEncodingExtensions.ToBytesFromNumber(BitConverter.GetBytes(signature.KeyId))),
                RlpUtil.EncodeElement(signature.Signature)
            };

            return RlpUtil.EncodeList(signatureArray.ToArray());
        }

        internal static byte[] EncodedCanonicalAuthorizationEnvelope(FlowTransaction txRequest)
        {
            var authEnvelopeElements = new List<byte[]>
            {
                EncodedCanonicalPayload(txRequest),
                EncodedSignatures(txRequest.PayloadSignatures.ToArray(), txRequest)
            };

            return RlpUtil.EncodeList(authEnvelopeElements.ToArray());
        }

        internal static byte[] EncodedCanonicalPaymentEnvelope(FlowTransaction txRequest)
        {
            var authEnvelopeElements = new List<byte[]>
            {
                EncodedCanonicalAuthorizationEnvelope(txRequest),
                EncodedSignatures(txRequest.EnvelopeSignatures.ToArray(), txRequest)
            };

            return RlpUtil.EncodeList(authEnvelopeElements.ToArray());
        }

        internal static byte[] EncodedCanonicalTransaction(FlowTransaction txRequest)
        {
            var authEnvelopeElements = new List<byte[]>
            {
                EncodedCanonicalPayload(txRequest),
                EncodedSignatures(txRequest.PayloadSignatures.ToArray(), txRequest),
                EncodedSignatures(txRequest.EnvelopeSignatures.ToArray(), txRequest)
            };

            return RlpUtil.EncodeList(authEnvelopeElements.ToArray());
        }
    }
}
