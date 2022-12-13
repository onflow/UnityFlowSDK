using DapperLabs.Flow.Sdk.Exceptions;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.DataObjects
{
	/// <summary>
	/// Represents an account on the Flow network
	/// </summary>
    public class FlowAccount
    {
	    /// <summary>
	    /// The address of the Flow account
	    /// </summary>
        public string Address;
	    
	    /// <summary>
	    /// The balance, in Flow Tokens, this account contains
	    /// </summary>
        public ulong Balance;
	    
	    /// <summary>
	    /// A list of keys that have access to 
	    /// </summary>
        public List<FlowAccountKey> Keys;
	    
	    /// <summary>
	    /// A list of contracts deployed to this account
	    /// </summary>
        public List<FlowContract> Contracts;
	    
	    /// <summary>
	    /// Error field used to indicate and error occurred when creating or getting FlowAccount objects.
	    /// </summary>
		public FlowError Error;
	}
}
