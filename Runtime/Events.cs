using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Network;

namespace DapperLabs.Flow.Sdk
{
	/// <summary>
	/// Event related functions
	/// </summary>
    public class Events
    {
	    /// <summary>
	    /// Get all event groups (each of which can contain several events) of a given type in a given block height range
	    /// </summary>
	    /// <param name="type">The type of event to query for</param>
	    /// <param name="startHeight">The block height at which to begin searching</param>
	    /// <param name="endHeight">The block height at which to end searching</param>
	    /// <returns>A Task that resolves to a FlowEventGroup</returns>
        public static async Task<List<FlowEventGroup>> GetForBlockHeightRange(string type, ulong startHeight, ulong endHeight)
        {
			try
			{
				return await NetworkClient.GetClient().GetEventsForHeightRange(type, startHeight, endHeight);
			}
			catch (Exception ex)
			{
				return new List<FlowEventGroup>
				{
					new FlowEventGroup
					{
						Error = new FlowError($"Events GetForBlockHeightRange failed, type: {type}, startHeight: {startHeight}, endHeight: {endHeight}. {ex.Message}", ex)
					}
				};
			}
		}

	    /// <summary>
	    /// Get all event groups (each of which can contain several events) of a given type in a given list of blocks
	    /// </summary>
	    /// <param name="type">The type of event to query for</param>
	    /// <param name="blockIds">A List of strings containing the blocks to search. Must not exceed 50 block ids.</param>
	    /// <returns>A Task that resolves to a List of FlowEventGroups</returns>
        public static async Task<List<FlowEventGroup>> GetForBlockIds(string type, List<string> blockIds)
        {
			try
			{
				// The Flow HTTP API allows a maximum of 50 block ids. 
				// https://developers.flow.com/http-api/#tag/Events
				if (blockIds.Count > 50)
                {
					throw new FlowException("Exceeded maximum block ids of 50.");
                }

				return await NetworkClient.GetClient().GetEventsForBlockIds(type, blockIds);
			}
			catch (Exception ex)
			{
				return new List<FlowEventGroup>
				{
					new FlowEventGroup
					{
						Error = new FlowError($"Events GetForBlockIds failed, type: {type}. {ex.Message}", ex)
					}
				};
			}
		}
    }
}
