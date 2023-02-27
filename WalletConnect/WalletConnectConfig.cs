using DapperLabs.Flow.Sdk.Crypto;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    public class WalletConnectConfig : WalletConfig
    {
        public string ProjectId;
        public string ProjectDescription;
        public string ProjectIconUrl;
        public string ProjectName;
        public string ProjectUrl;
        public object QrCodeDialogPrefab;
        public object WalletSelectDialogPrefab;
    }
}
