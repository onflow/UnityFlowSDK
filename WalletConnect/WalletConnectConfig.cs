using DapperLabs.Flow.Sdk.Crypto;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    /// <summary>
    /// Configuration for initializing Wallet Connect. 
    /// </summary>
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
