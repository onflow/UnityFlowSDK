using System.Collections.Generic;
using System.Threading.Tasks;
using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;

namespace DapperLabs.Flow.Sdk.Fcl
{
    /// <summary>
    /// Unity implementation of IPlatform. 
    /// </summary>
    public class UnityPlatform : IPlatform
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task<ICollection<FclService>> IPlatform.GetClientServices()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new List<FclService>();
        }
    }
}
