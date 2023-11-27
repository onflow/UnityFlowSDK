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
    /// <summary>
    /// This wallet provider implements the FCL (Flow Client Library) specification, which can be viewed here:
    /// https://github.com/onflow/flips/blob/main/application/20221108-fcl-specification.md
    /// It connects to Flow's Discovery Service to determine which wallets are supported. 
    /// </summary>
    public class FclProvider : IWallet
    {
        global::Fcl.Net.Core.Fcl fcl = null;
        private static readonly HttpClient _httpClient = new();

        /// <summary>
        /// Connects the app to the user's wallet, obtaining their Flow address. 
        /// This calls the Discovery Service endpoint, which returns a list of supported wallet providers. These
        /// are then displayed to the user to choose from. 
        /// </summary>
        /// <param name="username">Ignored for FCL.</param>
        /// <param name="OnAuthSuccess">Callback for when the user connects a wallet. Their Flow address is passed in as a string.</param>
        /// <param name="OnAuthFailed">Callback if an error occurred.</param>
        /// <returns>An async Task. This function can be awaited.</returns>
        public async Task Authenticate(string username, Action<string> OnAuthSuccess, Action OnAuthFailed)
        {
            UnityEngine.Object prefab = Resources.Load("WalletSelectDialogPrefab_FCL");
            var walletSelectDialog = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;

            try
            {
                var serviceProviders = await fcl.DiscoveryServicesAsync();

                var providers = new List<FclWalletProvider>();

                foreach (FclService service in serviceProviders)
                {
                    if (service.Method != FclServiceMethod.Data && service.Provider.Name != "WalletConnect")
                    {
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

        /// <summary>
        /// Retrieves the account associated with the FCL connected wallet.
        /// </summary>
        /// <returns>An SdkAccount object containing the authenticated Flow address, or null if there's no auth.</returns>
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

        /// <summary>
        /// Initializes the FCL provider. Must be called before calling anything else. 
        /// </summary>
        /// <param name="config">Reference to a FclConfig object containing config for FCL.</param>
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

        /// <summary>
        /// Checks if the user is authenticated with FCL. 
        /// </summary>
        /// <returns>True if the user is authenticated.</returns>
        public bool IsAuthenticated()
        {
            return fcl != null && fcl.User != null && fcl.User.LoggedIn;
        }

        /// <summary>
        /// Requests FCL to sign a transaction envelope. The request will appear in
        /// the user's connected wallet. 
        /// </summary>
        /// <param name="txn">The transaction to be signed.</param>
        /// <returns>The signature in bytes.</returns>
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

        /// <summary>
        /// Not yet implemented - stubbed for future use. 
        /// </summary>
        /// <param name="txn">The transaction to be signed.</param>
        /// <returns>The signature in bytes.</returns>
        public Task<byte[]> SignTransactionPayload(FlowTransaction txn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unauthenticates FCL. 
        /// </summary>
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

        /// <summary>
        /// Performs a mutate operation on the Flow blockchain via a transaction script. 
        /// </summary>
        /// <param name="script">The transaction script to be executed.</param>
        /// <param name="arguments">The arguments for the transaction script.</param>
        /// <returns></returns>
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
