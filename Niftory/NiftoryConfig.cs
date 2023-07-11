using DapperLabs.Flow.Sdk.Crypto;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.Niftory
{
    public class NiftoryConfig : WalletConfig
    {
        public string ClientId;
        public string AuthUrl;
        public string GraphQLUrl;
        public object QrCodeDialogPrefab;
    }
}
