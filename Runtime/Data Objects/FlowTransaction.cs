/*
MIT License

Copyright (c) 2021 Tyron Brand

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DapperLabs.Flow.Sdk.Cadence;

[assembly: InternalsVisibleTo("DapperLabs.FlowSDK.DevWallet")]

namespace DapperLabs.Flow.Sdk.DataObjects
{
    public class FlowTransaction
    {
        public string Script;
        public List<CadenceBase> Arguments = new List<CadenceBase>();
        public string ReferenceBlockId;
        public ulong GasLimit;
        public FlowTransactionProposalKey ProposalKey;
        public string Payer;
        public List<string> Authorizers = new List<string>();
        public List<FlowTransactionSignature> PayloadSignatures = new List<FlowTransactionSignature>();
        public List<FlowTransactionSignature> EnvelopeSignatures = new List<FlowTransactionSignature>();
		public Exceptions.FlowError Error;

        internal Dictionary<string, int> SignerList { get; }

        internal void AddArgument(CadenceBase arg)
        {
            Arguments.Add(arg);
        }

        internal void AddAuthorizer(string address)
        {
            Authorizers.Add(address);
        }

        internal void AddPayloadSignature(string address, uint keyId, byte[] signature)
        {
            PayloadSignatures.Add(
                new FlowTransactionSignature
                {
                    Address = address,
                    KeyId = keyId,
                    Signature = signature
                });
        }

        internal void AddEnvelopeSignature(string address, uint keyId, byte[] signature)
        {
            EnvelopeSignatures.Add(
                new FlowTransactionSignature
                {
                    Address = address,
                    KeyId = keyId,
                    Signature = signature
                });
        }

        
    }
}
