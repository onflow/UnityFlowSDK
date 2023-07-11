using System.Collections.Generic;
using DapperLabs.Flow.Sdk.Cadence;

namespace DapperLabs.Flow.Sdk.DataObjects
{
    public class FlowScriptRequest
    {
        public string Script;
        public List<CadenceBase> Arguments = new List<CadenceBase>();

        public void AddArgument(CadenceBase arg)
        {
            Arguments.Add(arg);
        }
    }
}