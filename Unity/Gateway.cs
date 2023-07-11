using System;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.Unity
{
    /// <summary>
    /// Base class for any %Flow Gateway implementations.
    /// </summary>
    public abstract class Gateway
    {
        /// <summary>
        /// The display name for this Gateway.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The network that this gateway should connect to.
        /// </summary>
        public abstract string Network { get; }

        /// <summary>
        /// A list of parameters that are required for for this Gateway to function.
        /// </summary>
        public abstract List<string> RequiredParameters { get; }

        /// <summary>
        /// Allows a Gateway to limit the options for a parameter to a list of options.
        /// </summary>
        /// 
        /// Used by the FlowControlWindow %Accounts panel to display a dropdown list of possible values.
        /// <example>
        /// @code
        /// public override Dictionary<string, List<string>> SelectionParameters {
        ///     var options = new Dictionary<string, List<string>>();
        ///     options["My Parameter"] = new List<string>(){"Option 1", "Option 2"};
        ///     options["Transfer Type"] = new List<string>(){"Binary", "Text"};
        ///     return options;
        /// }
        /// @endcode
        /// </example>
        public abstract Dictionary<string, List<string>> SelectionParameters { get; }

        /// <summary>
        /// Not normally called directly, see FlowControl.Account.Submit
        /// </summary>
        /// Submits a transaction and returns a Task that will resolve into a FlowTransactionResponse.  
        /// <param name="script">The transaction script that should be run.</param>
        /// <param name="parameters">A dictionary containing the required parameters for this gateway and their values.</param>
        /// <param name="arguments">Arguments that are passed to the script when run on the blockchain.</param>
        /// <returns>A Task<FlowTransactionResponse> object that will resolve into a FlowTransactionResponse when it completes.</returns>
        public abstract Task<FlowTransactionResponse> Submit(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments);

        /// <summary>
        /// Not normally called directly, see FlowControl.Account.SubmitAndWaitUntilSealed
        /// </summary>
        /// Submits a transaction and returns a Task that will resolve into a FlowTransactionResult.  
        /// <param name="script">The transaction script that should be run.</param>
        /// <param name="parameters">A dictionary containing the required parameters for this gateway and their values.</param>
        /// <param name="arguments">Arguments that are passed to the script when run on the blockchain.</param>
        /// <returns>A Task object that will resolve into a FlowTransactionResult when it completes.</returns>
        public abstract Task<FlowTransactionResult> SubmitAndWaitUntilSealed(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments);
        
        /// <summary>
        /// Not normally called directly, see FlowControl.Account.SubmitAndWaitUntilExecuted
        /// </summary>
        /// Submits a transaction and returns a Task that will resolve into a FlowTransactionResult.  
        /// <param name="script">The transaction script that should be run.</param>
        /// <param name="parameters">A dictionary containing the required parameters for this gateway and their values.</param>
        /// <param name="arguments">Arguments that are passed to the script when run on the blockchain.</param>
        /// <returns>A Task object that will resolve into a FlowTransactionResult when it completes.</returns>
        public abstract Task<FlowTransactionResult> SubmitAndWaitUntilExecuted(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments);

        /// <summary>
        /// Call to perform any initialization tasks this Gateway requires.
        /// </summary>
        public abstract void Init(Dictionary<string, string> parameters);
    }

    /// <summary>
    /// Gateway that connects to a local %Flow emulator.
    /// </summary>
    public class EmulatorGateway : Gateway
    {
        /// <summary>
        /// The string "EMULATOR"
        /// </summary>
        public override string Network => "EMULATOR";

        /// <summary>
        /// The string "Emulator"
        /// </summary>
        public override string Name => "Emulator";

        /// <summary>
        /// Returns {"Private Key","Address"}
        /// </summary>
        public override List<string> RequiredParameters => new List<string>() { "Address", "Private Key"};

        /// <summary>
        /// Returns a Dict<string, List<string>> that provides the protocol options (HTTP)
        /// </summary>
        public override Dictionary<string, List<string>> SelectionParameters => null;

        public override void Init(Dictionary<string, string> parameters)
        {
            FlowConfig config = new FlowConfig
            {
                NetworkUrl = FlowControl.Data.EmulatorSettings.emulatorEndpoint,
                Protocol = FlowConfig.NetworkProtocol.HTTP
            };
            FlowSDK.Init(config);
        }

        public override Task<FlowTransactionResponse> Submit(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            Init(parameters);
            arguments = arguments ?? new CadenceBase[] { };
            
            if (FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address != parameters["Address"])
            {
                throw new Exception($"Called submit with address {parameters["Address"]} while authenticated as {FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address}");
            }
            
            return Transactions.Submit(script, new List<CadenceBase>(arguments));
        }

        public override Task<FlowTransactionResult> SubmitAndWaitUntilSealed(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            return SubmitAndWait(FlowTransactionStatus.SEALED, script, parameters, arguments);
        }
        
        public override Task<FlowTransactionResult> SubmitAndWaitUntilExecuted(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            return SubmitAndWait(FlowTransactionStatus.EXECUTED, script, parameters, arguments);
        }

        private async Task<FlowTransactionResult> SubmitAndWait(FlowTransactionStatus waitUntil, string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            Init(parameters);
            arguments = arguments ?? new CadenceBase[] { };

            if (FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address != parameters["Address"])
            {
                throw new Exception($"Called submit with address {parameters["Address"]} while authenticated as {FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address}");
            }

            FlowTransactionResponse res = await Transactions.Submit(script, new List<CadenceBase>(arguments));
            
            if (res.Error != null)
            {
                return new FlowTransactionResult
                {
                    Error = res.Error,
                    ErrorMessage = res.Error.Message
                };
            }

            FlowTransactionResult txResult = null;
            
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(1000);
                txResult = await Transactions.GetResult(res.Id);
                
                if (txResult.Error != null)
                {
                    continue;
                }
                
                if (txResult.Status >= waitUntil)
                {
                    return txResult;
                }
            }

            return txResult;
        }    
    }

    /// <summary>
    /// Gateway that connects to the %Flow TESTNET network
    /// </summary>
    public class TestNetGateway : Gateway
    {
        /// <summary>
        /// The string "TESTNET"
        /// </summary>
        public override string Network => "TESTNET";

        /// <summary>
        /// The string "Flow Testnet"
        /// </summary>
        public override string Name => "Flow Testnet";

        /// <summary>
        /// Returns {"Private Key","Address"}
        /// </summary>
        public override List<string> RequiredParameters => new List<string>() { "Address", "Private Key" };

        /// <summary>
        /// Returns an empty Dictionary<string, List<string>>
        /// </summary>
        public override Dictionary<string, List<string>> SelectionParameters => new Dictionary<string, List<string>>();

        public override void Init(Dictionary<string, string> parameters)
        {
            FlowConfig config = new FlowConfig
            {
                NetworkUrl = "https://rest-testnet.onflow.org/v1",
                Protocol = FlowConfig.NetworkProtocol.HTTP
            };

            FlowSDK.Init(config);
        }

        public override Task<FlowTransactionResponse> Submit(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            Init(parameters);
            arguments = arguments ?? new CadenceBase[] { };
            
            if (FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address != parameters["Address"])
            {
                throw new Exception($"Called submit with address {parameters["Address"]} while authenticated as {FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address}");
            }

            return Transactions.Submit(script, new List<CadenceBase>(arguments));
        }

        public override Task<FlowTransactionResult> SubmitAndWaitUntilSealed(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            return SubmitAndWait(FlowTransactionStatus.SEALED, script, parameters, arguments);
        }
        
        public override Task<FlowTransactionResult> SubmitAndWaitUntilExecuted(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            return SubmitAndWait(FlowTransactionStatus.EXECUTED, script, parameters, arguments);
        }

        private async Task<FlowTransactionResult> SubmitAndWait(FlowTransactionStatus waitUntil, string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            Init(parameters);
            arguments = arguments ?? new CadenceBase[] { };
            
            if (FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address != parameters["Address"])
            {
                throw new Exception($"Called submit with address {parameters["Address"]} while authenticated as {FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address}");
            }
            
            FlowTransactionResponse res = await Transactions.Submit(script, new List<CadenceBase>(arguments));
            
            if (res.Error != null)
            {
                return new FlowTransactionResult
                {
                    Error = res.Error,
                    ErrorMessage = res.Error.Message
                };
            }

            FlowTransactionResult txResult = null;
            
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(1000);
                txResult = await Transactions.GetResult(res.Id);
                
                if (txResult.Error != null)
                {
                    continue;
                }
                
                if (txResult.Status >= waitUntil)
                {
                    return txResult;
                }
            }

            return txResult;
        }
    }

    /// <summary>
    /// Gateway that connects to the %Flow MAINNET network
    /// </summary>
    public class MainNetGateway : Gateway
    {
        /// <summary>
        /// The string "MAINNET"
        /// </summary>
        public override string Network => "MAINNET";

        /// <summary>
        /// The string "Flow Mainnet"
        /// </summary>
        public override string Name => "Flow Mainnet";

        /// <summary>
        /// Returns {"Private Key","Address"}
        /// </summary>
        public override List<string> RequiredParameters => new List<string>() { "Address", "Private Key" };

        /// <summary>
        /// Returns an empty Dictionary<string, List<string>>
        /// </summary>
        public override Dictionary<string, List<string>> SelectionParameters => new Dictionary<string, List<string>>();

        public override void Init(Dictionary<string, string> parameters)
        {
            FlowConfig config = new FlowConfig
            {
                NetworkUrl = "https://rest-mainnet.onflow.org/v1",
                Protocol = FlowConfig.NetworkProtocol.HTTP
            };

            FlowSDK.Init(config);
        }

        public override Task<FlowTransactionResponse> Submit(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            Init(parameters);
            arguments = arguments ?? new CadenceBase[] { };

            if (FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address != parameters["Address"])
            {
                throw new Exception($"Called submit with address {parameters["Address"]} while authenticated as {FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address}");
            }

            return Transactions.Submit(script, new List<CadenceBase>(arguments));
        }

        public override Task<FlowTransactionResult> SubmitAndWaitUntilSealed(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            return SubmitAndWait(FlowTransactionStatus.SEALED, script, parameters, arguments);
        }

        public override Task<FlowTransactionResult> SubmitAndWaitUntilExecuted(string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            return SubmitAndWait(FlowTransactionStatus.EXECUTED, script, parameters, arguments);
        }

        private async Task<FlowTransactionResult> SubmitAndWait(FlowTransactionStatus waitUntil, string script, Dictionary<string, string> parameters, params CadenceBase[] arguments)
        {
            Init(parameters);
            arguments = arguments ?? new CadenceBase[] { };

            if (FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address != parameters["Address"])
            {
                throw new Exception($"Called submit with address {parameters["Address"]} while authenticated as {FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address}");
            }

            FlowTransactionResponse res = await Transactions.Submit(script, new List<CadenceBase>(arguments));

            if (res.Error != null)
            {
                return new FlowTransactionResult
                {
                    Error = res.Error,
                    ErrorMessage = res.Error.Message
                };
            }

            FlowTransactionResult txResult = null;

            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(1000);
                txResult = await Transactions.GetResult(res.Id);

                if (txResult.Error != null)
                {
                    continue;
                }

                if (txResult.Status >= waitUntil)
                {
                    return txResult;
                }
            }

            return txResult;
        }
    }
}