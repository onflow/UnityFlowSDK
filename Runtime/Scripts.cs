using System;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Network;
using DapperLabs.Flow.Sdk.DataObjects;

namespace DapperLabs.Flow.Sdk
{
    /// <summary>
    /// Provides the ability to run scripts on the blockchain.
    /// </summary>
    public class Scripts
    {
        /// <summary>
        /// Executes the passed FlowScriptRequest on the currently connected network against the latest block on the blockchain.
        /// </summary>
        /// <param name="scriptRequest">A FlowScriptRequest containing the script information.</param>
        /// <returns>A Task that will resolve into a FlowScriptResponse when complete.</returns>
        public static async Task<FlowScriptResponse> ExecuteAtLatestBlock(FlowScriptRequest scriptRequest)
        {
			try
			{
				return await NetworkClient.GetClient().ExecuteScriptAtLatestBlock(scriptRequest);
			}
			catch (Exception ex)
			{
				return new FlowScriptResponse()
				{
					Error = new FlowError($"FlowScriptResponse encountered an error. {ex.Message}", ex)
				};
			}
		}
    }
}
