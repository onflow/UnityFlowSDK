using System;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.DataObjects
{
    public class FlowEventGroup
    {
        public string BlockId;
        public ulong BlockHeight;
        public List<FlowEvent> Events;
        public DateTimeOffset BlockTimestamp;
        public Exceptions.FlowError Error;
    }
}