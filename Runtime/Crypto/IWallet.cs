using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.DataObjects;

namespace DapperLabs.Flow.Sdk.Crypto
{
    /// <summary>
    /// Interface for a flow Wallet provider. 
    /// To create a new wallet provider, implement this interface and register it 
    /// using FlowSDK.RegisterWalletProvider(). 
    /// </summary>
    public interface IWallet
    {
        /// <summary>
        /// Initializes the wallet provider. Should be called before calling anything else. 
        /// </summary>
        /// <param name="config">A reference to a derived WalletConfig object containing config data.</param>
        public void Init(WalletConfig config);

        /// <summary>
        /// Authenticate's a user with the wallet provider. 
        /// </summary>
        /// <param name="username">A username hint of the account to be authenticated.</param>
        /// <param name="OnAuthSuccess">Callback to be called on successful authentication. The account address is passed as a param.</param>
        /// <param name="OnAuthFailed">Callback to be called on failed authentication.</param>
        public Task Authenticate(string username, System.Action<string> OnAuthSuccess, System.Action OnAuthFailed);

        /// <summary>
        /// Unauthenticates a user from the wallet provider. Any cached authentication data
        /// will be cleared. 
        /// </summary>
        public void Unauthenticate();

        /// <summary>
        /// Signs a flow transaction's payload using the wallet provider. 
        /// </summary>
        /// <param name="txn">The transaction to be signed.</param>
        /// <returns>The signature in bytes.</returns>
        public Task<byte[]> SignTransactionPayload(FlowTransaction txn);

        /// <summary>
        /// Signs a flow transaction's authorization envelope using the wallet provider. 
        /// </summary>
        /// <param name="txn">The transaction to be signed.</param>
        /// <returns>The signature in bytes.</returns>
        public Task<byte[]> SignTransactionEnvelope(FlowTransaction txn);

        /// <summary>
        /// Checks if the user is authenticated with the wallet provider. 
        /// </summary>
        /// <returns>True if the user is authenticated.</returns>
        public bool IsAuthenticated();

        /// <summary>
        /// Retrieves the FlowAccount of the authenticated user. 
        /// </summary>
        /// <returns>The FlowAccount of the authenticated user.</returns>
        public SdkAccount GetAuthenticatedAccount();
    }

    /// <summary>
    /// Abstract base class for IWallet configuration. Inherit from this for a 
    /// specific IWallet implementation if needed, adding whatever config data is needed
    /// for that implementation. 
    /// </summary>
    public abstract class WalletConfig
    {

    }
}
