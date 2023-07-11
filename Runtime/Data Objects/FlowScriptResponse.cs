using DapperLabs.Flow.Sdk.Cadence;

namespace DapperLabs.Flow.Sdk.DataObjects
{
    public class FlowScriptResponse
    {
        public CadenceBase Value { get; set; }
        public Exceptions.FlowError Error { get; set; }
    }
}