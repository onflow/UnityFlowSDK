using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    [Serializable]
    public class TxSignRequestParamRole
    {
        [JsonProperty("proposer")]
        public bool Proposer;

        [JsonProperty("authorizer")]
        public bool Authorizer;

        [JsonProperty("payer")]
        public bool Payer;

        [JsonProperty("param")]
        public bool Param;
    }

    [Serializable]
    public class TxSignRequestParamAccount
    {
        [JsonProperty("kind")]
        public string Kind;

        [JsonProperty("tempId")]
        public string TempId;

        [JsonProperty("addr")]
        public string Addr;

        [JsonProperty("keyId")]
        public int KeyId;

        [JsonProperty("sequenceNum")]
        public int SequenceNum;

        [JsonProperty("signature")]
        public string Signature;

        [JsonProperty("resolve")]
        public string Resolve;

        [JsonProperty("role")]
        public TxSignRequestParamRole Role;
    }

    [Serializable]
    public class TxSignRequestParamArgument
    {
        [JsonProperty("kind")]
        public string Kind;

        [JsonProperty("tempId")]
        public string TempId;

        [JsonProperty("value")]
        public string Value;

        [JsonProperty("asArgument")]
        public object AsArgument;

        [JsonProperty("xform")]
        public TxSignRequestParamXform Xform;
    }

    [Serializable]
    public class TxSignRequestParamMessage
    {
        [JsonProperty("cadence")]
        public string Cadence;

        [JsonProperty("refBlock")]
        public string RefBlock;

        [JsonProperty("computeLimit")]
        public int ComputeLimit;

        [JsonProperty("proposer")]
        public string Proposer;

        [JsonProperty("payer")]
        public string Payer;

        [JsonProperty("authorizations")]
        public string[] Authorizations;

        [JsonProperty("params")]
        public string[] Params;

        [JsonProperty("arguments")]
        public string[] Arguments;
    }

    [Serializable]
    public class TxSignRequestParamEvent
    {
        [JsonProperty("eventType")]
        public string EventType;

        [JsonProperty("start")]
        public string Start;

        [JsonProperty("end")]
        public string End;

        [JsonProperty("blockIds")]
        public string[] BlockIds;
    }

    [Serializable]
    public class TxSignRequestParamTransaction
    {
        [JsonProperty("id")]
        public string Id;
    }

    [Serializable]
    public class TxSignRequestParamBlock
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("height")]
        public string Height;

        [JsonProperty("isSealed")]
        public string IsSealed;
    }

    [Serializable]
    public class TxSignRequestParamCollection
    {
        [JsonProperty("id")]
        public string Id;
    }

    [Serializable]
    public class TxSignRequestParamProposalKey
    {
        [JsonProperty("address")]
        public string Address;

        [JsonProperty("keyId")]
        public int KeyId;

        [JsonProperty("sequenceNum")]
        public int SequenceNum;
    }

    [Serializable]
    public class TxSignRequestParamSignature
    {
        [JsonProperty("address")]
        public string Address;

        [JsonProperty("keyId")]
        public int KeyId;

        [JsonProperty("sig")]
        public string Sig;
    }

    [Serializable]
    public class TxSignRequestParamXform
    {
        [JsonProperty("label")]
        public string Label;
    }

    [Serializable]
    public class TxSignRequestParamInteraction
    {
        [JsonProperty("tag")]
        public string Tag;

        [JsonProperty("assigns")]
        public Dictionary<string, string> Assigns;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("reason")]
        public string Reason;

        [JsonProperty("accounts")]
        public Dictionary<string, TxSignRequestParamAccount> Accounts;

        [JsonProperty("params")]
        public Dictionary<string, string> Params;

        [JsonProperty("arguments")]
        public Dictionary<string, TxSignRequestParamArgument> Arguments;

        [JsonProperty("message")]
        public TxSignRequestParamMessage Message;

        [JsonProperty("proposer")]
        public string Proposer;

        [JsonProperty("authorizations")]
        public string[] Authorizations;

        [JsonProperty("payer")]
        public string[] Payer;

        [JsonProperty("events")]
        public TxSignRequestParamEvent Events;

        [JsonProperty("transaction")]
        public TxSignRequestParamTransaction Transaction;

        [JsonProperty("block")]
        public TxSignRequestParamBlock Block;

        [JsonProperty("account")]
        public TxSignRequestParamAccount Account;

        [JsonProperty("collection")]
        public TxSignRequestParamCollection Collection;
    }

    [Serializable]
    public class TxSignRequestParamVoucher
    {
        [JsonProperty("cadence")]
        public string Cadence;

        [JsonProperty("refBlock")]
        public string RefBlock;

        [JsonProperty("computeLimit")]
        public int ComputeLimit;

        [JsonProperty("arguments")]
        public object[] Arguments;

        [JsonProperty("proposalKey")]
        public TxSignRequestParamProposalKey ProposalKey;

        [JsonProperty("payer")]
        public string Payer;

        [JsonProperty("authorizers")]
        public string[] Authorizers;

        [JsonProperty("payloadSigs")]
        public TxSignRequestParamSignature[] PayloadSigs;

        [JsonProperty("envelopeSigs")]
        public TxSignRequestParamSignature[] EnvelopeSigs;
    }

    [Serializable]
    public class TxSignRequestParams
    {
        [JsonProperty("f_type")]
        public string FType;

        [JsonProperty("f_vsn")]
        public string FVsn;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("addr")]
        public string Addr;

        [JsonProperty("keyId")]
        public int KeyId;

        [JsonProperty("roles")]
        public TxSignRequestParamRole Roles;

        [JsonProperty("cadence")]
        public string Cadence;

        [JsonProperty("args")]
        public object[] Args;

        [JsonProperty("interaction")]
        public TxSignRequestParamInteraction Interaction;

        [JsonProperty("voucher")]
        public TxSignRequestParamVoucher Voucher;

        [JsonProperty("address")]
        public string Address;
    }

    public class TxSignResponseData
    {
        public string addr;
        public string keyId;
        public string signature;
    }

    public class TxSignResponse
    {
        public TxSignResponseData data;
        public string status;
        public string reason;
        public string type;
        public string f_type;
        public string f_vsn;
    }
}
