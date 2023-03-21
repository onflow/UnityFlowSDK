using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.DataObjects;

namespace DapperLabs.Flow.Sdk.Network
{
    internal abstract class NetworkClient
    {
        private static NetworkClient Client = null;

        internal static void Init(ref FlowConfig config)
        {
            switch (config.Protocol)
            {
                case FlowConfig.NetworkProtocol.HTTP:
                    Client = new HttpClient(config.NetworkUrl);
                    break;
            }
        }

        internal static NetworkClient GetClient()
        {
            return Client;
        }

        internal abstract Task<FlowBlock> GetBlockById(string id);

        internal abstract Task<FlowBlock> GetBlockByHeight(ulong height);

        internal abstract Task<FlowBlock> GetLatestBlock(bool isSealed);

        internal abstract Task<FlowCollection> GetCollectionById(string id);

        internal abstract Task<List<FlowEventGroup>> GetEventsForHeightRange(string type, ulong startHeight, ulong endHeight);

        internal abstract Task<List<FlowEventGroup>> GetEventsForBlockIds(string type, List<string> blockIds);

        internal abstract Task<FlowScriptResponse> ExecuteScriptAtLatestBlock(FlowScriptRequest scriptRequest);

        internal abstract Task<FlowScriptResponse> ExecuteScriptAtBlockId(FlowScriptRequest scriptRequest, string id);

        internal abstract Task<FlowScriptResponse> ExecuteScriptAtBlockHeight(FlowScriptRequest scriptRequest, ulong height);

        internal abstract Task<FlowTransactionResponse> SubmitTransaction(FlowTransaction txRequest);

        internal abstract Task<FlowTransaction> GetTransactionById(string txId);

        internal abstract Task<FlowTransactionResult> GetTransactionResult(string txId);

        internal abstract Task<FlowAccount> GetAccountByAddress(string address);

        internal abstract Task<FlowExecutionResult> GetExecutionResultForBlockId(string blockId);
    }
}