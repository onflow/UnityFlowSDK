using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    [Serializable]
    internal class TxSignRequestParamRole
    {
        [JsonProperty("proposer")]
        internal bool Proposer;

        [JsonProperty("authorizer")]
        internal bool Authorizer;

        [JsonProperty("payer")]
        internal bool Payer;

        [JsonProperty("param")]
        internal bool Param;
    }

    [Serializable]
    internal class TxSignRequestParamAccount
    {
        [JsonProperty("kind")]
        internal string Kind;

        [JsonProperty("tempId")]
        internal string TempId;

        [JsonProperty("addr")]
        internal string Addr;

        [JsonProperty("keyId")]
        internal int KeyId;

        [JsonProperty("sequenceNum")]
        internal int SequenceNum;

        [JsonProperty("signature")]
        internal string Signature;

        [JsonProperty("resolve")]
        internal string Resolve;

        [JsonProperty("role")]
        internal TxSignRequestParamRole Role;
    }

    [Serializable]
    internal class TxSignRequestParamArgument
    {
        [JsonProperty("kind")]
        internal string Kind;

        [JsonProperty("tempId")]
        internal string TempId;

        [JsonProperty("value")]
        internal string Value;

        [JsonProperty("asArgument")]
        internal object AsArgument;

        [JsonProperty("xform")]
        internal TxSignRequestParamXform Xform;
    }

    [Serializable]
    internal class TxSignRequestParamMessage
    {
        [JsonProperty("cadence")]
        internal string Cadence;

        [JsonProperty("refBlock")]
        internal string RefBlock;

        [JsonProperty("computeLimit")]
        internal int ComputeLimit;

        [JsonProperty("proposer")]
        internal string Proposer;

        [JsonProperty("payer")]
        internal string Payer;

        [JsonProperty("authorizations")]
        internal string[] Authorizations;

        [JsonProperty("params")]
        internal string[] Params;

        [JsonProperty("arguments")]
        internal string[] Arguments;
    }

    [Serializable]
    internal class TxSignRequestParamEvent
    {
        [JsonProperty("eventType")]
        internal string EventType;

        [JsonProperty("start")]
        internal string Start;

        [JsonProperty("end")]
        internal string End;

        [JsonProperty("blockIds")]
        internal string[] BlockIds;
    }

    [Serializable]
    internal class TxSignRequestParamTransaction
    {
        [JsonProperty("id")]
        internal string Id;
    }

    [Serializable]
    internal class TxSignRequestParamBlock
    {
        [JsonProperty("id")]
        internal string Id;

        [JsonProperty("height")]
        internal string Height;

        [JsonProperty("isSealed")]
        internal string IsSealed;
    }

    [Serializable]
    internal class TxSignRequestParamCollection
    {
        [JsonProperty("id")]
        internal string Id;
    }

    [Serializable]
    internal class TxSignRequestParamProposalKey
    {
        [JsonProperty("address")]
        internal string Address;

        [JsonProperty("keyId")]
        internal int KeyId;

        [JsonProperty("sequenceNum")]
        internal int SequenceNum;
    }

    [Serializable]
    internal class TxSignRequestParamSignature
    {
        [JsonProperty("address")]
        internal string Address;

        [JsonProperty("keyId")]
        internal int KeyId;

        [JsonProperty("sig")]
        internal string Sig;
    }

    [Serializable]
    internal class TxSignRequestParamXform
    {
        [JsonProperty("label")]
        internal string Label;
    }

    [Serializable]
    internal class TxSignRequestParamInteraction
    {
        [JsonProperty("tag")]
        internal string Tag;

        [JsonProperty("assigns")]
        internal Dictionary<string, string> Assigns;

        [JsonProperty("status")]
        internal string Status;

        [JsonProperty("reason")]
        internal string Reason;

        [JsonProperty("accounts")]
        internal Dictionary<string, TxSignRequestParamAccount> Accounts;

        [JsonProperty("params")]
        internal Dictionary<string, string> Params;

        [JsonProperty("arguments")]
        internal Dictionary<string, TxSignRequestParamArgument> Arguments;

        [JsonProperty("message")]
        internal TxSignRequestParamMessage Message;

        [JsonProperty("proposer")]
        internal string Proposer;

        [JsonProperty("authorizations")]
        internal string[] Authorizations;

        [JsonProperty("payer")]
        internal string[] Payer;

        [JsonProperty("events")]
        internal TxSignRequestParamEvent Events;

        [JsonProperty("transaction")]
        internal TxSignRequestParamTransaction Transaction;

        [JsonProperty("block")]
        internal TxSignRequestParamBlock Block;

        [JsonProperty("account")]
        internal TxSignRequestParamAccount Account;

        [JsonProperty("collection")]
        internal TxSignRequestParamCollection Collection;
    }

    [Serializable]
    internal class TxSignRequestParamVoucher
    {
        [JsonProperty("cadence")]
        internal string Cadence;

        [JsonProperty("refBlock")]
        internal string RefBlock;

        [JsonProperty("computeLimit")]
        internal int ComputeLimit;

        [JsonProperty("arguments")]
        internal object[] Arguments;

        [JsonProperty("proposalKey")]
        internal TxSignRequestParamProposalKey ProposalKey;

        [JsonProperty("payer")]
        internal string Payer;

        [JsonProperty("authorizers")]
        internal string[] Authorizers;

        [JsonProperty("payloadSigs")]
        internal TxSignRequestParamSignature[] PayloadSigs;

        [JsonProperty("envelopeSigs")]
        internal TxSignRequestParamSignature[] EnvelopeSigs;
    }

    [Serializable]
    internal class TxSignRequestParams
    {
        [JsonProperty("f_type")]
        internal string FType;

        [JsonProperty("f_vsn")]
        internal string FVsn;

        [JsonProperty("message")]
        internal string Message;

        [JsonProperty("addr")]
        internal string Addr;

        [JsonProperty("keyId")]
        internal int KeyId;

        [JsonProperty("roles")]
        internal TxSignRequestParamRole Roles;

        [JsonProperty("cadence")]
        internal string Cadence;

        [JsonProperty("args")]
        internal object[] Args;

        [JsonProperty("interaction")]
        internal TxSignRequestParamInteraction Interaction;

        [JsonProperty("voucher")]
        internal TxSignRequestParamVoucher Voucher;

        [JsonProperty("address")]
        internal string Address;
    }

    internal class TxSignResponseData
    {
        public string addr;
        public string keyId;
        public string signature;
    }

    internal class TxSignResponse
    {
        public TxSignResponseData data;
        public string status;
        public string reason;
        public string type;
        public string f_type;
        public string f_vsn;
    }
}
