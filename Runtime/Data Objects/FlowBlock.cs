using DapperLabs.Flow.Sdk.Exceptions;
using System;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.DataObjects
{
	/// <summary>
	/// Represents a Block on the Flow blockchain
	/// </summary>
    public class FlowBlock
    {
	    /// <summary>
	    /// The ID of the block
	    /// </summary>
        public string Id;
	    
	    /// <summary>
	    /// The ID of the parent of this block
	    /// </summary>
        public string ParentId;
	    
	    /// <summary>
	    /// The block height of this block
	    /// </summary>
        public ulong Height;
	    
	    
        public DateTimeOffset Timestamp;
        public List<FlowCollectionGuarantee> CollectionGuarantees;
        public List<FlowBlockSeal> BlockSeals;
        public List<string> Signatures;
		public FlowError Error;
	}
}
