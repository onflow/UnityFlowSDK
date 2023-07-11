using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using DapperLabs.Flow.Sdk.Crypto;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Unity;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Core.Models.Pairing;

#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.Networking;
#endif

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    [RpcMethod("flow_authz"), RpcResponseOptions(Clock.ONE_MINUTE, false, 99999)]
    internal class TxSignRequest : List<string>
    {
        internal TxSignRequest(IEnumerable<string> collection) : base(collection)
        {
        }
    }

    /// <summary>
    /// This wallet implementation uses Wallet Connect 2.0 to connect the game with 
    /// Flow wallets that also support Wallet Connect 2.0. For more information on 
    /// Wallet Connect 2.0, see https://docs.walletconnect.com/2.0/. 
    /// For documentation on how to use this provider, see 
    /// https://developers.flow.com/tools/unity-sdk/guides/wallet-connect
    /// </summary>
    public class WalletConnectProvider : IWallet
    {
        WalletConnectConfig wcConfig = null;

        WalletConnectSignClient _client = null;
        ConnectedData _connectedData = null;
        SessionStruct _session;

        GameObject qrDialog = null;

        bool _killSessionTask = false;

#if UNITY_ANDROID || UNITY_IOS
        string _currentWalletUrl = "";
#endif

        /// <summary>
        /// Initializes the Wallet Connect provider. Must be called before calling anything else. 
        /// </summary>
        /// <param name="config">Reference to a WalletConnectConfig object containing config for Wallet Connect.</param>
        void IWallet.Init(WalletConfig config)
        {
            if (config == null)
            {
                throw new Exception("Wallet Connect: Init() - must pass a valid WalletConnectConfig object.");
            }

            if (config is WalletConnectConfig)
            {
                wcConfig = config as WalletConnectConfig;
            }
            else
            {
                throw new Exception("Wallet Connect: Init() - Incorrect config type given. Config type must be WalletConnectConfig.");
            }

            UnityThreadExecutor.Init();
        }

        /// <summary>
        /// Connects the app to the user's wallet, obtaining their Flow address. 
        /// On desktop builds, a modal containing a QR code will appear, which the user can scan
        /// with their wallet app. 
        /// On Mobile builds, a list of supported wallets will appear, which the user can select
        /// and deep link to. 
        /// </summary>
        /// <param name="username">Ignored for Wallet Connect.</param>
        /// <param name="OnAuthSuccess">Callback for when the user approves the app connection. Their Flow address is passed in as a string.</param>
        /// <param name="OnAuthFailed">Callback for when the user denied the app connection, or an error occurred.</param>
        /// <returns>An async Task. This function can be awaited.</returns>
        async Task IWallet.Authenticate(string username, Action<string> OnAuthSuccess, Action OnAuthFailed)
        {
            if (_session.Acknowledged != null && (bool)_session.Acknowledged)
            {
                Debug.LogError("Wallet Connect: Already authenticated");
                return;
            }

            if (wcConfig == null)
            {
                Debug.LogError("Wallet Connect: Trying to call Authenticate - call Init() first!");
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            // Try to use user defined Wallet Selection prefab
            UnityEngine.Object prefab = wcConfig.WalletSelectDialogPrefab as UnityEngine.Object;

            if (prefab == null)
            {
                // Load default QR Code Dialog prefab
                Debug.Log("<b>WalletSelectDialogPrefab</b> not assigned in WalletConnectConfig, using default dialog prefab.");
                prefab = Resources.Load("WalletSelectDialogPrefab");
            }

            var walletSelectDialog = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
#else
            // Desktop - Show a QR Code to scan
            // Try to use user defined QR Code prefab
            UnityEngine.Object prefab = wcConfig.QrCodeDialogPrefab as UnityEngine.Object;

            if (prefab == null)
            {
                // Load default QR Code Dialog prefab
                Debug.Log("<b>QrCodeDialogPrefab</b> not assigned in WalletConnectConfig, using default dialog prefab.");
                prefab = Resources.Load("QRCodeDialogPrefab");
            }

            // Instantiate the prefab which shows a list of dev accounts to select from
            if (qrDialog == null)
            {
                qrDialog = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            }
#endif

            if (_client == null)
            {
                var options = new SignClientOptions()
                {
                    ProjectId = wcConfig.ProjectId,
                    Metadata = new Metadata()
                    {
                        Description = wcConfig.ProjectDescription,
                        Icons = new[] { wcConfig.ProjectIconUrl },
                        Name = wcConfig.ProjectName,
                        Url = wcConfig.ProjectUrl,
                    },
                    DataPath = $"{Application.persistentDataPath}/wc/store.json"
                };

                float timeOut = 10.0f;
                Task<WalletConnectSignClient> task = WalletConnectSignClient.Init(options);

                while (task.IsCompleted == false && timeOut > 0.0f)
                {
                    await Task.Delay(500);
                    timeOut -= 0.5f;
                }

                if (timeOut <= 0.0f || task.IsFaulted)
                {
                    if (qrDialog != null)
                    {
                        UnityEngine.Object.Destroy(qrDialog);
                        qrDialog = null;
                    }

                    if (task.IsFaulted)
                    {
                        Debug.Log("Wallet Connect: Exception triggered while initializing.");
                    }
                    else
                    {
                        Debug.Log("Wallet Connect: Authentication initialization timed out");
                    }

                    OnAuthFailed();
                    return;
                }

                _client = task.Result; 
            }

            if (_connectedData == null)
            {
                ConnectOptions connectOptions = new ConnectOptions()
                {
                    RequiredNamespaces = new RequiredNamespaces()
                    {
                        {
                            "flow", new RequiredNamespace()
                            {
                                Methods = new[]
                                {
                                    "flow_authn",
                                    "flow_authz",
                                    "flow_user_sign",
                                },
                                Chains = new[]
                                {
                                    "flow:testnet"
                                },
                                Events = new[]
                                {
                                    "chainChanged", "accountsChanged"
                                }
                            }
                        }
                    }
                };

                float timeOut = 10.0f;
                Task<ConnectedData> task = _client.Connect(connectOptions);

                while (task.IsCompleted == false && timeOut > 0.0f)
                {
                    await Task.Delay(500);
                    timeOut -= 0.5f;
                }

                if (timeOut <= 0.0f || task.IsFaulted)
                {
                    if (qrDialog != null)
                    {
                        UnityEngine.Object.Destroy(qrDialog);
                        qrDialog = null;
                    }

                    if (task.IsFaulted)
                    {
                        Debug.Log("Wallet Connect: Exception triggered while connecting.");
                    }
                    else
                    {
                        Debug.Log("Wallet Connect: Authentication connection timed out");
                    }

                    OnAuthFailed();
                    return;
                }

                _connectedData = task.Result; 
            }

            Debug.Log($"Wallet Connect: connection uri is {_connectedData.Uri}");

#if UNITY_ANDROID
            string[] appsToCheck = new string[2]
            {
                "io.outblock.lilico", // Lilico
                "com.dapperlabs.dapper.tokens.internal" // Dapper SC
            };
#elif UNITY_IOS
            string[] appsToCheck = new string[2]
            {
                "lilico://wc", // Lilico
                "dapper-pro://wc" // Dapper SC
            };
#endif

#if UNITY_ANDROID || UNITY_IOS

            // Mobile - Deep link to a mobile wallet app

            bool[] installedApps = new bool[2];

            var appChecker = new GetAppInfo();
            for (int i = 0; i < appsToCheck.Length; i++)
            {
                if (installedApps[i] = appChecker.CheckInstalledApp(appsToCheck[i]))
                {
                    Debug.Log($"{appsToCheck[i]} is installed");
                }
                else
                {
                    Debug.Log($"{appsToCheck[i]} is not installed");
                }
            }

            // Get the script component and initialize it
            WalletSelectDialog walletSelectDialogScript = walletSelectDialog.GetComponentInChildren<WalletSelectDialog>();

            if (walletSelectDialogScript == null)
            {
                Debug.LogError($"<b>WalletSelectDialog</b> component missing on {prefab.name}. ", prefab);
                return;
            }

            string urlEncoded = UnityWebRequest.EscapeURL(_connectedData.Uri);
            Debug.Log($"urlEncoded: {urlEncoded}");

            // If Dapper SC is installed, include it
            int numProviders = installedApps[1] ? 2 : 1;

            WalletSelectDialog.WalletProviderData[] wcProviders = new WalletSelectDialog.WalletProviderData[numProviders];
            wcProviders[0] = new WalletSelectDialog.WalletProviderData
            {
                Name = "Lilico",
                IsInstalled = installedApps[0],
                Icon = Resources.Load<Texture2D>("WalletSelectIcons/lilicoIcon"),
                BaseUri = "https://link.lilico.app",
                ConnectUri = $"https://link.lilico.app/wc?uri={urlEncoded}"
            };

            if (installedApps[1])
            {
                wcProviders[1] = new WalletSelectDialog.WalletProviderData
                {
                    Name = "Dapper SC",
                    IsInstalled = installedApps[1],
                    Icon = Resources.Load<Texture2D>("WalletSelectIcons/dapperIcon"),
                    BaseUri = "dapper-pro://",
                    ConnectUri = $"dapper-pro://wc?uri={urlEncoded}"
                };
            }

            bool initSuccess = walletSelectDialogScript.Init("Select Wallet", wcProviders, (WalletSelectDialog.WalletProviderData selectedWallet) => 
            {
                Debug.Log($"url: {selectedWallet.ConnectUri}");

                _currentWalletUrl = selectedWallet.BaseUri;
                Application.OpenURL(selectedWallet.ConnectUri);
                UnityEngine.Object.Destroy(walletSelectDialog);
            });

#else
            
            // Get the script component and initialize it
            QRCodeDialog qrDialogScript = qrDialog.GetComponentInChildren<QRCodeDialog>();

            if (qrDialogScript == null)
            {
                Debug.LogError($"<b>QrCodeDialog</b> component missing on {prefab.name}. Unable to render QR code.", prefab);
                return;
            }

            bool initSuccess = qrDialogScript.Init(_connectedData.Uri, () => 
            {
                Debug.Log("Dialog closed, stopping session connection.");
                _killSessionTask = true;
            });

            if (initSuccess == false)
            {
                UnityEngine.Object.Destroy(qrDialog);
                qrDialog = null;
                return;
            }
#endif
            Debug.Log("Waiting for approval...");
            
            Task<SessionStruct> sessionTask = _connectedData.Approval;
            while (sessionTask.IsCompleted == false)
            {
                await Task.Delay(500);
                if (_killSessionTask)
                {
                    OnAuthFailed();
                    _killSessionTask = false;
                    return;
                }
            }

            if (sessionTask.IsFaulted)
            {
                OnAuthFailed();
                return;
            }
            _session = sessionTask.Result;

            Debug.Log($"_session.Topic: {_session.Topic}");

            UnityEngine.Object.Destroy(qrDialog);
            qrDialog = null;

            string accountValue = _session.Namespaces["flow"].Accounts[0];
            string account = accountValue.Split(':')[2];

            OnAuthSuccess(account);
        }

        /// <summary>
        /// Retrieves the account associated with the connected Wallet Connect session.
        /// </summary>
        /// <returns>An SdkAccount object containing the authenticated Flow address, or null if there's no auth.</returns>
        SdkAccount IWallet.GetAuthenticatedAccount()
        {
            if (_session.Acknowledged != null && (bool)_session.Acknowledged)
            {
                string accountValue = _session.Namespaces["flow"].Accounts[0];
                string account = accountValue.Split(':')[2];

                return new SdkAccount
                {
                    Address = account
                };
            }

            return null;
        }

        /// <summary>
        /// Checks if the user is authenticated with Wallet Connect. 
        /// </summary>
        /// <returns>True if the user is authenticated.</returns>
        bool IWallet.IsAuthenticated()
        {
            if (_session.Acknowledged != null)
            {
                return (bool)_session.Acknowledged;
            }
            return false;
        }

        /// <summary>
        /// Requests Wallet Connect to sign a transaction envelope. The request will appear in
        /// the user's connected wallet. 
        /// </summary>
        /// <param name="txn">The transaction to be signed.</param>
        /// <returns>The signature in bytes.</returns>
        async Task<byte[]> IWallet.SignTransactionEnvelope(FlowTransaction txn)
        {
            if (wcConfig == null)
            {
                Debug.LogError("Wallet Connect: Trying to call SignTransactionEnvelope - call Init() first!");
                return null;
            }

            if (FlowSDK.GetWalletProvider().IsAuthenticated() == false)
            {
                Debug.LogError("Wallet Connect: Trying to call SignTransactionEnvelope, but no user is authenticated - call Authenticate() first!");
                return null;
            }

            byte[] canonicalAuthorizationEnvelope = Rlp.EncodedCanonicalAuthorizationEnvelope(txn);
            byte[] message = DomainTag.AddTransactionDomainTag(canonicalAuthorizationEnvelope);

            string accountValue = _session.Namespaces["flow"].Accounts[0];
            string account = accountValue.Split(':')[2];

            string messageHex = BitConverter.ToString(message).Replace("-", "").ToLower();

            var reqParams = BuildTxSignRequestParams(txn, account, messageHex);

            var req = new TxSignRequest(new string[1] {JsonConvert.SerializeObject(reqParams) });

            Debug.Log($"WalletConnect: sign transaction request: {JsonConvert.SerializeObject(req)}");

#if UNITY_ANDROID || UNITY_IOS
            if (_currentWalletUrl != "")
            {
                UnityThreadExecutor.ExecuteInUpdate(() =>
                {
                    Application.OpenURL(_currentWalletUrl);
                });
            }
#endif
            Debug.Log("sending request...");
            var responseReturned = await _client.Engine.Request<TxSignRequest, TxSignResponse>(_session.Topic, req);

            Debug.Log($"WalletConnect: sign transaction response: {JsonConvert.SerializeObject(responseReturned)}");

            if (responseReturned.data != null && responseReturned.data.signature.Length > 0)
            {
                return responseReturned.data.signature.FromHexToBytes();
            }

            throw new Exception("WalletConnect: sign transaction response does not contain a signature.");
        }

        /// <summary>
        /// Not yet implemented - stubbed for future use. 
        /// </summary>
        /// <param name="txn">The transaction to be signed.</param>
        /// <returns>The signature in bytes.</returns>
        Task<byte[]> IWallet.SignTransactionPayload(FlowTransaction txn)
        {
            throw new NotImplementedException("Wallet Connect Provider currently only supports a single signer (ie proposer, authorizer and payer), and therefore " +
                "must sign the envelope. Only use SignTransactionEnvelope until multi-signing has been implemented in a future version of this provider.");
        }

        /// <summary>
        /// Disconnects the app from Wallet Connect and the user's wallet. 
        /// </summary>
        void IWallet.Unauthenticate()
        {
            if (_session.Acknowledged != null && (bool)_session.Acknowledged)
            {
                _client.Disconnect(_session.Topic, new ErrorResponse());

                _connectedData = null;
                _session = new SessionStruct();

#if UNITY_ANDROID || UNITY_IOS
                _currentWalletUrl = "";
#endif
            }
        }

        private string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            string ret = "";
            for (int i = 0; i < length; i++)
            {
                int j = UnityEngine.Random.Range(0, 35);
                ret += chars[j];
            }
            return ret;
        }

        private TxSignRequestParams BuildTxSignRequestParams(FlowTransaction txn, string account, string messageHex)
        {
            var reqParams = new TxSignRequestParams
            {
                FType = "Signable",
                FVsn = "1.0.1",
                Message = messageHex,
                Addr = account,
                KeyId = (int)txn.ProposalKey.KeyId,
                Roles = new TxSignRequestParamRole
                {
                    Proposer = true,
                    Authorizer = true,
                    Payer = true,
                    Param = false
                },
                Cadence = txn.Script,
                Args = new object[txn.Arguments.Count],
                Interaction = new TxSignRequestParamInteraction
                {
                    Tag = "TRANSACTION",
                    Assigns = new Dictionary<string, string>(),
                    Status = "OK",
                    Reason = null,
                    Accounts = new Dictionary<string, TxSignRequestParamAccount>()
                    {
                        {
                            $"{account}-{txn.ProposalKey.KeyId}", new TxSignRequestParamAccount
                            {
                                Kind = "ACCOUNT",
                                TempId = $"{account}-{txn.ProposalKey.KeyId}",
                                Addr = account,
                                KeyId = (int)txn.ProposalKey.KeyId,
                                SequenceNum = (int)txn.ProposalKey.SequenceNumber,
                                Signature = null,
                                Resolve = null,
                                Role = new TxSignRequestParamRole
                                {
                                    Proposer = true,
                                    Authorizer = true,
                                    Payer = true,
                                    Param = false
                                }
                            }
                        }
                    },
                    Params = new Dictionary<string, string>(),
                    Arguments = new Dictionary<string, TxSignRequestParamArgument>(),
                    Message = new TxSignRequestParamMessage
                    {
                        Cadence = txn.Script,
                        RefBlock = txn.ReferenceBlockId,
                        ComputeLimit = (int)txn.GasLimit,
                        Proposer = null,
                        Payer = null,
                        Authorizations = new string[0],
                        Params = new string[0],
                        Arguments = new string[txn.Arguments.Count]
                    },
                    Proposer = $"{account}-{txn.ProposalKey.KeyId}",
                    Authorizations = new string[1] { $"{account}-{txn.ProposalKey.KeyId}" },
                    Payer = new string[1] { $"{account}-{txn.ProposalKey.KeyId}" },
                    Events = new TxSignRequestParamEvent
                    {
                        EventType = null,
                        Start = null,
                        End = null,
                        BlockIds = new string[0]
                    },
                    Transaction = new TxSignRequestParamTransaction
                    {
                        Id = null
                    },
                    Block = new TxSignRequestParamBlock
                    {
                        Id = null,
                        Height = null,
                        IsSealed = null
                    },
                    Account = new TxSignRequestParamAccount
                    {
                        Addr = null
                    },
                    Collection = new TxSignRequestParamCollection
                    {
                        Id = null
                    }
                },
                Voucher = new TxSignRequestParamVoucher
                {
                    Cadence = txn.Script,
                    RefBlock = txn.ReferenceBlockId,
                    ComputeLimit = (int)txn.GasLimit,
                    Arguments = new object[txn.Arguments.Count],
                    ProposalKey = new TxSignRequestParamProposalKey
                    {
                        Address = txn.ProposalKey.Address,
                        KeyId = (int)txn.ProposalKey.KeyId,
                        SequenceNum = (int)txn.ProposalKey.SequenceNumber
                    },
                    Payer = account,
                    Authorizers = new string[1] { account },
                    PayloadSigs = new TxSignRequestParamSignature[0],
                    EnvelopeSigs = new TxSignRequestParamSignature[1]
                    {
                        new TxSignRequestParamSignature
                        {
                            Address = account,
                            KeyId = 0,
                            Sig = null
                        }
                    }
                },
                Address = account
            };

            for (int i = 0; i < txn.Arguments.Count; i++)
            {
                reqParams.Args[i] = txn.Arguments[i];

                string argTempId = RandomString(10);

                reqParams.Interaction.Arguments.Add(argTempId, new TxSignRequestParamArgument
                {
                    Kind = "ARGUMENT",
                    TempId = argTempId,
                    Value = txn.Arguments[i].GetValue(),
                    AsArgument = txn.Arguments[i],
                    Xform = new TxSignRequestParamXform
                    {
                        Label = txn.Arguments[i].Type
                    }
                });

                reqParams.Interaction.Message.Arguments[i] = argTempId;

                reqParams.Voucher.Arguments[i] = txn.Arguments[i];
            }

            return reqParams;
        }
    }
}
