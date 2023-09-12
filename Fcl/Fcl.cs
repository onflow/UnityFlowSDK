using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.Crypto;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Unity;
using Fcl.Net.Core;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Service.Strategies;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.Fcl
{
    public class FclProvider : IWallet
    {
        global::Fcl.Net.Core.Fcl fcl = null;
        private static readonly HttpClient _httpClient = new();

        public async Task Authenticate(string username, Action<string> OnAuthSuccess, Action OnAuthFailed)
        {
            UnityEngine.Object prefab = Resources.Load("WalletSelectDialogPrefab");
            var walletSelectDialog = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;

            try
            {
                var serviceProviders = await fcl.DiscoveryServicesAsync();

                Debug.Log($"Fcl: discovery service returned {serviceProviders.Count} providers.");
                var providers = new List<FclWalletProvider>();

                foreach (FclService service in serviceProviders)
                {
                    if (service.Method != FclServiceMethod.Data && service.Provider.Name != "WalletConnect")
                    {
                        Debug.Log($"{service.Provider.Name}. Method: {service.Method}. Endpoint: {service.Endpoint}. uid: {service.Uid}");

                        providers.Add(new FclWalletProvider
                        {
                            Name = service.Provider.Name,
                            Logo = service.Provider.Icon,
                            Method = service.Method,
                            Endpoint = service.Endpoint,
                            Uid = service.Uid
                        });
                    }
                }

                WalletSelectDialog walletSelectDialogScript = walletSelectDialog.GetComponentInChildren<WalletSelectDialog>();

                bool initSuccess = walletSelectDialogScript.Init("Select Wallet", providers, async (FclServiceMethod method, string endpoint, string uid) =>
                {
                    try
                    {
                        await fcl.AuthenticateAsync(new FclService
                        {
                            Endpoint = endpoint,
                            Method = method,
                            Uid = uid
                        });

                        if (fcl.User.LoggedIn)
                        {
                            OnAuthSuccess(fcl.User.Address);
                        }
                        else
                        {
                            OnAuthFailed();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Fcl: Authenticate - {ex.Message}.", ex);
                    }
                    finally
                    {
                        UnityEngine.Object.Destroy(walletSelectDialog);
                    }
                });

            }
            catch (Exception ex)
            {
                throw new Exception($"Fcl: Authenticate - {ex.Message}.", ex);
            }
        }

        public SdkAccount GetAuthenticatedAccount()
        {
            if (IsAuthenticated())
            {
                return new SdkAccount
                {
                    Address = fcl.User.Address
                };
            }

            return null;
        }

        public void Init(WalletConfig config)
        {
            try
            {
                if (config == null)
                {
                    throw new Exception("Fcl: Init() - must pass a valid FclConfig object.");
                }

                FclConfig fclConfig = null;

                if (config is FclConfig)
                {
                    fclConfig = config as FclConfig;
                }
                else
                {
                    throw new Exception("Fcl: Init() - Incorrect config type given. Config type must be FclConfig.");
                }

                var appInfo = new FclAppInfo
                {
                    Icon = new Uri(fclConfig.IconUri),
                    Title = fclConfig.Title
                };

                var walletDiscoveryConfig = new FclWalletDiscovery
                {
                    Authn = new Uri("https://fcl-discovery.onflow.org/api/testnet/authn")
                };

                global::Fcl.Net.Core.Config.FclConfig cfg = new global::Fcl.Net.Core.Config.FclConfig(walletDiscoveryConfig, appInfo, "", ChainId.Testnet);

                var sdkOptions = new FlowClientOptions
                {
                    ServerUrl = ServerUrl.TestnetHost
                };

                var fetchServiceConfig = new FetchServiceConfig
                {
                    Location = fclConfig.Location
                };

                var fetchService = new FetchService(_httpClient, fetchServiceConfig);

                var wcConfig = new WalletConnectConfig
                {
                    ProjectDescription = fclConfig.Description,
                    ProjectIconUrl = fclConfig.IconUri,
                    ProjectId = fclConfig.WalletConnectProjectId,
                    ProjectName = fclConfig.Title,
                    ProjectUrl = fclConfig.Url,
                    QrCodeDialogPrefab = fclConfig.WalletConnectQrCodeDialogPrefab,
                    WalletSelectDialogPrefab = null
                };

                // strategies
#if UNITY_IOS || UNITY_ANDROID
                var strategies = new Dictionary<FclServiceMethod, IStrategy>
                {
                    { FclServiceMethod.WcRpc, new UnityWalletConnectStrategy(fetchService, wcConfig) },
                    { FclServiceMethod.Data, new DataStrategy(fetchService) }
                };
#else
                var strategies = new Dictionary<FclServiceMethod, IStrategy>
                {
                    { FclServiceMethod.HttpPost, new UnityHttpPostStrategy(fetchService, null) },
                    { FclServiceMethod.WcRpc, new UnityWalletConnectStrategy(fetchService, wcConfig) },
                    { FclServiceMethod.Data, new DataStrategy(fetchService) }
                };
#endif

                fcl = new global::Fcl.Net.Core.Fcl(
                    cfg,
                    new FlowHttpClient(_httpClient, sdkOptions),
                    new UnityPlatform(),
                    strategies);

                UnityThreadExecutor.Init();
            }
            catch (Exception ex)
            {
                throw new Exception("Fcl: Init() - failed to initialize FCL.NET.", ex);
            }
        }

        public bool IsAuthenticated()
        {
            return fcl != null && fcl.User != null && fcl.User.LoggedIn;
        }

        public async Task<byte[]> SignTransactionEnvelope(FlowTransaction txn)
        {
            try
            {
                byte[] canonicalAuthorizationEnvelope = Rlp.EncodedCanonicalAuthorizationEnvelope(txn);
                byte[] message = DomainTag.AddTransactionDomainTag(canonicalAuthorizationEnvelope);

                string str = Encoding.UTF8.GetString(message);
                var res = await fcl.SignUserMessageAsync(str);

                return Encoding.UTF8.GetBytes(res.Signature);
            }
            catch (Exception ex)
            {
                throw new Exception("Fcl: failed to sign transaction envelope.", ex);
            }
        }

        public Task<byte[]> SignTransactionPayload(FlowTransaction txn)
        {
            throw new NotImplementedException();
        }

        public void Unauthenticate()
        {
            if (IsAuthenticated())
            {
                try
                {
                    fcl.Unauthenticate();
                }
                catch (Exception ex)
                {
                    throw new Exception("Fcl: failed to unauthenticate FLC.NET.", ex);
                }
            }
        }

        public async Task<FlowTransactionResponse> Mutate(string script, List<CadenceBase> arguments = null)
        {
            try
            {
                var tx = new FclMutation
                {
                    Script = script
                };

                if (arguments != null)
                {
                    tx.Arguments = arguments.ToFclCadenceList();
                }

                var transactionId = await fcl.MutateAsync(tx);

                return new FlowTransactionResponse
                {
                    Id = transactionId
                };
            }
            catch (Exception ex)
            {
                string msg = $"FclProvider: Exception thrown calling Mutate: {ex.Message}. ";
                if (ex.InnerException != null)
                {
                    msg += $"{ex.InnerException.Message}. ";

                    if (ex.InnerException.InnerException != null)
                    {
                        msg += $"{ex.InnerException.InnerException.Message}. ";
                    }
                }

                return new FlowTransactionResponse
                {
                    Error = new FlowError(msg, ex)
                };
            }
        }
    }
}
