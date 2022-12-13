using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace QuickstartGuide
{
    public class FlowControlExample : MonoBehaviour
    {
        public Text outputText;
        //Make Start a coroutine so we don't lock up the editor while this is running.
        private IEnumerator Start()
        {
            #region Emulator Check

            outputText.text += "Checking for emulator:  ";
            //Wait up to 2.5 seconds for the emulator to start.
            int waited = 0;

            while (!FlowControl.IsEmulatorRunning && waited < 5)
            {
                waited++;
                yield return new WaitForSeconds(.5f);
            }

            if (!FlowControl.IsEmulatorRunning)
            {
                Debug.LogError("Emulator not running.");
                yield break;
            }

            outputText.text += "FOUND\n\n";

            #endregion

            #region Create script only FlowControl.Account
            outputText.text += "Creating read only account:  ";
            //No account is required to execute scripts on Flow.  FlowControl requires an account only to know which
            //network to connect to.
            FlowControl.Account scriptOnlyAccount = new FlowControl.Account
            {
                GatewayName = "Emulator"
            };

            outputText.text += "DONE\n\n";
            #endregion

            #region Execute Script Example

            outputText.text += "Executing script:  ";
            const string code = @"pub fun main(message: String): Int{
                log(message)
                return 42
            }";

            //Execute the script
            Task<FlowScriptResponse> task = scriptOnlyAccount.ExecuteScript(code, new CadenceString("Test"));

            //Wait until it has been executed.
            yield return new WaitUntil(() => task.IsCompleted);

            //Check the result for errors
            if (task.Result.Error != null)
            {
                Debug.LogError($"Error:  {task.Result.Error.Message}");
                yield break;
            }

            //Output the result, casting the returned value to a CadenceNumber.
            Debug.Log($"Script result: {task.Result.Value.As<CadenceNumber>().Value}");
            outputText.text += $"Result:  {task.Result.Value.As<CadenceNumber>().Value}\n\n";

            #endregion

            #region Create SdkAccount and FlowControl.Account Example
            outputText.text += "Creating SdkAccount:  ";

            FlowSDK.RegisterWalletProvider(ScriptableObject.CreateInstance<DevWalletProvider>());

            string authAddress = "";
            FlowSDK.GetWalletProvider().Authenticate("", (string address) =>
            {
                authAddress = address;
            }, null);

            yield return new WaitUntil(() => { return authAddress != ""; });

            //Convert FlowAccount to SdkAccount
            SdkAccount emulatorSdkAccount = FlowControl.GetSdkAccountByAddress(authAddress);
            if (emulatorSdkAccount == null)
            {
                Debug.LogError("Error getting SdkAccount for emulator_service_account");
                yield break;
            }

            //Create a new account with the name "User"
            Task<SdkAccount> newAccountTask = CommonTransactions.CreateAccount("User");
            yield return new WaitUntil(() => newAccountTask.IsCompleted);

            if (newAccountTask.Result.Error != null)
            {
                Debug.LogError($"Error creating new account: {newAccountTask.Result.Error.Message}");
                yield break;
            }

            outputText.text += "DONE\n\n";

            //Here we have an SdkAccount
            SdkAccount userSdkAccount = newAccountTask.Result;

            outputText.text += "Creating FlowControl Account:  ";
            //Create FlowControl.Account for this user from the SdkAccount data
            FlowControl.Account userAccount = new FlowControl.Account
            {
                Name = userSdkAccount.Name,
                GatewayName = "Emulator",
                AccountConfig = new Dictionary<string, string>
                {
                    ["Address"] = userSdkAccount.Address,
                    ["Private Key"] = userSdkAccount.PrivateKey
                }
            };

            //Save the user so it can be found later
            FlowControl.Data.Accounts.Add(userAccount);

            outputText.text += "DONE\n\n";
            Debug.Log("Created User account");
            #endregion

            #region Deploy Contract Example

            outputText.text += "Deploying Contract:  ";
            //Simple test contract
            const string contractCode = @"
                pub contract HelloWorld {
                    pub let greeting: String
        
                    pub event TestEvent(field: String)
        
                    init() {
                        self.greeting = ""Hello, World!""
                    }
        
                    pub fun hello(data: String): String {
                        emit TestEvent(field:data)
                        return self.greeting
                    }
                }";


            FlowSDK.GetWalletProvider().Authenticate(userAccount.Name, null, null);
            //Deploy contract to User account
            Task<FlowTransactionResponse> deployContractTask = CommonTransactions.DeployContract("HelloWorld", contractCode);

            yield return new WaitUntil(() => deployContractTask.IsCompleted);

            if (deployContractTask.Result.Error != null)
            {
                Debug.LogError($"Error deploying contract: {deployContractTask.Result.Error.Message}");
                yield break;
            }

            Debug.Log("Deployed Contract");
            outputText.text += "DONE\n\n";

            #endregion

            #region Add Text Replacement
            outputText.text += "Add text replacement:  ";
            FlowControl.TextReplacement newTextReplacement = new FlowControl.TextReplacement
            {
                description = "User Address",
                originalText = "%USERADDRESS%",
                replacementText = userSdkAccount.Address,
                active = true,
                ApplyToAccounts = new List<string> { "User" },
                ApplyToGateways = new List<string> { "Emulator" }
            };

            FlowControl.Data.TextReplacements.Add(newTextReplacement);
            outputText.text += "DONE\n\n";
            #endregion

            #region Execute Transaction Example
            outputText.text += "Submitting Transaction:  ";
            //Simple transaction that utilizes the deployed contract and text replacement
            string transaction = @"
                import HelloWorld from %USERADDRESS% 
                transaction {
                    prepare(acct: AuthAccount) {
                        log(""Transaction Test"")
                        HelloWorld.hello(data:""Test Event"")
                    }
                }";

            Task<FlowTransactionResult> transactionTask = userAccount.SubmitAndWaitUntilSealed(transaction);
            outputText.text += "Submitted\n\n";
            yield return new WaitUntil(() => transactionTask.IsCompleted);
            outputText.text += "Waiting for transaction to seal:  ";
            if (transactionTask.Result.Error != null || !string.IsNullOrEmpty(transactionTask.Result.ErrorMessage))
            {
                Debug.LogError($"Error executing transaction: {transactionTask.Result.Error?.Message ?? transactionTask.Result.ErrorMessage}");
                yield break;
            }
            outputText.text += "DONE\n\n";

            outputText.text += "Getting events:  ";
            FlowEvent txEvent = transactionTask.Result.Events.Find(x => x.Type.Contains("TestEvent"));

            //Show that the transaction finished and display the value of the event that was emitted during execution.
            //The Payload of the returned FlowEvent will be a CadenceComposite.  We want the value associated with the
            //"field" field as a string
            Debug.Log($"Executed transaction.  Event type: {txEvent.Type}.  Event payload: {txEvent.Payload.As<CadenceComposite>().CompositeFieldAs<CadenceString>("field").Value}");
            outputText.text += $"Done.  Event contents:  {txEvent.Payload.As<CadenceComposite>().CompositeFieldAs<CadenceString>("field").Value}\n\n";
            #endregion
            outputText.text += "Demo complete";
            Debug.Log("Demo complete");

            //Cleanup:  Remove all created data;
            FlowControl.Data.Accounts.Remove(userAccount);
            FlowControl.Data.TextReplacements.Clear();
        }
    }
}