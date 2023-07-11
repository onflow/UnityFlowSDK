using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Fcl.Net.Core;
using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;

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

        async Task<T> IStrategy.ExecuteAsync<T>(FclService service, FclServiceConfig config = null, object data = null, HttpMethod httpMethod = null)
        {
            Debug.Log("UnityWalletConnectStrategy ExecuteAsync");

            if (service.Endpoint == "flow_authn")
            {
                // Desktop - Show a QR Code to scan
                // Try to use user defined QR Code prefab
                UnityEngine.Object prefab = _config.QrCodeDialogPrefab as UnityEngine.Object;

                if (prefab == null)
                {
                    // Load default QR Code Dialog prefab
                    Debug.Log("<b>QrCodeDialogPrefab</b> not assigned in WalletConnectConfig, using default dialog prefab.");
                    prefab = Resources.Load("QRCodeDialogPrefab_FCL");
                }

                // Instantiate the prefab which shows a list of dev accounts to select from
                if (_qrDialog == null)
                {
                    _qrDialog = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                }

                if (_client == null)
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
                            throw new System.Exception("Fcl UnityWalletConnectStrategy: Exception triggered while initializing WC.");
                        }

                        throw new System.Exception("Fcl UnityWalletConnectStrategy: Authentication initialization timed out");
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
                            throw new System.Exception("Fcl UnityWalletConnectStrategy: Exception triggered while connecting.");
                        }

                        throw new System.Exception("Fcl UnityWalletConnectStrategy: Authentication connection timed out");
                    }

                    _connectedData = task.Result;
                }

                Debug.Log($"Wallet Connect: connection uri is {_connectedData.Uri}");

                // Get the script component and initialize it
                QRCodeDialog qrDialogScript = _qrDialog.GetComponentInChildren<QRCodeDialog>();

                if (qrDialogScript == null)
                {
                    throw new System.Exception($"Fcl UnityWalletConnectStrategy: <b>QrCodeDialog</b> component missing on {prefab.name}. Unable to render QR code.");
                }

                bool initSuccess = qrDialogScript.Init(_connectedData.Uri, () =>
                {
                    Debug.Log("Dialog closed, stopping session connection.");
                    _killSessionTask = true;
                });

                if (initSuccess == false)
                {
                    UnityEngine.Object.Destroy(_qrDialog);
                    _qrDialog = null;
                    throw new System.Exception("Fcl UnityWalletConnectStrategy: qrDialogScript Init failed.");
                }

                Debug.Log("Waiting for approval...");

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
                        throw new System.Exception($"Fcl WalletConnectStrategy: Exception occurred waiting for connection approval.", sessionTask.Exception);
                    }
                    _session = sessionTask.Result;
                }

                UnityEngine.Object.Destroy(_qrDialog);
                _qrDialog = null;
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
                    _ => throw new System.Exception($"Fcl WalletConnectStrategy: Request not supported"),
                } as T;
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Fcl WalletConnectStrategy: Sending Request Failed.", ex);
            }
        }
    }
}
