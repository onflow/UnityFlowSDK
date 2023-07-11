using System;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Network;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Cadence;
using System.Collections.Generic;

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

		/// <summary>
		/// Executes the passed script and parameters on the currently connected network against the latest block on the blockchain.
		/// </summary>
		/// <param name="script">The text contents of the script to execute</param>
		/// <param name="arguments">Cadence arguments that will be passed to the script</param>
		/// <returns>A Task that will resolve into a FlowScriptResponse when complete.</returns>
		public static async Task<FlowScriptResponse> ExecuteAtLatestBlock(string script, params CadenceBase[] arguments)
		{
			FlowScriptRequest scriptRequest = new FlowScriptRequest
			{
				Script = script,
				Arguments = new List<CadenceBase>(arguments)
			};

			return await ExecuteAtLatestBlock(scriptRequest);
		}
	}
}
