using DapperLabs.Flow.Sdk.Crypto;
using DapperLabs.Flow.Sdk.Network;

namespace DapperLabs.Flow.Sdk
{
    /// <summary>
    /// Configuration to tell the FlowSDK which network to connect to.
    /// </summary>
    public class FlowConfig
    {
        public enum NetworkProtocol
        {
            HTTP
        }

        public static string MAINNETURL = "https://rest-mainnet.onflow.org/v1";
        public static string TESTNETURL = "https://rest-testnet.onflow.org/v1";
        
        public string NetworkUrl { get; set; } = "http://localhost:8888/v1";
        public NetworkProtocol Protocol { get; set; } = NetworkProtocol.HTTP;
    }


    /// <summary>
    /// Entry point for the Flow SDK.  To connect to a Flow network, call Init with a FlowConfig to determine which network
    /// to connect to.  That network will be used for all future FlowSDK operations until Init is called again.
    /// </summary>
    public class FlowSDK
    {
        internal static IWallet walletProvider;

        /// <summary>
        /// Initializes the Flow SDK. 
        /// </summary>
        /// <param name="config">Config information to initialize the Flow SDK such as network connection info.</param>
        public static void Init(FlowConfig config)
        {
            NetworkClient.Init(ref config);
        }

        /// <summary>
        /// Returns true if the FlowSDK is currently connected to a network.
        /// </summary>
        /// <returns></returns>
        public static bool IsNetworkConnected()
        {
            return NetworkClient.GetClient() != null;
        }

        /// <summary>
        /// Registers a Wallet provider to use for transaction signing. Only one wallet provider
        /// can be registered. 
        /// </summary>
        /// <param name="walletProvider">A concrete instance of IWallet</param>
        public static void RegisterWalletProvider(IWallet walletProvider)
        {
            FlowSDK.walletProvider = walletProvider;
        }

        /// <summary>
        /// Gets the registered Wallet provider. 
        /// </summary>
        /// <returns>The registered wallet provider</returns>
        public static IWallet GetWalletProvider()
        {
            return walletProvider;
        }
    }
}
