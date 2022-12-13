using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.Unity;
using DapperLabs.Flow.Sdk.DevWallet;

namespace FlowSDKDemo
{
    /// <summary>
    /// A simple controller for switching between UI panels to demonstrate the various API calls within the SDK. 
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        [Header("Blocks")]
        // Block inputs
        public GameObject BlocksGetByIdInput;
        public GameObject BlocksGetByHeightInput;

        // Block results
        public GameObject BlocksResultId;
        public GameObject BlocksResultParentId;
        public GameObject BlocksResultHeight;
        public GameObject BlocksResultTimestamp;
        public GameObject BlocksResultSignatures;
        public GameObject BlocksResultSeals;
        public GameObject BlocksResultCollGuarantees;

        [Header("Collections")]
        // Collection inputs
        public GameObject CollectionsGetByIdInput;

        // Collection results
        public GameObject CollectionsResultId;
        public GameObject CollectionsResultTxIds;

        [Header("Events")]
        // Event inputs
        public GameObject EventsGetByHeightTypeInput;
        public GameObject EventsGetByHeightStartInput;
        public GameObject EventsGetByHeightEndInput;
        public GameObject EventsGetByBlockIdsTypeInput;
        public GameObject EventsGetByBlockIdsInput;

        // Event results
        public GameObject EventsResultDropdown;
        public GameObject EventsResultBlockId;
        public GameObject EventsResultBlockHeight;
        public GameObject EventsResultBlockTimestamp;
        public GameObject EventsResultBlockEvents;

        [Header("Scripts")]
        // Script inputs
        public GameObject ScriptsArg1;
        public GameObject ScriptsArg2;
        public GameObject ScriptsAddressArg;
        public GameObject ScriptsPrintNftsAddrArg;
        public TextAsset CadenceGetTokensScript;
        public TextAsset CadencePrintNftsScript;
        public TextAsset CadenceGetNftsForSaleScript;

        // Script results
        public GameObject ScriptsResult;

        [Header("Transactions")]
        // Transaction inputs
        public GameObject TxSignedInAccount;
        public GameObject TxSignInBtn;
        public GameObject TxArgsString;
        public GameObject TxArgsNumber;
        public GameObject TxArgsNftId;
        public GameObject TxArgsNftPrice;
        public GameObject TxIdInput;
        public TextAsset CadenceListNftScript;
        public GameObject TxSubmitButton1;
        public GameObject TxSubmitButton2;
        public GameObject TxSubmitButton3;

        // Transaction results
        public GameObject TxResultId;
        public GameObject TxResultScript;
        public GameObject TxResultRefBlockId;
        public GameObject TxResultGasLimit;
        public GameObject TxResultArgs;
        public GameObject TxResultAuthorizers;
        public GameObject TxResultPayloadSigs;
        public GameObject TxResultEnvelopeSigs;
        public GameObject TxResultPayer;
        public GameObject TxResultProposer;
        public GameObject TxResultStatus;
        public GameObject TxResultStatusCode;
        public GameObject TxResultError;
        public GameObject TxResultEvents;

        [Header("Accounts")]
        // Account inputs
        public GameObject AcctSignedInAccount;
        public GameObject AcctSignInBtn;
        public GameObject AcctGetAddressIpt;
        public GameObject AcctCreateAddressIpt;
        public GameObject AcctDeployNameIpt;
        public GameObject AcctDeploySourceIpt;
        public GameObject AcctRemoveNameIpt;
        public GameObject AcctUpdateNameIpt;
        public GameObject AcctUpdateSourceIpt;
        public GameObject AcctCreateAccountBtn;
        public GameObject AcctDeployContractBtn;
        public GameObject AcctRemoveContractBtn;
        public GameObject AcctUpdateContractBtn;

        // Account results
        public GameObject AcctResultAddress;
        public GameObject AcctResultBalance;
        public GameObject AcctResultKeys;
        public GameObject AcctResultContracts;

        [Header("Misc")]
        public GameObject[] panels;

        List<FlowEventGroup> eventGroupsResult = null;

        void SetActivePanel(int index)
        {
            for (var i = 0; i < panels.Length; i++)
            {
                var active = i == index;
                var g = panels[i];
                if (g.activeSelf != active) g.SetActive(active);
            }
        }

        void OnEnable()
        {
            SetActivePanel(0);
        }

        void Start()
        {
            // Initialize FlowSDK. You must pass in a FlowConfig object which specifies which protocol
            // and Url to connect to (only HTTP is currently supported). 
            // By default, connects to the emulator on localhost. 

            FlowConfig flowConfig = new FlowConfig();

            flowConfig.NetworkUrl = FlowControl.Data.EmulatorSettings.emulatorEndpoint ?? "http://localhost:8888/v1";             // local emulator
            //flowConfig.NetworkUrl = "https://rest-testnet.onflow.org/v1";  // testnet
            //flowConfig.NetworkUrl = "https://rest-mainnet.onflow.org/v1";  // mainnet

            flowConfig.Protocol = FlowConfig.NetworkProtocol.HTTP;
            FlowSDK.Init(flowConfig);
            FlowSDK.RegisterWalletProvider(ScriptableObject.CreateInstance<DevWalletProvider>());
        }

        /// <summary>
        /// Demonstrates calling the Blocks.GetById() API. 
        /// </summary>
        public async void BlocksGetById()
        {
            string blockId = BlocksGetByIdInput.GetComponent<InputField>().text;

            FlowBlock block = await Blocks.GetById(blockId);

            PrintBlocksResult(block);
        }

        /// <summary>
        /// Demonstrates calling the Blocks.GetByHeight() API. 
        /// </summary>
        public async void BlocksGetByHeight()
        {
            ulong blockHeight = ulong.Parse(BlocksGetByHeightInput.GetComponent<InputField>().text);

            FlowBlock block = await Blocks.GetByHeight(blockHeight);

            PrintBlocksResult(block);
        }

        /// <summary>
        /// Demonstrates calling the Blocks.GetLatest() API. 
        /// </summary>
        public async void BlocksGetLatest()
        {
            FlowBlock block = await Blocks.GetLatest();

            PrintBlocksResult(block);
        }

        void PrintBlocksResult(FlowBlock block)
        {
            if (block != null)
            {
                if (block.Error != null)
                {
                    Debug.LogError(block.Error.Message);
                    return;
                }

                BlocksResultId.GetComponent<Text>().text = block.Id;
                BlocksResultParentId.GetComponent<Text>().text = block.ParentId;
                BlocksResultHeight.GetComponent<Text>().text = block.Height.ToString();
                BlocksResultTimestamp.GetComponent<Text>().text = block.Timestamp.ToString();

                string signatures = "";
                foreach (string sig in block.Signatures)
                {
                    signatures += $"{sig}\n";
                }
                BlocksResultSignatures.GetComponent<Text>().text = signatures;

                string blockSealIds = "";
                foreach (var seal in block.BlockSeals)
                {
                    blockSealIds += $"{seal.BlockId}\n";
                }
                BlocksResultSeals.GetComponent<Text>().text = blockSealIds;

                string collGuaranteeIds = "";
                foreach (var coll in block.CollectionGuarantees)
                {
                    collGuaranteeIds += $"{coll.CollectionId}\n";
                }
                BlocksResultCollGuarantees.GetComponent<Text>().text = collGuaranteeIds;
            }
        }

        /// <summary>
        /// Demonstrates calling the Collections.GetById() API. 
        /// </summary>
        public async void CollectionsGetById()
        {
            string collectionId = CollectionsGetByIdInput.GetComponent<InputField>().text;

            FlowCollection collection = await Collections.GetById(collectionId);

            PrintCollectionsResult(collection);
        }

        void PrintCollectionsResult(FlowCollection collection)
        {
            if (collection != null)
            {
                if (collection.Error != null)
                {
                    Debug.LogError(collection.Error.Message);
                    return;
                }

                CollectionsResultId.GetComponent<Text>().text = collection.Id;

                string transactionIds = "";
                foreach (string transactionId in collection.TransactionIds)
                {
                    transactionIds += $"{transactionId}\n";
                }
                CollectionsResultTxIds.GetComponent<Text>().text = transactionIds;
            }
        }

        /// <summary>
        /// Demonstrates calling the Events.GetForBlockHeightRange() API. 
        /// </summary>
        public async void EventsGetForHeightRange()
        {
            string type = EventsGetByHeightTypeInput.GetComponent<InputField>().text;
            string startHeightInput = EventsGetByHeightStartInput.GetComponent<InputField>().text;
            string endHeightInput = EventsGetByHeightEndInput.GetComponent<InputField>().text;

            if (type.Length == 0)
            {
                Debug.LogError("Event Type can not be blank.");
                return;
            }

            ulong startHeight;
            ulong endHeight;
                
            try
            {
                startHeight = ulong.Parse(startHeightInput);
            }
            catch
            {
                Debug.LogError("Start Block Height is not a valid integer");
                return;
            }

            try
            {
                endHeight = ulong.Parse(endHeightInput);
            }
            catch
            {
                Debug.LogError("End Block Height is not a valid integer");
                return;
            }

            eventGroupsResult = await Events.GetForBlockHeightRange(type, startHeight, endHeight);

            EventsResultDropdown.GetComponent<Dropdown>().ClearOptions();
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (FlowEventGroup eventGroup in eventGroupsResult)
            {
                options.Add(new Dropdown.OptionData(eventGroup.BlockHeight.ToString()));
            }
            EventsResultDropdown.GetComponent<Dropdown>().AddOptions(options);

            OnEventsResultDropdownChanged(0);
        }

        /// <summary>
        /// Demonstrates calling the Events.GetForBlockIds() API. 
        /// </summary>
        public async void EventsGetForBlockIds()
        {
            string type = EventsGetByBlockIdsTypeInput.GetComponent<InputField>().text;
            string blockIdsStr = EventsGetByBlockIdsInput.GetComponent<InputField>().text;

            List<string> blockIds = blockIdsStr.Split(',').ToList();

            eventGroupsResult = await Events.GetForBlockIds(type, blockIds);

            EventsResultDropdown.GetComponent<Dropdown>().ClearOptions();
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (FlowEventGroup eventGroup in eventGroupsResult)
            {
                options.Add(new Dropdown.OptionData(eventGroup.BlockHeight.ToString()));
            }
            EventsResultDropdown.GetComponent<Dropdown>().AddOptions(options);

            OnEventsResultDropdownChanged(0);
        }

        void OnEventsResultDropdownChanged(int index)
        {
            foreach(FlowEventGroup eventGroup in eventGroupsResult)
            {
                if (eventGroup.Error != null)
                {
                    Debug.LogError(eventGroup.Error.Message);
                    continue;
                }

                if (eventGroup.BlockHeight.ToString() == EventsResultDropdown.GetComponent<Dropdown>().options[index].text)
                {
                    EventsResultBlockId.GetComponent<Text>().text = eventGroup.BlockId;
                    EventsResultBlockHeight.GetComponent<Text>().text = eventGroup.BlockHeight.ToString();
                    EventsResultBlockTimestamp.GetComponent<Text>().text = eventGroup.BlockTimestamp.ToString();

                    string eventsText = "";
                    foreach(var ev in eventGroup.Events)
                    {
                        eventsText += $"Type: {ev.Type}\n";
                        eventsText += $"Tx Id: {ev.TransactionId}\n";
                        eventsText += $"Tx index: {ev.TransactionIndex}\n";
                        eventsText += $"Event index: {ev.EventIndex}\n";
                        eventsText += $"Payload: {ev.Payload}\n\n";
                    }

                    // Limit the number of characters to be drawn on the screen, otherwise if it's too
                    // large we start to get maximum vertices errors. 
                    if (eventsText.Length >= 1024)
                    {
                        eventsText = eventsText.Substring(0, 1024);
                    }

                    EventsResultBlockEvents.GetComponent<Text>().text = eventsText;
                }
            }
        }

        /// <summary>
        /// Demonstrates calling the Scripts.ExecuteAtLatestBlock() API with a script has no arguments
        /// and returns a cadence String. 
        /// </summary>
        public async void ExecuteSimpleScript()
        {
            string script = @"
pub fun main(): String {
    return ""this is a simple example script.""
}";

            FlowScriptRequest scriptReq = new FlowScriptRequest
            {
                Script = script
            };

            FlowScriptResponse response = await Scripts.ExecuteAtLatestBlock(scriptReq);

            CadenceString responseStr = (CadenceString)response.Value;

            ScriptsResult.GetComponent<Text>().text = responseStr.Value;
        }

        /// <summary>
        /// Demonstrates calling the Scripts.ExecuteAtLatestBlock() API with a script containing two cadence 
        /// UInt32 arguments and returns a cadence UInt32 value. 
        /// </summary>
        public async void ExecuteScriptWithArgs()
        {
            string script = @"
pub fun main(a: UInt32, b: UInt32): UInt32 {
    return a + b
}";

            string arg1 = ScriptsArg1.GetComponent<InputField>().text;
            string arg2 = ScriptsArg2.GetComponent<InputField>().text;

            FlowScriptRequest scriptReq = new FlowScriptRequest
            {
                Script = script
            };

            scriptReq.AddArgument(new CadenceNumber(CadenceNumberType.UInt32, arg1));
            scriptReq.AddArgument(new CadenceNumber(CadenceNumberType.UInt32, arg2));

            FlowScriptResponse response = await Scripts.ExecuteAtLatestBlock(scriptReq);

            CadenceNumber responseVal = (CadenceNumber)response.Value;

            ScriptsResult.GetComponent<Text>().text = responseVal.Value;
        }

        /// <summary>
        /// Demonstrates calling the Scripts.ExecuteAtLatestBlock() API with a script that is loaded from file (*.cdc). 
        /// The script takes a cadence Address argument and returns a cadence UFix64 value. 
        /// </summary>
        public async void ExecuteGetTokens()
        {
            string script = CadenceGetTokensScript.text;
            string arg = ScriptsAddressArg.GetComponent<InputField>().text;

            FlowScriptRequest scriptReq = new FlowScriptRequest
            {
                Script = script
            };

            scriptReq.AddArgument(new CadenceAddress(arg));

            FlowScriptResponse response = await Scripts.ExecuteAtLatestBlock(scriptReq);

            if (response.Error != null)
            {
                Debug.LogError(response.Error.Message);
                return;
            }
            
            CadenceNumber responseVal = (CadenceNumber)response.Value;

            string resultText = $"Account {arg} Tokens: {responseVal.Value}";
            ScriptsResult.GetComponent<Text>().text = resultText;
        }

        /// <summary>
        /// Demonstrates calling the Scripts.ExecuteAtLatestBlock() API with a script that returns a cadence Array
        /// of cadence UInt64 values (NFT Ids). 
        /// </summary>
        public async void ExecutePrintNfts()
        {
            string script = CadencePrintNftsScript.text;
            string arg = ScriptsPrintNftsAddrArg.GetComponent<InputField>().text;

            FlowScriptRequest scriptReq = new FlowScriptRequest
            {
                Script = script
            };

            scriptReq.AddArgument(new CadenceAddress(arg));

            FlowScriptResponse response = await Scripts.ExecuteAtLatestBlock(scriptReq);

            if (response.Error != null)
            {
                Debug.LogError(response.Error.Message);
                return;
            }

            // The script returns an array of ids, so convert the return value to an array. 
            CadenceArray responseVal = (CadenceArray)response.Value;

            string resultText = $"Account {arg} NFTs:\n";

            // Iterate the array of UInt64 values
            foreach (CadenceBase value in responseVal.Value)
            {
                CadenceNumber nftId = (CadenceNumber)value;
                resultText += $"{nftId.Value}\n";
            }

            ScriptsResult.GetComponent<Text>().text = resultText;
        }

        /// <summary>
        /// Demonstrates calling the Scripts.ExecuteAtLatestBlock() API with a script that returns a cadence Array 
        /// of structs defined in the script (cadence Composite). 
        /// </summary>
        public async void ExecuteGetNftsForSale()
        {
            string script = CadenceGetNftsForSaleScript.text;

            FlowScriptRequest scriptReq = new FlowScriptRequest
            {
                Script = script
            };

            FlowScriptResponse response = await Scripts.ExecuteAtLatestBlock(scriptReq);

            // The script returns an array of composites, so convert the return value to an array. 
            CadenceArray responseVal = (CadenceArray)response.Value;

            string resultText = "NFTs for sale:\n";

            // Iterate the array
            foreach (CadenceBase value in responseVal.Value)
            {
                // Convert to a Composite
                CadenceComposite composite = (CadenceComposite)value;

                // Retrieve the fields of the Composite by name, converting them to the specified template argument. 
                // For reference, the struct in the cadence script looks like this: 
                // pub struct NFTSale
                // {
                //     pub var id: UInt64
                //     pub var price: UFix64
                //     pub var address: Address
                // }

                CadenceNumber id = composite.CompositeFieldAs<CadenceNumber>("id");
                CadenceNumber price = composite.CompositeFieldAs<CadenceNumber>("price");
                CadenceAddress address = composite.CompositeFieldAs<CadenceAddress>("address");

                resultText += $"Id: {id.Value}    Price: {price.Value}    Owner: {address.Value}\n";
            }

            ScriptsResult.GetComponent<Text>().text = resultText;
        }

        /// <summary>
        /// Demonstrates calling the Transactions.Submit() API with a script that has no arguments. 
        /// </summary>
        public async void SubmitTxSinglePayerProposerAuthorizer()
        {
            ClearTransactionResult();

            string script = @"
transaction {
    prepare(acct: AuthAccount) {
        log(""transaction executing"");
    }
}
";

            // Using the same account as proposer, payer and authorizer. 
            FlowTransactionResponse response = await Transactions.Submit(script);
            if (response.Error != null)
            {
                Debug.LogError(response.Error.Message);
                return;
            }

            TxResultId.GetComponent<Text>().text = response.Id;

            TxIdInput.GetComponent<InputField>().text = response.Id;
        }

        /// <summary>
        /// Demonstrates calling the Transactions.Submit() API with a script containing arguments. 
        /// </summary>
        public async void SubmitTxWithArgs()
        {
            ClearTransactionResult();

            string script = @"
transaction(argString: String, argNumber: UInt32) {
    prepare(acct: AuthAccount) {
        let text1 = ""string argument: ""
        let text2 = ""number argument: ""
        log(text1.concat(argString))
        log(text2.concat(argNumber.toString()))
    }
}
";
            
            string stringArg = TxArgsString.GetComponent<InputField>().text;
            string numberArg = TxArgsNumber.GetComponent<InputField>().text;

            List<CadenceBase> args = new List<CadenceBase>();
            args.Add(new CadenceString(stringArg));
            args.Add(new CadenceNumber(CadenceNumberType.UInt32, numberArg));

            // Using the same account as proposer, payer and authorizer. 
            FlowTransactionResponse response = await Transactions.Submit(script, args);

            TxResultId.GetComponent<Text>().text = response.Id;

            TxIdInput.GetComponent<InputField>().text = response.Id;
        }

        /// <summary>
        /// Demonstrates calling the Transactions.Submit() API with a script containing arguments. 
        /// </summary>
        public async void SubmitTxListNft()
        {
            ClearTransactionResult();

            string script = CadenceListNftScript.text;

            string nftId = TxArgsNftId.GetComponent<InputField>().text;
            string nftPrice = TxArgsNftPrice.GetComponent<InputField>().text;

            List<CadenceBase> args = new List<CadenceBase>();
            args.Add(new CadenceNumber(CadenceNumberType.UInt64, nftId));
            args.Add(new CadenceNumber(CadenceNumberType.UFix64, nftPrice));

            // Using the same account as proposer, payer and authorizer. 
            FlowTransactionResponse response = await Transactions.Submit(script, args);

            TxResultId.GetComponent<Text>().text = response.Id;

            TxIdInput.GetComponent<InputField>().text = response.Id;
        }

        /// <summary>
        /// Demonstrates calling the Transactions.GetById() API. 
        /// </summary>
        public async void GetTransactionById()
        {
            ClearTransactionResult();

            string txId = TxIdInput.GetComponent<InputField>().text;

            FlowTransaction response = await Transactions.GetById(txId);

            if (response.Error != null)
            {
                Debug.LogError(response.Error.Message);
                return;
            }

            TxResultId.GetComponent<Text>().text = txId;

            string script = response.Script;
            script.Replace("\r", "");
            script.Replace("\n", "");
            TxResultScript.GetComponent<Text>().text = script;

            TxResultRefBlockId.GetComponent<Text>().text = response.ReferenceBlockId;
            TxResultGasLimit.GetComponent<Text>().text = response.GasLimit.ToString();

            string args = "";
            foreach (CadenceBase arg in response.Arguments)
            {
                args += JsonConvert.SerializeObject(arg);
                args += "\n";
            }
            TxResultArgs.GetComponent<Text>().text = args;

            string authorizers = "";
            foreach (string authorizer in response.Authorizers)
            {
                authorizers += authorizer;
                authorizers += "\n";
            }
            TxResultAuthorizers.GetComponent<Text>().text = authorizers;

            string payloadSigs = "";
            foreach (FlowTransactionSignature sig in response.PayloadSignatures)
            {
                payloadSigs += sig.Address;
                payloadSigs += "\n";
            }
            TxResultPayloadSigs.GetComponent<Text>().text = payloadSigs;

            string envelopeSigs = "";
            foreach (FlowTransactionSignature sig in response.EnvelopeSignatures)
            {
                envelopeSigs += sig.Address;
                envelopeSigs += "\n";
            }
            TxResultEnvelopeSigs.GetComponent<Text>().text = envelopeSigs;

            TxResultPayer.GetComponent<Text>().text = response.Payer;
            TxResultProposer.GetComponent<Text>().text = response.ProposalKey.Address;
        }

        void ClearTransactionResult()
        {
            TxResultId.GetComponent<Text>().text = "";
            TxResultScript.GetComponent<Text>().text = "";
            TxResultRefBlockId.GetComponent<Text>().text = "";
            TxResultGasLimit.GetComponent<Text>().text = "";
            TxResultArgs.GetComponent<Text>().text = "";
            TxResultAuthorizers.GetComponent<Text>().text = "";
            TxResultPayloadSigs.GetComponent<Text>().text = "";
            TxResultEnvelopeSigs.GetComponent<Text>().text = "";
            TxResultPayer.GetComponent<Text>().text = "";
            TxResultProposer.GetComponent<Text>().text = "";
            TxResultStatus.GetComponent<Text>().text = "";
            TxResultStatusCode.GetComponent<Text>().text = "";
            TxResultError.GetComponent<Text>().text = "";
            TxResultEvents.GetComponent<Text>().text = "";
        }

        /// <summary>
        /// Demonstrates calling the Transactions.GetResult() API. 
        /// </summary>
        public async void GetTransactionResult()
        {
            ClearTransactionResult();

            string txId = TxIdInput.GetComponent<InputField>().text;

            FlowTransactionResult response = await Transactions.GetResult(txId);

            if (response.Error != null)
            {
                Debug.LogError(response.Error.Message);
                return;
            }

            TxResultStatus.GetComponent<Text>().text = response.Status.ToString();
            TxResultStatusCode.GetComponent<Text>().text = response.StatusCode.ToString();
            TxResultError.GetComponent<Text>().text = response.ErrorMessage;

            string eventsText = "";
            foreach (var ev in response.Events)
            {
                eventsText += $"Type: {ev.Type}\n";
                eventsText += $"Tx Id: {ev.TransactionId}\n";
                eventsText += $"Tx index: {ev.TransactionIndex}\n";
                eventsText += $"Event index: {ev.EventIndex}\n";
                eventsText += $"Payload: {ev.Payload}\n\n";
            }
            TxResultEvents.GetComponent<Text>().text = eventsText;
        }

        /// <summary>
        /// Demonstrates calling the Accounts.GetByAddress() API. 
        /// </summary>
        public async void GetAccount()
        {
            ClearAccountResult();

            string address = AcctGetAddressIpt.GetComponent<InputField>().text;

            FlowAccount response = await Accounts.GetByAddress(address);

            PrintAccountResult(response);
        }

        /// <summary>
        /// Demonstrates how to create an account using the Accounts.Create() API. 
        /// </summary>
        public void CreateAccount()
        {
            ClearAccountResult();

            string name = AcctCreateAddressIpt.GetComponent<InputField>().text;

            StartCoroutine(CreateAccountCoroutine(name));
        }

        IEnumerator CreateAccountCoroutine(string name)
        {
            // Create the account
            Task<SdkAccount> newAccountTask = CommonTransactions.CreateAccount(name);

            yield return new WaitUntil(() => newAccountTask.IsCompleted);

            // Retrieve the new account details
            Task<FlowAccount> getAccountTask = Accounts.GetByAddress(newAccountTask.Result.Address);

            yield return new WaitUntil(() => getAccountTask.IsCompleted);

            PrintAccountResult(getAccountTask.Result);
        }

        void PrintAccountResult(FlowAccount account)
        {
            if (account.Error != null)
            {
                Debug.LogError(account.Error.Message);
                return;
            }

            AcctResultAddress.GetComponent<Text>().text = account.Address;
            AcctResultBalance.GetComponent<Text>().text = account.Balance.ToString();

            string keysResult = "";
            foreach (FlowAccountKey key in account.Keys)
            {
                keysResult += $"Index: {key.Id}\n";
                keysResult += $"Weight: {key.Weight}\n";
                keysResult += $"Sequence Number: {key.SequenceNumber}\n";
                keysResult += $"Revoked: {key.Revoked}\n";
                keysResult += $"Public Key: {key.PublicKey}\n\n";
            }
            AcctResultKeys.GetComponent<Text>().text = keysResult;

            string contractsResult = "";
            foreach (FlowContract contract in account.Contracts)
            {
                contractsResult += $"{contract.Name}\n";
            }
            AcctResultContracts.GetComponent<Text>().text = contractsResult;
        }

        /// <summary>
        /// Demonstrates calling the Accounts.DeployContract() API. 
        /// </summary>
        public async void DeployContract()
        {
            ClearAccountResult();

            string name = AcctDeployNameIpt.GetComponent<InputField>().text;
            string source = AcctDeploySourceIpt.GetComponent<InputField>().text;

            string script = File.ReadAllText($"Assets/{source}");

            await CommonTransactions.DeployContract(name, script);
        }

        /// <summary>
        /// Demonstrates calling the Accounts.RemoveContract() API. 
        /// </summary>
        public async void RemoveContract()
        {
            ClearAccountResult();

            string name = AcctRemoveNameIpt.GetComponent<InputField>().text;

            await CommonTransactions.RemoveContract(name);
        }

        /// <summary>
        /// Demonstrates calling the Accounts.UpdateContract() API. 
        /// </summary>
        public async void UpdateContract()
        {
            ClearAccountResult();

            string name = AcctUpdateNameIpt.GetComponent<InputField>().text;
            string source = AcctUpdateSourceIpt.GetComponent<InputField>().text;

            string script = File.ReadAllText($"Assets/{source}");

            await CommonTransactions.UpdateContract(name, script);
        }

        void ClearAccountResult()
        {
            AcctResultAddress.GetComponent<Text>().text = "";
            AcctResultBalance.GetComponent<Text>().text = "";
            AcctResultKeys.GetComponent<Text>().text = "";
            AcctResultContracts.GetComponent<Text>().text = "";
        }

        public void SignInClicked()
        {
            if (FlowSDK.GetWalletProvider().IsAuthenticated())
            {
                FlowSDK.GetWalletProvider().Unauthenticate();
                TxSignedInAccount.GetComponent<Text>().text = "<none>";
                TxSignInBtn.GetComponentInChildren<Text>().text = "Sign In";

                AcctSignedInAccount.GetComponent<Text>().text = "<none>";
                AcctSignInBtn.GetComponentInChildren<Text>().text = "Sign In";

                TxSubmitButton1.GetComponent<Button>().interactable = false;
                TxSubmitButton2.GetComponent<Button>().interactable = false;
                TxSubmitButton3.GetComponent<Button>().interactable = false;

                AcctCreateAccountBtn.GetComponent<Button>().interactable = false;
                AcctDeployContractBtn.GetComponent<Button>().interactable = false;
                AcctRemoveContractBtn.GetComponent<Button>().interactable = false;
                AcctUpdateContractBtn.GetComponent<Button>().interactable = false;
            }
            else
            {
                FlowSDK.GetWalletProvider().Authenticate("", (string authAccount) =>
                {
                    Debug.Log($"Authenticated: {authAccount}");

                    TxSignedInAccount.GetComponent<Text>().text = authAccount;
                    TxSignInBtn.GetComponentInChildren<Text>().text = "Sign Out";

                    AcctSignedInAccount.GetComponent<Text>().text = authAccount;
                    AcctSignInBtn.GetComponentInChildren<Text>().text = "Sign Out";

                    TxSubmitButton1.GetComponent<Button>().interactable = true;
                    TxSubmitButton2.GetComponent<Button>().interactable = true;
                    TxSubmitButton3.GetComponent<Button>().interactable = true;

                    AcctCreateAccountBtn.GetComponent<Button>().interactable = true;
                    AcctDeployContractBtn.GetComponent<Button>().interactable = true;
                    AcctRemoveContractBtn.GetComponent<Button>().interactable = true;
                    AcctUpdateContractBtn.GetComponent<Button>().interactable = true;
                }, () =>
                {
                    Debug.Log("Authentication failed, aborting transaction.");
                });
            }
        }
    }
}