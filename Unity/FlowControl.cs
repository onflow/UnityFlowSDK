using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DapperLabs.Flow.Sdk.Unity
{
    /// <summary>
    /// FlowControl sits on top of the %Flow %Unity SDK and provides editor tools and functions to make %Unity integration easier.
    /// </summary>
    public class FlowControl : MonoBehaviour
    {
        private static Dictionary<string, Gateway> _gatewayCache;

        [SerializeField]
        private FlowControlData _data;

        [NonSerialized]
        private static string _emulatorOutput;

        //Emulator process management variables
        private static Thread EmulatorThread = null;
        private static Process EmulatorProcess;

        /// <summary>
        /// Clears the current FlowControlData object associated with this FlowControl instance.
        /// </summary>
        public static void ClearData()
        {
            Data.EmulatorSettings = new FlowControl.EmulatorSettings();
            Data.TextReplacements = new List<FlowControl.TextReplacement>();
            Data.Accounts = new List<FlowControl.Account>();
        }

        /// <summary>
        /// Returns a JSON representation of the current FlowControlData object.
        /// </summary>
        /// <returns>A string containing the JSON representation of the current FlowControlData object.</returns>
        public static string ToJson()
        {
            return JsonConvert.SerializeObject(Data, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        }

        /// <summary>
        /// Populates the current FlowControlData object from a JSON string.
        /// </summary>
        /// <param name="json">The JSON encoded configuration values that should be set on the active FlowControlData object</param>
        public static void FromJson(string json)
        {
            ClearData();
            JsonConvert.PopulateObject(json, Data);
        }

        /// <summary>
        /// Accesses the current FlowControlData object in use by FlowControl
        /// </summary>
        [SerializeField]
        public static FlowControlData Data
        {
            
            get
            {
                //If not already set, get any existing instances.
                if (Instance == null)
                {
                    Instance = FindObjectOfType<FlowControl>();

                    if (Instance == null)
                    {
                        UnityEngine.Debug.Log("No FlowControl components found.");
                        return null;
                    }
                }

                if (Instance._data == null)
                {
                    UnityEngine.Debug.Log("FlowControl data reference not set.");
                }

                return Instance._data;
            }

            set
            {
                if (Instance == null)
                {
                    UnityEngine.Debug.Log("No FlowControl components found.");
                }

                Instance._data = value;
            }
        }

        /// <summary>
        /// Clears the emulator output buffer
        /// </summary>
        public static void ClearEmulatorOutput()
        {
            _emulatorOutput = "";
        }

        /// <summary>
        /// Contains a cache of instances of all found Gateway classes for later querying/use.
        /// </summary>
        public static Dictionary<string, Gateway> GatewayCache
        {
            get
            {
                if (_gatewayCache == null)
                {
                    CacheGateways();
                }

                return _gatewayCache;
            }

            set => _gatewayCache = value;
        }

        /// <summary>
        /// Returns the current status of the emulator.  True if it is running, false if it is not or if the reference to the running process has been lost.
        /// </summary>
        public static bool IsEmulatorRunning => EmulatorThread != null && EmulatorThread.IsAlive && !EmulatorProcess.HasExited;

        /// <summary>
        /// Returns the output of the emulator output buffer.
        /// </summary>
        public static string EmulatorOutput => _emulatorOutput;

        /// <summary>
        /// Gets the currently instantiated FlowControl instance.
        /// </summary>
        public static FlowControl Instance { get; set; } = null;

        private static void CacheGateways()
        {
            //Forces create if it doesn't exist;
            if (_gatewayCache == null)
            {
                _gatewayCache = new Dictionary<string, Gateway>();
            }

            _gatewayCache.Clear();

            //Iterate through all classes that inherit from Gateway.
            foreach (Type gatewayProvider in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
               .Where(x => x.IsSubclassOf(typeof(Gateway)))
               .ToList())
            {
                Gateway tempGateway = (Gateway)Activator.CreateInstance(gatewayProvider);

                if (!_gatewayCache.ContainsKey(tempGateway.Name))
                {
                    _gatewayCache[tempGateway.Name] = tempGateway;
                }
            }
        }

        IEnumerator Start()
        {
            if (Data == null)
            {
                Data = ScriptableObject.CreateInstance<FlowControlData>();
                UnityEngine.Debug.Log("FlowControl doesn't have an assigned data resource, creating.");
                yield break;
            }

            Instance = this;


            if (Data.EmulatorSettings.runEmulatorInPlayMode)
            {
                StartEmulator();
            }

            yield break;
        }

        public static string FindFlowExecutable()
        {
            //Find where the flow executable is.
            List<string> pathsToCheck = new List<string>();
            if (Data.EmulatorSettings.flowExecutablePathOverride != "")
            {
                if (File.Exists(Data.EmulatorSettings.flowExecutablePathOverride))
                {
                    return Data.EmulatorSettings.flowExecutablePathOverride;
                }
            }

            string currentPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable("PATH");
            pathsToCheck.AddRange(currentPathString.Split(Path.PathSeparator));
            pathsToCheck.Add("/opt/homebrew/bin");
            pathsToCheck.Add("/usr/local/bin");

            foreach (string path in pathsToCheck.Where(path => File.Exists($"{path}/flow.exe") || File.Exists($"{path}/flow")))
            {
                return $"{path}/flow";
            }
            
            return "";
        }
        
        
        /// <summary>
        /// Starts the emulator on a background thread.
        /// </summary>
        public static void StartEmulator()
        {
            //Exit if the emulator data directory isn't set
            if (Data.EmulatorSettings.emulatorDataDirectory == "" || !Directory.Exists(Data.EmulatorSettings.emulatorDataDirectory))
            {
                Debug.Log("No emulator data directory specified, can not start emulator.");
                return;
            }
            
            //Create the flow.json init file if it doesn't exist.
            if (!File.Exists($"{Data.EmulatorSettings.emulatorDataDirectory}/flow.json"))
            {
                ProcessStartInfo psi = new ProcessStartInfo(FindFlowExecutable(), "init")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = Data.EmulatorSettings.emulatorDataDirectory
                };

                Process emulatorInitProcess = Process.Start(psi);
                emulatorInitProcess.WaitForExit();
            }
            
#if UNITY_EDITOR
            //Kill any other running flow processes
            if (Data.EmulatorSettings.killOtherFlow)
            {
                Process[] flowProcesses = Process.GetProcessesByName("flow");
                foreach (Process fp in flowProcesses)
                {
                    fp.Kill();
                }
            }

            //Start the new emulator process
            EmulatorThread = new Thread(StartEmulatorProcess)
            {
                IsBackground = true
            };
            EmulatorThread.Start();

            string serviceAddress = "";
            string servicePrivKey = "";

            //Get the service account information.
            while ((servicePrivKey == "" || serviceAddress == "") && EmulatorThread.IsAlive)
            {
                try
                {
                    foreach (string split in EmulatorOutput.Split(' '))
                    {
                        if (split.StartsWith("serviceAddress="))
                        {
                            serviceAddress = "0x" + split.Split('=')[1];
                        }

                        if (split.StartsWith("servicePrivKey="))
                        {
                            servicePrivKey = split.Split(new string[] { "servicePrivKey=" }, StringSplitOptions.None)[1];
                        }
                    }
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }

            //If an emulator service account already exists, update it.  If not, create one.
            Account oldServiceAccount = FlowControl.Data.Accounts.Find(x => x.Name == "emulator_service_account");
            if (oldServiceAccount != null)
            {
                oldServiceAccount.AccountConfig["Private Key"] = servicePrivKey;
                oldServiceAccount.AccountConfig["Address"] = serviceAddress;
            }
            else
            {
                Account serviceAccount = new FlowControl.Account() { GatewayName = "Emulator", Name = "emulator_service_account" };
                serviceAccount.AccountConfig["Private Key"] = servicePrivKey;
                serviceAccount.AccountConfig["Address"] = serviceAddress;
                Data.Accounts.Add(serviceAccount);
            }
#endif
        }

        /// <summary>
        /// Stops a running emulator process.
        /// </summary>
        public static void StopEmulator()
        {
#if UNITY_EDITOR
            if (EmulatorProcess != null)
            {
                try
                {
                    EmulatorProcess.Kill();
                    EmulatorThread.Abort();
                }
                catch
                {

                }
            }
            EmulatorThread = null;
#endif
        }

        private static void StartEmulatorProcess()
        {
            ProcessStartInfo psi = new ProcessStartInfo(FindFlowExecutable(), $"emulator --host 0.0.0.0 {(Data.EmulatorSettings.persistData?" --persist":"")}{(Data.EmulatorSettings.verbose?" --verbose" : "")}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Data.EmulatorSettings.emulatorDataDirectory
            };

            EmulatorProcess = Process.Start(psi);

            Task<string> soTask = EmulatorProcess.StandardOutput.ReadLineAsync();
            Task<string> seTask = EmulatorProcess.StandardError.ReadLineAsync();

            Thread.Sleep(100);
            if (EmulatorProcess.HasExited)
            {
                UnityEngine.Debug.Log("Error running emulator:  exit code: " + EmulatorProcess.ExitCode.ToString());
            }

            while (!EmulatorProcess.HasExited)
            {
                if (soTask.IsCompleted)
                {
                    _emulatorOutput += Regex.Replace(soTask.Result, @"\e\[(\d+;)*(\d+)?[ABCDHJKfmsu]", "") + "\n\n";
                    soTask = EmulatorProcess.StandardOutput.ReadLineAsync();
                }

                if (seTask.IsCompleted)
                {
                    _emulatorOutput += "<color=red>" + Regex.Replace(seTask.Result, @"\e\[(\d+;)*(\d+)?[ABCDHJKfmsu]", "") + "</color>\n\n";
                    seTask = EmulatorProcess.StandardError.ReadLineAsync();
                }
            }
        }

        /// <summary>
        /// Used to convert a FlowControl.Account object into a SdkAccount object.
        /// </summary>
        /// <param name="name">Account name to query for.</param>
        /// <returns>An SDKAccount containing data for the requested FlowControl.Account</returns>
        public static SdkAccount GetSdkAccountByName(string name)
        {
            Account acct = Data.Accounts.FirstOrDefault(x => x.Name == name);
            if (acct == null)
            {
                return null;
            };

            return new SdkAccount()
            {
                Name = acct.Name,
                Address = acct.AccountConfig["Address"],
                PrivateKey = acct.AccountConfig["Private Key"]
            };
        }

        /// <summary>
        /// Used to convert a FlowControl.Account object into a SdkAccount object.
        /// </summary>
        /// <param name="address">The %Flow address to query for.</param>
        /// <returns>An SDKAccount containing data for the requested FlowControl.Account</returns>
        public static SdkAccount GetSdkAccountByAddress(string address)
        {
            Account acct = Data.Accounts.FirstOrDefault(x => x.AccountConfig["Address"] == address);
            if (acct == null)
            {
                return null;
            };

            return new SdkAccount
            {
                Name = acct.Name,
                Address = acct.AccountConfig["Address"],
                PrivateKey = acct.AccountConfig["Private Key"]
            };
        }

        /// <summary>
        /// Deletes the emulator database to get a clean state upon next startup.
        /// </summary>
        public static void ClearEmulatorData()
        {
            if (EmulatorThread != null && EmulatorThread.IsAlive)
            {
                Debug.Log("Exiting due to emulator thread running");
                return;
            }

            Process[] flowProcesses = Process.GetProcessesByName("flow");
            foreach (Process fp in flowProcesses)
            {
                fp.Kill();
            }
            
            //Emulator data has been cleared so sequence numbers are no longer valid, reset all tracking.
            Transactions.ResetSequenceTracking();
            
            DirectoryInfo di = new DirectoryInfo(Data.EmulatorSettings.emulatorDataDirectory + "/flowdb");

            if (di.Exists)
            {
                di.Delete(true);
            }
            
            //Delete .meta file if it exists
            File.Delete(Data.EmulatorSettings.emulatorDataDirectory + "/flowdb.meta");
        }

        private void OnDisable()
        {
            StopEmulator();
        }


        /// <summary>
        /// Text replacement information.  FlowControl operations will first replace all instances of originalText with replacementText before processing them.
        /// </summary>
        [Serializable]
        public class TextReplacement
        {
            /// <summary>
            /// Description displayed in FlowControl editor window for this TextReplacement.
            /// </summary>
            public string description;

            /// <summary>
            /// The text that should be replaced.
            /// </summary>
            public string originalText;

            /// <summary>
            /// The text that will replace originalText.
            /// </summary>
            public string replacementText;

            /// <summary>
            /// If true, this TextReplacement will be considered.  If false, it will never be processed.
            /// </summary>
            public bool active;

            /// <summary>
            /// A list of Account names that this TextReplacement applies to.  "All" will apply to all accounts, "None" will apply to no accounts.
            /// </summary>
            [SerializeField]
            private List<string> _applyToAccounts = new List<string>();

            /// <summary>
            /// A list of Gateway names that this TextReplacement applies to.  "All" will apply to all gateways, "None" will apply to no gateways.
            /// </summary>
            [SerializeField]
            private List<string> _applyToGateways = new List<string>();

            /// <summary>
            /// A list of account names that this text replacement will apply to.  The string "All" can be used to indicate all accounts.
            /// </summary>
            public List<string> ApplyToAccounts 
            {
                get
                {
                    if (_applyToAccounts.Contains("All") && _applyToAccounts.Count > 1)
                    {
                        _applyToAccounts.Clear();
                        _applyToAccounts.Add("All");
                    }

                    return _applyToAccounts;
                }

                set => _applyToAccounts = value;
            }

            /// <summary>
            /// A list of gateway names that this text replacement will apply to.  The string "All" can be used to indicate all gateways
            /// </summary>
            public List<string> ApplyToGateways 
            {
                get
                {
                    if (_applyToGateways.Contains("All") && _applyToGateways.Count > 1)
                    {
                        _applyToGateways.Clear();
                        _applyToGateways.Add("All");
                    }

                    return _applyToGateways;
                }

                set => _applyToGateways = value;
            }
        }

        /// <summary>
        /// Settings related to the emulator.
        /// </summary>
        [Serializable]
        public class EmulatorSettings
        {
            /// <summary>
            /// Path where flow.json and flowdb persistent data should be stored.
            /// </summary>
            public string emulatorDataDirectory = "";

            /// <summary>
            /// Path at which the flow binary can be found.  If populated, it won't attempt to autodetect
            /// the path.
            /// </summary>
            public string flowExecutablePathOverride = "";

            /// <summary>
            /// If true, the emulator is started when entering play mode or when running a build.
            /// </summary>
            public bool runEmulatorInPlayMode = true;

            /// <summary>
            /// If true, emulator blockchain state will be persisted to disk.
            /// </summary>
            public bool persistData = true;

            /// <summary>
            /// If true, running flow.exe processes will be killed before attempting to start a new one.
            /// </summary>
            public bool killOtherFlow = true;

            /// <summary>
            /// If true, the Emulator Output window will display verbose logging from flow.
            /// </summary>
            public bool verbose = true;

            /// <summary>
            /// The URL used to access the emulator
            /// </summary>
            public string emulatorEndpoint = "http://127.0.0.1:8888/v1";
        }

        /// <summary>
        /// Name to endpoint connection string mapping.  Only used for direct connections to the flow network (emulator, testnet, mainnet).
        /// Other gateways may or may not use this.
        /// </summary>
        [Serializable]
        public class Network
        {
            /// <summary>
            /// The display name of the endpoint.
            /// </summary>
            public string name;

            /// <summary>
            /// The connection string used by the Flow SDK to establish a connection to this endpoint.
            /// </summary>
            public string endpoint;
        }

        /// <summary>
        /// FlowControl Account information.
        /// </summary>
        [Serializable]
        public class Account : ISerializationCallbackReceiver
        {
            [SerializeField]
            private string _name = "";

            //Used for serialization
            [SerializeField]
            private List<string> infoKeys = new List<string>();
            [SerializeField]
            private List<string> infoValues = new List<string>();

            [SerializeField]
            private string _gatewayName = "";

            [JsonIgnore]
            private Gateway _gatewayObject;

            private Dictionary<string, string> _accountConfig = new Dictionary<string, string>();

            /// <summary>
            /// Instance of the Gateway in use by this account.
            /// </summary>
            
            [JsonIgnore]
            public Gateway GatewayObject
            {
                get
                {
                    if (_gatewayObject == null || _gatewayObject.Name != GatewayName)
                    {
                        _gatewayObject = FlowControl.GatewayCache[GatewayName];
                    }

                    return _gatewayObject;
                }

                set => _gatewayObject = value;
            }

            /// <summary>
            /// Name of the Account.
            /// </summary>
            public string Name { get => _name; set => _name = value; }

            /// <summary>
            /// Name of the Gateway in use by this account.
            /// </summary>
            public string GatewayName { get => _gatewayName; set => _gatewayName = value; }

            /// <summary>
            /// Gateway specific data for this account
            /// </summary>
            public Dictionary<string, string> AccountConfig
            {
                get
                {
                    if (_accountConfig == null)
                    {
                        _accountConfig = new Dictionary<string, string>();
                    }

                    return _accountConfig;
                }

                set => _accountConfig = value;
            }

            /// <summary>
            /// Performs text replacements that are assigned to this account and gateway.
            /// </summary>
            /// <param name="data">A string that will have relevant replacements done to it.</param>
            /// <returns>The updated string.</returns>
            public string DoTextReplacements(string data)
            {
                List<TextReplacement> replacements = FlowControl.Data.TextReplacements.FindAll(x => x.active);
                replacements = replacements.FindAll(x => (x.ApplyToAccounts.Contains("All") || x.ApplyToAccounts.Contains(Name)));
                replacements = replacements.FindAll(x => (x.ApplyToGateways.Contains("All") || x.ApplyToGateways.Contains(GatewayName)));

                foreach (TextReplacement rep in replacements)
                {
                    data = data.Replace(rep.originalText, rep.replacementText);
                }

                return data;
            }

            /// <summary>
            /// Submits a transaction and returns immediately, before it is executed by the blockchain.
            /// </summary>
            /// <param name="script">Transaction script that should be submitted.</param>
            /// <param name="args">Arguments required by the transaction script.</param>
            /// <returns>A Task that will resolve to a FlowTransactionResponce upon completion.</returns>
            public Task<FlowTransactionResponse> Submit(string script, params CadenceBase[] args)
            {
                script = DoTextReplacements(script);
                return GatewayObject.Submit(script, _accountConfig, args);
            }

            /// <summary>
            /// Submits a transaction and returns after the transaction has either been sealed by the blockchain, or when an error occurs.
            /// </summary>
            /// <param name="script">Transaction script that should be submitted.</param>
            /// <param name="args">Arguments required by the transaction script.</param>
            /// <returns>A Task that will resolve to a FlowTransactionResult upon completion.</returns>
            public Task<FlowTransactionResult> SubmitAndWaitUntilSealed(string script, params CadenceBase[] args)
            {
                script = DoTextReplacements(script);
                return GatewayObject.SubmitAndWaitUntilSealed(script, _accountConfig, args);
            }
            
            /// <summary>
            /// Submits a transaction and returns after the transaction has either been executed by the blockchain, or when an error occurs.
            /// This is faster than waiting until sealed, but has a very, very small chance of being rolled back by the chain after returning.
            /// </summary>
            /// <param name="script">Transaction script that should be submitted.</param>
            /// <param name="args">Arguments required by the transaction script.</param>
            /// <returns>A Task that will resolve to a FlowTransactionResult upon completion.</returns>
            /// <example>
            /// @code
            /// IEnumerator MyTransaction() {
            ///     var task = myAccount.SubmitAndWaitUntilExecuted(transactionCode, new CadenceInt(5));
            ///     yield return new WaitUntil(() => task.IsCompleted);
            ///
            ///     if (transactionTask.Result.Error != null || !string.IsNullOrEmpty(transactionTask.Result.ErrorMessage))
            ///     {
            ///         Debug.LogError($"Error executing transaction: {transactionTask.Result.Error?.Message??transactionTask.Result.ErrorMessage}");
            ///         yield break;
            ///     }
            /// }
            /// @endcode
            /// </example>
            public Task<FlowTransactionResult> SubmitAndWaitUntilExecuted(string script, params CadenceBase[] args)
            {
                script = DoTextReplacements(script);
                return GatewayObject.SubmitAndWaitUntilExecuted(script, _accountConfig, args);
            }

            /// <summary>
            /// Executes a script on the blockchain.
            /// </summary>
            /// %Scripts do not require an account to run on the %Flow network, but it is offered as part of the Account class to allow for automatic Gateway and Network selection.
            /// <param name="script">Cadence script that should be executed.</param>
            /// <param name="args">Arguments required by the script.</param>
            /// <returns>A Task that will resolve to a FlowScriptResponse upon completion.</returns>
            /// Example:
            /// <example>
            /// @code
            /// IEnumerator AddOne() {
            ///     var task = myAccount.ExecuteScript(scriptData, new CadenceInt(5));
            ///     yield return new WaitUntil(() => task.IsCompleted);
            ///     
            ///     return  int.Parse(task.Result.Value.As<CadenceNumber>().Value);
            /// }
            /// @endcode
            /// </example>
            public Task<FlowScriptResponse> ExecuteScript(string script, params CadenceBase[] args)
            {
                script = DoTextReplacements(script);

                FlowScriptRequest sr = new FlowScriptRequest();
                sr.Script = script;
                sr.Arguments = new List<CadenceBase>(args);

                GatewayObject.Init(AccountConfig);
                return Scripts.ExecuteAtLatestBlock(sr);
            }

            /// <summary>
            /// Deploys a contract to this account.
            /// </summary>
            /// <param name="name">The name of the contract to deploy</param>
            /// <param name="contractText">The cadence contents of the contract</param>
            /// <returns>A Task that will resolve to a FlowTransactionResponse when complete</returns>
            public Task<FlowTransactionResponse> DeployContract(string name, string contractText)
            {
                contractText = DoTextReplacements(contractText);
                GatewayObject.Init(AccountConfig);
                return CommonTransactions.DeployContract(name, contractText);
            }
            
            /// <summary>
            /// Updates a contract on this account
            /// </summary>
            /// <param name="name">The name of the contract to update</param>
            /// <param name="contractText">The cadence contents of the updated contract</param>
            /// <returns>A Task that will resolve to a FlowTransactionResponse when complete</returns>
            public Task<FlowTransactionResponse> UpdateContract(string name, string contractText)
            {
                contractText = DoTextReplacements(contractText);
                GatewayObject.Init(AccountConfig);
                return CommonTransactions.UpdateContract(name, contractText);
            }

            /// <summary>
            /// Removes a contract from this account
            /// </summary>
            /// <param name="name">The name of the contract to remove</param>
            /// <returns>A Task that will resolve to a FlowTransactionResponse when complete</returns>
            public Task<FlowTransactionResponse> RemoveContract(string name)
            {
                GatewayObject.Init(AccountConfig);
                return CommonTransactions.RemoveContract(name);
            }
            
            /// <summary>
            /// Serialization helper
            /// </summary>
            public void OnBeforeSerialize()
            {
                infoKeys = _accountConfig.Keys.ToList();
                infoValues = _accountConfig.Values.ToList();
            }

            /// <summary>
            /// Serialization helper
            /// </summary>
            public void OnAfterDeserialize()
            {
                for (int i = 0; i < infoKeys.Count; i++)
                {
                    _accountConfig[infoKeys[i]] = infoValues[i];
                }
            }
        }
    }
}