using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.Fcl
{
    public class UnityPlatform : IPlatform
    {
        async Task<ICollection<FclService>> IPlatform.GetClientServices()
        {
            Debug.Log("UnityPlatform GetClientServices");
            return new List<FclService>();
        }
    }
}
