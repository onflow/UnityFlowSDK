using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Fcl.Net.Core;
using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using UnityEngine;

#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.Networking;
#endif

namespace DapperLabs.Flow.Sdk.Fcl
{
    [RpcMethod("flow_authn")]
    internal class AuthnRequest : List<string>
    {
        internal AuthnRequest(IEnumerable<string> collection) : base(collection)
        {
        }
    }

    [RpcMethod("flow_authz")]
    internal class AuthzRequest : List<string>
    {
        internal AuthzRequest(IEnumerable<string> collection) : base(collection)
        {
        }
    }

    [RpcMethod("flow_user_sign")]
    internal class UserSignRequest : List<string>
    {
        internal UserSignRequest(IEnumerable<string> collection) : base(collection)
        {
        }
    }

    [RpcMethod("flow_pre_authz")]
    internal class PreAuthzRequest : List<string>
    {
        internal PreAuthzRequest(IEnumerable<string> collection) : base(collection)
        {
        }
    }

    public class UnityWalletConnectStrategy : IStrategy
    {
        private WalletConnectConfig _config;
        private GameObject _qrDialog = null;
        bool _killSessionTask = false;

        WalletConnectSignClient _client = null;
        ConnectedData _connectedData = null;
        SessionStruct _session;

        public UnityWalletConnectStrategy(FetchService fetchService, WalletConnectConfig config)
        {
            _config = config;
        }

        async Task<T> IStrategy.ExecuteAsync<T>(FclService service, FclServiceConfig config, object data, HttpMethod httpMethod)
        {
            if (service.Endpoint == "flow_authn")
            {
#if !UNITY_ANDROID && !UNITY_IOS
                // Desktop - Show a QR Code to scan
                // Try to use user defined QR Code prefab
                UnityEngine.Object prefab = _config.QrCodeDialogPrefab as UnityEngine.Object;

                if (prefab == null)
                {
                    // Load default QR Code Dialog prefab
                    Debug.Log("Fcl: <b>QrCodeDialogPrefab</b> not assigned in WalletConnectConfig, using default dialog prefab.");
                    prefab = Resources.Load("QRCodeDialogPrefab_FCL");
                }

                // Instantiate the prefab which shows a list of dev accounts to select from
                if (_qrDialog == null)
                {
                    _qrDialog = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                }
#endif

                if (_client == null)
                {
                    try
                    {
                        var options = new SignClientOptions()
                        {
                            ProjectId = _config.ProjectId,
                            Metadata = new Metadata()
                            {
                                Description = _config.ProjectDescription,
                                Icons = new[] { _config.ProjectIconUrl },
                                Name = _config.ProjectName,
                                Url = _config.ProjectUrl,
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
                            if (_qrDialog != null)
                            {
                                UnityEngine.Object.Destroy(_qrDialog);
                                _qrDialog = null;
                            }

                            if (task.IsFaulted)
                            {
                                throw new Exception("Fcl: WalletConnectStrategy: Exception triggered while initializing WC.", task.Exception);
                            }

                            throw new Exception("Fcl: WalletConnectStrategy: Authentication initialization timed out");
                        }

                        _client = task.Result;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Fcl: WalletConnectStrategy: Exception thrown while initializing Wallet Connect Sign Client.", ex);
                    }
                }

                if (_connectedData == null)
                {
                    try
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
                                            "flow_pre_authz"
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
                            if (_qrDialog != null)
                            {
                                UnityEngine.Object.Destroy(_qrDialog);
                                _qrDialog = null;
                            }

                            if (task.IsFaulted)
                            {
                                throw new Exception("Fcl: WalletConnectStrategy: Exception triggered while connecting.", task.Exception);
                            }

                            throw new Exception("Fcl: WalletConnectStrategy: Authentication connection timed out");
                        }

                        _connectedData = task.Result;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Fcl: WalletConnectStrategy: Exception thrown during WC Connect.", ex);
                    }
                }

                Debug.Log($"Fcl: Wallet Connect: connection uri is {_connectedData.Uri}");

#if UNITY_ANDROID || UNITY_IOS

                // Mobile - Deep link to a mobile wallet app
                try
                {
                    string urlEncoded = UnityWebRequest.EscapeURL(_connectedData.Uri);
                    string url = $"{service.Uid}?uri={urlEncoded}";
                    Debug.Log($"Fcl: deeplink url: {url}");
                    Application.OpenURL(url);
                }
                catch (Exception ex)
                {
                    throw new Exception("Fcl: WalletConnectStrategy: Exception thrown deep linking to wallet app.", ex);
                }
#else
                // Get the script component and initialize it
                QRCodeDialog qrDialogScript = _qrDialog.GetComponentInChildren<QRCodeDialog>();

                if (qrDialogScript == null)
                {
                    throw new Exception($"Fcl: WalletConnectStrategy: <b>QrCodeDialog</b> component missing on {prefab.name}. Unable to render QR code.");
                }

                bool initSuccess = qrDialogScript.Init(_connectedData.Uri, () =>
                {
                    _killSessionTask = true;
                });

                if (initSuccess == false)
                {
                    UnityEngine.Object.Destroy(_qrDialog);
                    _qrDialog = null;
                    throw new Exception("Fcl: WalletConnectStrategy: qrDialogScript Init failed.");
                }
#endif

                Debug.Log("Fcl: Waiting for user to approve connection in wallet...");

                if (_connectedData.Approval.Status != TaskStatus.RanToCompletion)
                {
                    Task<SessionStruct> sessionTask = _connectedData.Approval;
                    while (sessionTask.IsCompleted == false)
                    {
                        await Task.Delay(500);
                        if (_killSessionTask)
                        {
                            _killSessionTask = false;
                            return new FclAuthResponse
                            {
                                Status = ResponseStatus.Declined
                            } as T;
                        }
                    }

                    if (sessionTask.IsFaulted)
                    {
                        throw new Exception($"Fcl: WalletConnectStrategy: Exception occurred waiting for connection approval.", sessionTask.Exception);
                    }
                    _session = sessionTask.Result;
                }

#if !UNITY_ANDROID && !UNITY_IOS
                UnityEngine.Object.Destroy(_qrDialog);
                _qrDialog = null;
#endif
            }

            var requestData = new Dictionary<string, object>();

            if (data != null)
            {
                foreach (var item in data.ToDictionary<string, object>())
                    requestData.Add(item.Key, item.Value);
            }

            var req = new List<string> { JsonConvert.SerializeObject(requestData) };

            try
            {
                return service.Endpoint switch
                {
                    "flow_authn" => await _client.Request<AuthnRequest, FclAuthResponse>(_session.Topic, new AuthnRequest(req)),
                    "flow_authz" => await _client.Request<AuthzRequest, FclAuthResponse>(_session.Topic, new AuthzRequest(req)),
                    "flow_user_sign" => await _client.Request<UserSignRequest, FclAuthResponse>(_session.Topic, new UserSignRequest(req)),
                    "flow_pre_authz" => await _client.Request<PreAuthzRequest, FclAuthResponse>(_session.Topic, new PreAuthzRequest(req)),
                    _ => throw new Exception($"Fcl: WalletConnectStrategy: Request method '{service.Endpoint}' not supported."),
                } as T;
            }
            catch (Exception ex)
            {
                throw new Exception($"Fcl: WalletConnectStrategy: Exception thrown sending WC Request.", ex);
            }
        }
    }
}
