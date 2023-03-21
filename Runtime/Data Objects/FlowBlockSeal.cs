using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.DataObjects
{
    public class FlowBlockSeal
    {
        public string BlockId;
        public string ExecutionReceiptId;
        public List<string> ExecutionReceiptSignatures;
        public List<string> ResultApprovalSignatures;
    }
}