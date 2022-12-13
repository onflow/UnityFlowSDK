using System;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Network;

namespace DapperLabs.Flow.Sdk
{
	/// <summary>
	/// Collection related functions
	/// </summary>
    public class Collections
    {
	    /// <summary>
	    /// Get a collection by it's ID.
	    /// </summary>
	    /// <param name="id">The ID of the collection to fetch</param>
	    /// <returns>A Task that resolves to a FlowCollection with the given ID</returns>
        public static async Task<FlowCollection> GetById(string id)
        {
			try
			{
				return await NetworkClient.GetClient().GetCollectionById(id);
			}
			catch (Exception ex)
			{
				return new FlowCollection
				{
					Error = new FlowError($"Collections GetById failed, id: {id}. {ex.Message}", ex)
				};
			}
		}
    }
}
