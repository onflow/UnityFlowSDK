using System;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Network;
using DapperLabs.Flow.Sdk.DataObjects;

namespace DapperLabs.Flow.Sdk
{
    public class ExecutionResults
    {
        /// <summary>
        /// Gets a FlowExecutionResult for a given Block ID
        /// </summary>
        /// <param name="blockId">The ID of the block to query</param>
        /// <returns>A Task that will resolve to a FlowExecutionResult</returns>
        public static async Task<FlowExecutionResult> GetForBlockId(string blockId)
        {
            try
            {
                return await NetworkClient.GetClient().GetExecutionResultForBlockId(blockId);
            }
            catch (Exception ex)
            {
                return new FlowExecutionResult
                {
                    Error = new FlowError($"Getting ExecutionResults failed, blockId: {blockId}. {ex.Message}", ex)
                };
            }
        }
    }
}