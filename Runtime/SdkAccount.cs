using DapperLabs.Flow.Sdk.Exceptions;

namespace DapperLabs.Flow.Sdk
{
    /// <summary>
    /// Holds account information required when submitting a transaction.
    /// </summary>
    public class SdkAccount
    {
        /// <summary>
        /// Name of the account.  Only used for identifying the account within the SDK, not used by the Flow blockchain.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Flow account address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Private key of the account.
        /// </summary>
        public string PrivateKey { get; set; }

		/// <summary>
		/// An error, if one occurs during the account retrieval.  Null if no error occurred
		/// </summary>
		public FlowError Error { get; set; }
	}
}
