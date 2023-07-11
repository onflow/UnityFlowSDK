using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.Crypto;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;


namespace DapperLabs.Flow.Sdk.Niftory
{
    public class NiftoryProvider : IWallet
    {
        private NiftoryConfig _config = null;
        private static readonly HttpClient _httpClient = new();

        private Token _token = null;
        private string _flowAddress;

        private GameObject qrDialog = null;

        private static int SequenceNumberRecoveryDelay = 15000;
        private static int SequenceNumberRecoveryAttempts = 3;

        private bool _killSessionTask = false;

        public async Task Authenticate(string username, Action<string> OnAuthSuccess, Action OnAuthFailed)
        {
            Debug.Log("Niftory: Authenticate() - Begin");

            _killSessionTask = false;

            if (_config == null)
            {
                Debug.LogError("Niftory: Authenticate() - Must call Init before Authenticate.");

                OnAuthFailed();
                return;
            }

            // Set up user facing dialog - Try to use user defined QR Code prefab
            UnityEngine.Object prefab = _config.QrCodeDialogPrefab as UnityEngine.Object;
            if (prefab == null)
            {
                // Load default QR Code Dialog prefab
                Debug.Log("Niftory: Authenticate() - <b>QrCodeDialogPrefab</b> not assigned in NiftoryConfig, using default dialog prefab.");
                prefab = Resources.Load("QRCodeDialogPrefab_Niftory");
            }

            // Instantiate the prefab
            if (qrDialog == null)
            {
                qrDialog = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            }

            // Get the script component and initialize it
            QRCodeDialog qrDialogScript = qrDialog.GetComponentInChildren<QRCodeDialog>();
            if (qrDialogScript == null)
            {
                Debug.LogError($"Niftory: Authenticate() - <b>QrCodeDialog</b> component missing on {prefab.name}. Unable to render QR code.");
                
                GameObject.Destroy(qrDialog);
                qrDialog = null;

                OnAuthFailed();

                return;
            }

            // Attempt to recover session using a stored refresh token
            string storedRefreshToken = PlayerPrefs.GetString("NiftoryProviderRefreshToken", "");
            if (storedRefreshToken != "")
            {
                try
                {
                    Debug.Log("Niftory: Authenticate() - Attempting to refresh bearer token...");

                    var success = await RefreshToken(storedRefreshToken);

                    if (success)
                    {
                        _flowAddress = await GetWalletAsync();

                        UnityEngine.Object.Destroy(qrDialog);
                        qrDialog = null;

                        OnAuthSuccess(_flowAddress);

                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(new Exception("Niftory: Authenticate() - Refresh Token failed.", ex));
                }
            }

            try
            {
                Debug.Log("Niftory: Authenticate() - Attempting clean auth...");

                // Stored refresh token was unsuccessful, Acquire new user auth.
                var scope = "openid email profile offline_access";

                var postData = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("client_id", _config.ClientId),
                    new KeyValuePair<string, string>("scope", scope),
                    new KeyValuePair<string, string>("prompt", "consent")
                };

                HttpContent content = new FormUrlEncodedContent(postData);

                var response = await _httpClient.PostAsync($"{_config.AuthUrl}/oidc/device/auth", content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                //Debug.Log($"Niftory: Authenticate() - Auth responseBody: {responseBody}");

                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseBody);

                // Set up QR code dialog
                bool initSuccess = qrDialogScript.Init(authResponse.VerificationUriComplete, authResponse.UserCode, () =>
                {
                    Debug.Log("Niftory: Authenticate() - Dialog closed by user, stopping session connection.");
                    _killSessionTask = true;
                });

                if (initSuccess == false)
                {
                    UnityEngine.Object.Destroy(qrDialog);
                    qrDialog = null;
                    return;
                }

                await TokenPoller(authResponse);
                PlayerPrefs.SetString("NiftoryProviderRefreshToken", _token.RefreshToken);

                if (_killSessionTask)
                {
                    OnAuthFailed();
                    return;
                }

                // update dialog - set to loading indicator
                qrDialogScript.Dialog.SetActive(false);
                qrDialogScript.LoadingIndicator.SetActive(true);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.IdToken);

                _flowAddress = await GetWalletAsync();

                UnityEngine.Object.Destroy(qrDialog);
                qrDialog = null;

                OnAuthSuccess(_flowAddress);
            }
            catch (Exception ex)
            {
                UnityEngine.Object.Destroy(qrDialog);
                qrDialog = null;

                _killSessionTask = true;

                OnAuthFailed();

                Debug.LogException(new Exception("Niftory: Authenticate() - Clean Auth failed.", ex));
            }
        }

        private async Task<T> SendGraphQLRequest<T>(string query, string? operationName = null, IDictionary<string, object> vars = null)
        {
            GrpahQLRequest req = new GrpahQLRequest
            {
                Query = query,
                OperationName = operationName,
                Variables = vars
            };

            string json = JsonConvert.SerializeObject(req);
            //Debug.Log($"GraphQL request: {json}");
            
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_config.GraphQLUrl}", content);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // bearer token has likely expired - attempt to refresh
                bool success = await RefreshToken(_token.RefreshToken);
                if (success)
                {
                    // retry request
                    return await SendGraphQLRequest<T>(query, operationName, vars);
                }
                else
                {
                    throw new Exception($"Niftory: SendGraphQLRequest() - Token expired. Could not automatically refresh token.");
                }
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            //Debug.Log($"GraphQL responseBody: {responseBody}");

            T output = JsonConvert.DeserializeObject<T>(responseBody);

            return output;
        }

        private async Task<string> GetWalletAsync()
        {
            Debug.Log("Niftory: Obtaining Wallet for User...");

            string createWalletQuery = @"
mutation createNiftoryWallet {
    createNiftoryWallet {
        id
        address
    }
}
";
            var wallet = await SendGraphQLRequest<NiftoryWallet>(createWalletQuery);

            if (wallet.Address == null)
            {
                return await WalletPoller();
            }
            else
            {
                return wallet.Address;
            }
        }

        private async Task<string> WalletPoller()
        {
            while (_killSessionTask == false)
            {
                Debug.Log("Niftory: Polling wallet graphQL endpoint...");

                string query = @"
query wallet {
    wallet {
        address
        id
    }
}
";
                var wallet = await SendGraphQLRequest<NiftoryWalletResponse>(query);

                if (wallet.Data != null && wallet.Data.Wallet != null && wallet.Data.Wallet.Address != null)
                {
                    Debug.Log($"Niftory: Obtained Wallet - Address: {wallet.Data.Wallet.Address}");
                    return wallet.Data.Wallet.Address;
                }

                await Task.Delay(5000);
            }

            return "";
        }

        private async Task TokenPoller(AuthResponse authResponse)
        {
            while (_killSessionTask == false)
            {
                await Task.Delay(5000);

                Debug.Log("Niftory: Authenticate() - Polling token endpoint...");

                var postData = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("client_id", _config.ClientId),
                    new KeyValuePair<string, string>("device_code", authResponse.DeviceCode),
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code")
                };

                HttpContent content = new FormUrlEncodedContent(postData);

                var response = await _httpClient.PostAsync($"{_config.AuthUrl}/oidc/token", content);
                
                string responseBody = await response.Content.ReadAsStringAsync();
                //Debug.Log($"Niftory: Authenticate() - token responseBody: {responseBody}");

                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                if (tokenResponse.IdToken != null)
                {
                    _token = new Token
                    {
                        IdToken = tokenResponse.IdToken,
                        RefreshToken = tokenResponse.RefreshToken
                    };

                    break;
                }

                // authorization_pending just means the user is still signing in
                if (tokenResponse.Error != null && tokenResponse.Error != "authorization_pending")
                {
                    throw new Exception($"Niftory: Poller() - Error from token endpoint: {tokenResponse.Error} - {tokenResponse.ErrorDescription}");
                }
            }

            Debug.Log($"Niftory: Id Token: {_token.IdToken}");
        }

        private async Task<bool> RefreshToken(string refreshToken)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            var postData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", _config.ClientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
            };

            HttpContent content = new FormUrlEncodedContent(postData);

            var response = await _httpClient.PostAsync($"{_config.AuthUrl}/oidc/token", content);

            string responseBody = await response.Content.ReadAsStringAsync();
            //Debug.Log($"Niftory: Token responseBody: {responseBody}");

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

            _token = new Token
            {
                IdToken = tokenResponse.IdToken,
                RefreshToken = tokenResponse.RefreshToken
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.IdToken);
            PlayerPrefs.SetString("NiftoryProviderRefreshToken", _token.RefreshToken);

            Debug.Log($"Niftory: Refreshed Id Token: {_token.IdToken}");

            return !string.IsNullOrEmpty(_token.IdToken);
        }


        public SdkAccount GetAuthenticatedAccount()
        {
            if (_flowAddress != "")
            {
                return new SdkAccount
                {
                    Address = _flowAddress
                };
            }

            return null;
        }

        public void Init(WalletConfig config)
        {
            if (config == null)
            {
                throw new Exception("Niftory: Init() - Must pass a valid NiftoryConfig object.");
            }

            if (config is NiftoryConfig)
            {
                _config = config as NiftoryConfig;
            }
            else
            {
                throw new Exception("Niftory: Init() - Incorrect config type given. Config type must be NiftoryConfig.");
            }
        }

        public bool IsAuthenticated()
        {
            return _token != null && _flowAddress != "";
        }

        public async Task<FlowTransactionResponse> Mutate(string script, List<CadenceBase> arguments = null)
        {
            try
            {
                return await MutateAsync(script, arguments, 0);
            }
            catch (Exception ex)
            {
                return new FlowTransactionResponse
                {
                    Error = new FlowError($"NiftoryProvider: Exception thrown calling Mutate: {ex.Message}", ex)
                };
            }
        }

        private async Task<FlowTransactionResponse> MutateAsync(string script, List<CadenceBase> arguments = null, int resendAttempt = 0)
        {
            arguments = arguments ?? new List<CadenceBase>();
            Dictionary<string, object> args = SerialiseArguments(script, arguments);

            Dictionary<string, object> vars = new Dictionary<string, object>
            {
                ["address"] = _flowAddress,
                ["transaction"] = script,
                ["args"] = args
            };

            string executeTransactionQuery = @"
mutation executeTransaction($address: String!, $transaction: String!, $args: JSONObject, $name: String) {
executeTransaction(address: $address, transaction: $transaction, args: $args, name: $name) {
    id,
    hash,
    state,
    result
}
}
";
            NiftoryTransactionResponse result = await SendGraphQLRequest<NiftoryTransactionResponse>(executeTransactionQuery, null, vars);
            if (result.Errors != null && result.Errors.Length > 0)
            {
                return new FlowTransactionResponse
                {
                    Error = new FlowError(string.Join("\n", result.Errors.Select(e => e.Message)))
                };
            }

            // Check if transaction was ok
            FlowTransactionResult txResult = await Transactions.GetResult(result.Data.Transaction.Hash);
            while (txResult.Status < FlowTransactionStatus.FINALIZED)
            {
                await Task.Delay(500);
                txResult = await Transactions.GetResult(result.Data.Transaction.Hash);
            }

            // Error code 1007 indicates a sequence number mismatch occurred
            if (txResult.ErrorMessage.Contains("[Error Code: 1007]"))
            {
                Debug.Log($"NiftoryProvider: Error 1007 - checking sequence number failed. Waiting and resubmitting - attempt: {resendAttempt + 1}");

                if (resendAttempt < SequenceNumberRecoveryAttempts)
                {
                    // wait for SequenceNumberRecoveryDelay ms, then resubmit
                    await Task.Delay(SequenceNumberRecoveryDelay);

                    return await MutateAsync(script, arguments, resendAttempt + 1);
                }
            }

            return new FlowTransactionResponse
            {
                Id = result.Data.Transaction.Hash,
                Error = txResult.Error
            };
        }

        private Dictionary<string, object> SerialiseArguments(string script, List<CadenceBase> arguments)
        {
            // remove comments
            string blockComments = @"\/\*[\s\S]*?\*\/";
            string lineComments = @"\/\/.*";

            string cleanedScript = Regex.Replace(script, blockComments, "");
            cleanedScript = Regex.Replace(cleanedScript, lineComments, "");

            // get transaction arguments
            string transactionLine = @"(?<=transaction*[ \t]*\()(.*?)(?=\))";
            string args = Regex.Match(cleanedScript, transactionLine).Value;

            string transactionArguments = @"(?<=(,|^)[ \t]*)\w+(?=[ \t]*:)";
            var matches = Regex.Matches(args, transactionArguments);

            // handle input arg and script arg mismatch
            if (matches.Count != arguments.Count)
            {
                throw new Exception($"Niftory: SerialiseArguments() - Argument count does not match parsed transaction arguments: {string.Join(",", matches.Select(m => m.Value))}");
            }

            Dictionary<string, object> output = new Dictionary<string, object>();

            for (int i = 0; i < matches.Count; i++)
            {
                output.Add(matches[i].Value, arguments[i].ToNiftoryObject());
            }

            return output;
        }

        public Task<byte[]> SignTransactionEnvelope(FlowTransaction txn)
        {
            throw new NotImplementedException("Niftory Provider currently only supports custodial wallets. Signing must be performed server side by the wallet provider. " +
                "Only use Mutate until signing has been implemented in a future version of this provider.");
        }

        public Task<byte[]> SignTransactionPayload(FlowTransaction txn)
        {
            throw new NotImplementedException("Niftory Provider currently only supports custodial wallets. Signing must be performed server side by the wallet provider. " +
                "Only use Mutate until signing has been implemented in a future version of this provider.");
        }

        public void Unauthenticate()
        {
            _token = null;
            _flowAddress = "";

            PlayerPrefs.SetString("NiftoryProviderRefreshToken", "");

            // clear client to reset headers
            _httpClient.DefaultRequestHeaders.Clear();

            Debug.Log("Niftory: UnAuthenticate() - Authentication Cleared");
        }
    }
}
