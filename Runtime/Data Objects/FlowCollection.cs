using DapperLabs.Flow.Sdk.Exceptions;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.DataObjects
{
    public class FlowCollection
    {
        public string Id;
        public List<string> TransactionIds;
		public FlowError Error;
    }
}
