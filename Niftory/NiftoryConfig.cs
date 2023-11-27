using DapperLabs.Flow.Sdk.Crypto;

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
