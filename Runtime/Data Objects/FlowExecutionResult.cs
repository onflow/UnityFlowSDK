using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.DataObjects
{
    public class FlowExecutionResult
    {
        public string PreviousResultId;
        public string BlockId;
        public List<FlowChunk> Chunks;
        public List<FlowServiceEvent> ServiceEvents;
        public Exceptions.FlowError Error;
    }
}