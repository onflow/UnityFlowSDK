using System.Collections.Generic;
using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Niftory
{
    internal class AuthResponse
    {
        [JsonProperty("device_code")]
        public string? DeviceCode { get; set; }

        [JsonProperty("user_code")]
        public string? UserCode { get; set; }


        [JsonProperty("verification_uri")]
        public string? VerificationUri { get; set; }

        [JsonProperty("verification_uri_complete")]
        public string? VerificationUriComplete { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }

    internal class TokenResponse
    {
        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("error_description")]
        public string? ErrorDescription { get; set; }

        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }

        [JsonProperty("id_token")]
        public string? IdToken { get; set; }

        [JsonProperty("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string? Scope { get; set; }

        [JsonProperty("token_type")]
        public string? Bearer { get; set; }
    }

    internal class Token
    {
        public string IdToken;
        public string RefreshToken;
    }

    internal class NiftoryWallet
    {
        [JsonProperty("address")]
        public string? Address;

        [JsonProperty("id")]
        public string Id;
    }

    internal class NiftoryWalletData
    {
        [JsonProperty("wallet")]
        public NiftoryWallet Wallet;
    }

    internal class NiftoryWalletResponse
    {
        [JsonProperty("data")]
        public NiftoryWalletData Data;
    }

    internal class NiftoryTransaction
    {
        [JsonProperty("address")]
        public string Address;

        [JsonProperty("transaction")]
        public string Script;

        [JsonProperty("address")]
        public Dictionary<string, string> Arguments;
    }

    internal class NiftoryTransactionResult
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("hash")]
        public string Hash;

        [JsonProperty("state")]
        public string State;

        [JsonProperty("result")]
        public string Result;
    }

    internal class NiftoryTransactionData
    {
        [JsonProperty("executeTransaction")]
        public NiftoryTransactionResult Transaction;
    }

    internal class NiftoryTransactionError
    {
        [JsonProperty("message")]
        public string Message;
    }

    internal class NiftoryTransactionResponse
    {
        [JsonProperty("data")]
        public NiftoryTransactionData Data;
        
        [JsonProperty("errors")]
        public NiftoryTransactionError[] Errors;
    }

    internal class GrpahQLRequest
    {
        [JsonProperty("query")]
        public string Query;

        [JsonProperty("operationName", NullValueHandling = NullValueHandling.Ignore)]
        public string? OperationName;

        [JsonProperty("variables", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Variables;
    }
}
