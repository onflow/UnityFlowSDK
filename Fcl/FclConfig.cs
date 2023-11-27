using DapperLabs.Flow.Sdk.Crypto;

namespace DapperLabs.Flow.Sdk.Fcl
{
    /// <summary>
    /// Configuration for initializing FCL. 
    /// </summary>
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
