using DapperLabs.Flow.Sdk.Cadence;

namespace DapperLabs.Flow.Sdk.DataObjects
{
    public class FlowEvent
    {
        public string Type;
        public string TransactionId;
        public uint TransactionIndex;
        public uint EventIndex;
        public CadenceBase Payload;
    }
}
