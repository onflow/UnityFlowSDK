using System;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Network;

namespace DapperLabs.Flow.Sdk
{
	/// <summary>
	/// %Flow account related functions
	/// </summary>
    public class Accounts
    {
	    /// <summary>
	    /// Get a FlowAccount for the specified %Flow address
	    /// </summary>
	    /// <param name="address">The %Flow address</param>
	    /// <returns>A Task that will resolve to a FlowAccount object</returns>
        public static async Task<FlowAccount> GetByAddress(string address)
        {
			try
			{
				return await NetworkClient.GetClient().GetAccountByAddress(address);
			}
			catch (Exception ex)
			{
				return new FlowAccount
				{
					Error = new FlowError($"Accounts GetByAddress failed. {ex.Message}", ex)
				};
			}
        }
    }
}
