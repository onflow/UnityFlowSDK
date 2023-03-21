using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.DataObjects
{
    public class FlowTransactionResult
    {
        public FlowTransactionStatus Status;
        public uint StatusCode;
        public string ErrorMessage;
        public List<FlowEvent> Events;
        public Exceptions.FlowError Error;
    }
}