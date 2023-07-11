using DapperLabs.Flow.Sdk.Crypto;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.Fcl
{
    public class FclConfig : WalletConfig
    {
        public string IconUri;
        public string Title;
        public string Location;

        // Wallet connect
        public string Description;
        public string Url;
        public string WalletConnectProjectId;
        public object WalletConnectQrCodeDialogPrefab = null;
    }
}
