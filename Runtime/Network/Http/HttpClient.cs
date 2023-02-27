using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Exceptions;
using Newtonsoft.Json;
using System;
using System.Text;
using DapperLabs.Flow.Sdk.Cadence;

namespace DapperLabs.Flow.Sdk.Network
{
    internal class HttpClient : NetworkClient
    {
        private readonly System.Net.Http.HttpClient client;
        private readonly string url;

        internal HttpClient(string url)
        {
            client = new System.Net.Http.HttpClient();
            this.url = url;
        }

        internal async override Task<FlowScriptResponse> ExecuteScriptAtLatestBlock(FlowScriptRequest scriptRequest)
        {
            try
            {
                HttpScriptRequest request = scriptRequest.ToHttpScriptRequest();
                string json = JsonConvert.SerializeObject(request);
                StringContent body = new StringContent(json);

                HttpResponseMessage response = await client.PostAsync($"{url}/scripts", body);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                responseBody = responseBody.Replace("\"", "");
                byte[] responseBytes = System.Convert.FromBase64String(responseBody);
                string responseJson = Encoding.UTF8.GetString(responseBytes);

                return new FlowScriptResponse
                {
                    Value = JsonConvert.DeserializeObject<CadenceBase>(responseJson, new CadenceCreationConverter())
                };
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"ExecuteScriptAtLatestBlock request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowScriptResponse> ExecuteScriptAtBlockId(FlowScriptRequest scriptRequest, string id)
        {
            try
            {
                HttpScriptRequest request = scriptRequest.ToHttpScriptRequest();
                string json = JsonConvert.SerializeObject(request);
                StringContent body = new StringContent(json);

                HttpResponseMessage response = await client.PostAsync($"{url}/scripts?block_id={id}", body);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                responseBody = responseBody.Replace("\"", "");
                byte[] responseBytes = System.Convert.FromBase64String(responseBody);
                string responseJson = Encoding.UTF8.GetString(responseBytes);

                return new FlowScriptResponse
                {
                    Value = JsonConvert.DeserializeObject<CadenceBase>(responseJson, new CadenceCreationConverter())
                };
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"ExecuteScriptAtBlockId request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowScriptResponse> ExecuteScriptAtBlockHeight(FlowScriptRequest scriptRequest, ulong height)
        {
            try
            {
                HttpScriptRequest request = scriptRequest.ToHttpScriptRequest();
                string json = JsonConvert.SerializeObject(request);
                StringContent body = new StringContent(json);

                HttpResponseMessage response = await client.PostAsync($"{url}/scripts?block_height={height}", body);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                responseBody = responseBody.Replace("\"", "");
                byte[] responseBytes = System.Convert.FromBase64String(responseBody);
                string responseJson = Encoding.UTF8.GetString(responseBytes);

                return new FlowScriptResponse
                {
                    Value = JsonConvert.DeserializeObject<CadenceBase>(responseJson, new CadenceCreationConverter())
                };
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"ExecuteScriptAtBlockHeight request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowAccount> GetAccountByAddress(string address)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{url}/accounts/{address}?expand=keys,contracts");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpAccount account = JsonConvert.DeserializeObject<HttpAccount>(responseBody);

                return account.ToFlowAccount();
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetAccountByAddress request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowBlock> GetBlockByHeight(ulong height)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{url}/blocks?height={height}&expand=payload");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpBlock[] httpBlocks = JsonConvert.DeserializeObject<HttpBlock[]>(responseBody);

                List<FlowBlock> blocks = httpBlocks.ToFlowBlocks();

                if (blocks.Count < 1)
                {
                    throw new FlowException($"No block available at height {height}.");
                }

                return blocks[0];

            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetBlockByHeight request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowBlock> GetBlockById(string id)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{url}/blocks/{id}?expand=payload");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpBlock[] httpBlocks = JsonConvert.DeserializeObject<HttpBlock[]>(responseBody);

                List<FlowBlock> blocks = httpBlocks.ToFlowBlocks();

                if (blocks.Count < 1)
                {
                    throw new FlowException($"No block with id {id}.");
                }

                return blocks[0];

            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetBlockById request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowCollection> GetCollectionById(string id)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{url}/collections/{id}?expand=transactions");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpCollection collection = JsonConvert.DeserializeObject<HttpCollection>(responseBody);

                return collection.ToFlowCollection();
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetCollectionById request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<List<FlowEventGroup>> GetEventsForBlockIds(string type, List<string> blockIds)
        {
            try
            {
                string blockIdStr = "";
                foreach (string blockId in blockIds)
                {
                    blockIdStr += blockId;
                    blockIdStr += ",";
                }
                blockIdStr = blockIdStr.Substring(0, blockIdStr.Length - 1);

                HttpResponseMessage response = await client.GetAsync($"{url}/events?type={type}&block_ids={blockIdStr}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpEventGroup[] ev = JsonConvert.DeserializeObject<HttpEventGroup[]>(responseBody);

                return ev.ToFlowEventGroups();
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetEventsForBlockIds request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<List<FlowEventGroup>> GetEventsForHeightRange(string type, ulong startHeight, ulong endHeight)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{url}/events?type={type}&start_height={startHeight}&end_height={endHeight}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpEventGroup[] ev = JsonConvert.DeserializeObject<HttpEventGroup[]>(responseBody);

                return ev.ToFlowEventGroups();
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetEventsForHeightRange request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowBlock> GetLatestBlock(bool isSealed = true)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{url}/blocks?height={(isSealed ? "sealed" : "final")}&expand=payload");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpBlock[] httpBlocks = JsonConvert.DeserializeObject<HttpBlock[]>(responseBody);

                List<FlowBlock> blocks = httpBlocks.ToFlowBlocks();

                if (blocks.Count < 1)
                {
                    throw new FlowException($"Could not get latest {(isSealed ? "sealed" : "final")} block.");
                }

                return blocks[0];

            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetLatestBlock request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowTransaction> GetTransactionById(string txId)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{url}/transactions/{txId}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpTransaction tx = JsonConvert.DeserializeObject<HttpTransaction>(responseBody);

                return tx.ToFlowTransaction();
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetTransactionById request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowTransactionResult> GetTransactionResult(string txId)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{url}/transaction_results/{txId}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpTransactionResult tx = JsonConvert.DeserializeObject<HttpTransactionResult>(responseBody);

                return tx.ToFlowTransactionResult();
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetTransactionResult request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowTransactionResponse> SubmitTransaction(FlowTransaction txRequest)
        {
            try
            {
                HttpTransactionRequest tx = txRequest.ToHttpTransactionRequest();
                string json = JsonConvert.SerializeObject(tx);
                StringContent body = new StringContent(json);

                HttpResponseMessage response = await client.PostAsync($"{url}/transactions", body);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpTransaction responseJson = JsonConvert.DeserializeObject<HttpTransaction>(responseBody);

                return new FlowTransactionResponse
                {
                    Id = responseJson.id
                };
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"SubmitTransaction request failed. {ex.Message}", ex);
            }
        }

        internal async override Task<FlowExecutionResult> GetExecutionResultForBlockId(string blockId)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{url}/execution_results?block_id={blockId}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                HttpExecutionResult result = JsonConvert.DeserializeObject<HttpExecutionResult>(responseBody);

                return result.ToFlowExecutionResult();
            }
            catch (HttpRequestException ex)
            {
                throw new FlowException($"GetExecutionResultForBlockId request failed. {ex.Message}", ex);
            }
        }
    }
}
