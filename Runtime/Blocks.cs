using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.Network;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Exceptions;
using System;

namespace DapperLabs.Flow.Sdk
{
	/// <summary>
	/// Block related functions
	/// </summary>
    public class Blocks
    {
	    /// <summary>
	    /// Gets a Block by its ID
	    /// </summary>
	    /// <param name="id">The ID of the block to fetch</param>
	    /// <returns>A Task that will resolve to a FlowBlock that has the given ID</returns>
        public static async Task<FlowBlock> GetById(string id)
        {
			try
			{
				return await NetworkClient.GetClient().GetBlockById(id);
			}
			catch (Exception ex)
			{
				return new FlowBlock
				{
					Error = new FlowError($"Blocks GetById failed, id: {id}. {ex.Message}", ex)
				};
			}
		}

	    /// <summary>
	    /// Gets a Block by its block height
	    /// </summary>
	    /// <param name="height">The height of the block to fetch</param>
	    /// <returns>A Task that will resolve to a FlowBlock</returns>
        public static async Task<FlowBlock> GetByHeight(ulong height)
        {
			try
			{
				return await NetworkClient.GetClient().GetBlockByHeight(height);
			}
			catch (Exception ex)
			{
				return new FlowBlock
				{
					Error = new FlowError($"Blocks GetByHeight failed, height: {height}. {ex.Message}", ex)
				};
			}
		}

	    /// <summary>
	    /// Gets the latest block on the chain.
	    /// </summary>
	    /// <param name="isSealed">If True, get the latest sealed block.  If false, get the latest available.</param>
	    /// <returns>A Task that will resolve into a FlowBlock.</returns>
        public static async Task<FlowBlock> GetLatest(bool isSealed = true)
        {
			try
			{
				return await NetworkClient.GetClient().GetLatestBlock(isSealed);
			}
			catch (Exception ex)
			{
				return new FlowBlock
				{
					Error = new FlowError($"Blocks GetLatest failed. {ex.Message}", ex)
				};
			}
		}
    }
}
