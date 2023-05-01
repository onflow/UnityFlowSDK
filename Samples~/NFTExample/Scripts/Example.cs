using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Unity;
using TMPro;
using UnityEngine.UI;
using Convert = DapperLabs.Flow.Sdk.Cadence.Convert;


namespace NFTExample
{
    public class Example : MonoBehaviour
    {
        //The Cadence files that will be used

        //The transaction that mints and saves an NFT
        public CadenceTransactionAsset mintTransaction;

        //Script that queries a collection on an account to list SDKExampleNFTs
        public CadenceScriptAsset listScript;

        //The contract defining our SDKExampleNFT NFT.
        public CadenceContractAsset SDKExampleNFTContract;
        public CadenceContractAsset NonFungibleTokenContract;

        public GameObject NFTContentPanel;
        public GameObject NFTPrefab;

        public TMP_Text accountText;
        public TMP_Text statusText;

        public TMP_InputField textInputField;
        public TMP_InputField URLInputField;

        public GameObject mintPanel;
        public Button mintPanelButton;
        public Button authenticateButton;
        public Button listButton;

        public void Start()
        {
            //Initialize the FlowSDK, connecting to an emulator using HTTP
            FlowSDK.Init(new FlowConfig
            {
                NetworkUrl = FlowControl.Data.EmulatorSettings.emulatorEndpoint,
                Protocol = FlowConfig.NetworkProtocol.HTTP
            });

            //Register the DevWallet provider that we will be using
            FlowSDK.RegisterWalletProvider(new DevWalletProvider());
            
        }

        public void DeployContracts()
        {
            //Deploy the SDKExampleNFT contract if it is not already deployed
            StartCoroutine(DeployContractsCoroutine());
        }

        // Mints an NFT and stores it on the authenticated account.
        public void MintNFT()
        {
            if (FlowSDK.GetWalletProvider() != null && FlowSDK.GetWalletProvider().IsAuthenticated())
            {
                StartCoroutine(MintNFTCoroutine());
            }

            mintPanel.SetActive(false);
        }

        //Authenticate with the registered wallet provider
        public void Authenticate()
        {
            FlowSDK.GetWalletProvider().Authenticate("", OnAuthSuccess, OnAuthFailed);
        }

        //Called when authentication completes successfully
        private void OnAuthFailed()
        {
            Debug.LogError("Authentication failed!");
            accountText.text = $"Account:  {FlowSDK.GetWalletProvider().GetAuthenticatedAccount()?.Address ?? "None"}";
            if (FlowSDK.GetWalletProvider().GetAuthenticatedAccount() == null)
            {
                mintPanelButton.interactable = false;
                listButton.interactable = false;
            }
        }

        //Called if there is an error authenticating
        private void OnAuthSuccess(string obj)
        {
            accountText.text = $"Account:  {FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address}";
            mintPanelButton.interactable = true;
            listButton.interactable = true;
        }

        //Displays the minting panel
        public void ShowMintPanel()
        {
            textInputField.text = "";
            URLInputField.text = "";
            mintPanel.SetActive(true);
        }

        //Coroutine to mint a new NFT
        public IEnumerator MintNFTCoroutine()
        {
            //Display minting message
            statusText.text = "Minting...";

            //Create argument listfor transaction
            List<CadenceBase> args = new List<CadenceBase>
            {
                Convert.ToCadence(new Dictionary<string, string>
                {
                    ["Text"] = textInputField.text,
                    ["URL"] = URLInputField.text
                }, "{String:String}")
            };

            //Execute transaction
            Task<FlowTransactionResponse> txResponse = Transactions.Submit(mintTransaction.text, args);

            //Wait for transaction to submit
            while (!txResponse.IsCompleted)
            {
                yield return null;
            }

            //Ensure there were no errors submitting transaction
            if (txResponse.Result.Error != null)
            {
                statusText.text = "Error, see log";
                Debug.LogError(txResponse.Result.Error.Message);
                yield break;
            }

            //Get the result of the transaction to ensure it's completed
            Task<FlowTransactionResult> txResult = Transactions.GetResult(txResponse.Result.Id);

            //Wait until it is completed
            while (!txResult.IsCompleted)
            {
                yield return null;
            }

            //Ensure there were no errors
            if (txResult.Result.Error != null)
            {
                statusText.text = "Error, see log";
                Debug.LogError(txResult.Result.Error.Message);
                yield break;
            }

            //Remove minting message
            statusText.text = "";
        }

        //Creates a coroutine to update the NFT list panel
        public void UpdateNFTPanel()
        {
            StartCoroutine(UpdateNFTPanelCoroutine());
        }

        //Updates the NFT panel list of NFTs in a coroutine
        public IEnumerator UpdateNFTPanelCoroutine()
        {
            //Create the script request.  We use the text in the GetNFTsOnAccount.cdc file and pass the address of the
            //authenticated account as the address of the account we want to query.
            FlowScriptRequest scriptRequest = new FlowScriptRequest
            {
                Script = listScript.text,
                Arguments = new List<CadenceBase>
                {
                    new CadenceAddress(FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address)
                }
            };

            //Execute the script and wait until it is completed.
            Task<FlowScriptResponse> scriptResponse = Scripts.ExecuteAtLatestBlock(scriptRequest);
            yield return new WaitUntil(() => scriptResponse.IsCompleted);

            //Destroy existing NFT display prefabs
            foreach (TMP_Text child in NFTContentPanel.GetComponentsInChildren<TMP_Text>())
            {
                Destroy(child.transform.parent.gameObject);
            }

            //Iterate over the returned dictionary
            Dictionary<ulong, Dictionary<string, string>> results = Convert.FromCadence<Dictionary<UInt64, Dictionary<string, string>>>(scriptResponse.Result.Value);
            //Iterate over the returned dictionary
            foreach (KeyValuePair<ulong, Dictionary<string, string>> nft in results)
            {
                //Create a prefab for the NFT
                GameObject prefab = Instantiate(NFTPrefab, NFTContentPanel.transform);

                //Set the text
                string text = $"ID:  {nft.Key}\n";
                foreach (KeyValuePair<string, string> pair in nft.Value)
                {
                    text += $"    {pair.Key}: {pair.Value}\n";
                }

                prefab.GetComponentInChildren<TMP_Text>().text = text;
            }
        }

        // Deploy contracts as needed (upon first run, or after a data purge)
        public IEnumerator DeployContractsCoroutine()
        {
            statusText.text = "Verifying contracts";
            //Wait 1 second to ensure emulator has started up and service account information has been captured.
            yield return new WaitForSeconds(1.0f);

            //Get the address of the emulator_service_account, then get an account object for that account. 
            Task<FlowAccount> accountTask = Accounts.GetByAddress(FlowControl.Data.Accounts.Find(acct => acct.Name == "emulator_service_account").AccountConfig["Address"]);
            //Wait until the account fetch is complete
            yield return new WaitUntil(() => accountTask.IsCompleted);

            //Check for errors.
            if (accountTask.Result.Error != null)
            {
                Debug.LogError(accountTask.Result.Error.Message);
                Debug.LogError(accountTask.Result.Error.StackTrace);
            }

            //We now have an Account object, which contains the contracts deployed to that account.  Check if the NonFungileToken and SDKExampleNFT contracts are deployed
            if (!accountTask.Result.Contracts.Exists(x => x.Name == "SDKExampleNFT") || !accountTask.Result.Contracts.Exists(x => x.Name == "NonFungibleToken"))
            {
                statusText.text = "Deploying contracts,\napprove transactions";

                //First authenticate as the emulator_service_account using DevWallet
                FlowSDK.GetWalletProvider().Authenticate("emulator_service_account", null, null);

                //Ensure that we authenticated properly
                if (FlowSDK.GetWalletProvider().GetAuthenticatedAccount() == null)
                {
                    Debug.LogError("No authenticated account.");
                    yield break;
                }

                //Deploy the NonFungibleToken contract
                Task<FlowTransactionResponse> txResponse = CommonTransactions.DeployContract("NonFungibleToken", NonFungibleTokenContract.text);
                yield return new WaitUntil(() => txResponse.IsCompleted);
                if (txResponse.Result.Error != null)
                {
                    Debug.LogError(txResponse.Result.Error.Message);
                    Debug.LogError(txResponse.Result.Error.StackTrace);
                    yield break;
                }

                //Wait until the transaction finishes executing
                Task<FlowTransactionResult> txResult = Transactions.GetResult(txResponse.Result.Id);
                yield return new WaitUntil(() => txResult.IsCompleted);
                
                //Deploy the SDKExampleNFT contract
                txResponse = CommonTransactions.DeployContract("SDKExampleNFT", SDKExampleNFTContract.text);
                yield return new WaitUntil(() => txResponse.IsCompleted);
                if (txResponse.Result.Error != null)
                {
                    Debug.LogError(txResponse.Result.Error.Message);
                    Debug.LogError(txResponse.Result.Error.StackTrace);
                    yield break;
                }

                //Wait until the transaction finishes executing
                txResult = Transactions.GetResult(txResponse.Result.Id);
                yield return new WaitUntil(() => txResult.IsCompleted);

                //Unauthenticate as the emulator_service_account
                FlowSDK.GetWalletProvider().Unauthenticate();
            }

            //Enable the Authenticate button.
            authenticateButton.interactable = true;
            statusText.text = "";
        }
    }
}