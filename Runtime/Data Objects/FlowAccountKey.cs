namespace DapperLabs.Flow.Sdk.DataObjects
{
    /// <summary>
    /// Represents a key on a Flow account
    /// </summary>
    public class FlowAccountKey
    {
        /// <summary>
        /// The ID of this key
        /// </summary>
        public uint Id;
        
        /// <summary>
        /// The public key
        /// </summary>
        public string PublicKey;
        
        /// <summary>
        /// The signing algorithm used by this key
        /// </summary>
        public uint SignAlgo;
        
        /// <summary>
        /// The hashing algorithm used by this key
        /// </summary>
        public uint HashAlgo;
        
        /// <summary>
        /// The weight of this key.  1000 total weight is required to execute a transaction.  If this key does not
        /// have 1000 weight, additional authorizers will be required.
        /// </summary>
        public uint Weight;
        
        /// <summary>
        /// The current sequence number of this key.  Any transaction submitted using this key must use the correct
        /// sequence number to prevent relay attacks.
        /// </summary>
        public uint SequenceNumber;
        
        /// <summary>
        /// Indicates if this key has been revoked.  Keys are not removed from an account, only revoked.
        /// </summary>
        public bool Revoked;
    }
}
