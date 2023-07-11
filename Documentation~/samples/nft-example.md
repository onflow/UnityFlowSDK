# Simple NFT demo

This tutorial will show you how to create, mint and list a simple NFT.  It follows the
Non Fungible Token standard (https://github.com/onflow/flow-nft/blob/master/contracts/NonFungibleToken.cdc),
but does not implement the MetadataViews interface.  If you would like to make your NFT compatible with
marketplaces, look at implementing MetadataViews (https://github.com/onflow/flow-nft/blob/master/contracts/MetadataViews.cdc)

The following are the main points of this tutorial:
1. Creating a contract that implements INFT
2. Deploying the contract
3. Listing, minting and storing NFTs defined by the contract via a transaction


### Getting started
Load the Samples/Flow SDK/x.x.x/Example NFT/Scenes/NFTExampleScene scene.
Press play and approve the transactions that come up (only on first time run)
Click Authenticate and choose the emulator_service_account.
Click Mint
Fill in the Text and URL fields and click Mint
Approve the transaction
Click List to refresh the NFT display panel and show your newly minted NFT
Repeat Mint and List as desired to make your list grow

Now we'll show you how this works.

### Creating an NFT contract

When creating an NFT it is recommended (but not required) to implement the NonFungibleToken.INFT
interface.  We will be doing so in this case.

At its simplest, an NFT on Flow is a resource with a unique id.  A Collection is a resource
that will allow you to store, list, deposit, and withdraw NFTs of a specific type.

We recommend reading through the NFT tutorial at https://developers.flow.com/cadence/tutorial/05-non-fungible-tokens-1
to understand what is happening, as well as reviewing the contents of Cadence/Contracts/SDKExampleNFT.cdc

The SDKExampleNFT minter allows for anyone to mint an SDKExampleNFT.  Typically you would restrict
minting to an authorized account.

This tutorial will not delve deeply into the NFT contract or Cadence, instead focusing on interacting
with them using the functionality the Unity SDK provides.

### Deploying the contracts

Open up Example.cs to follow along.

Our Start function looks like this:

```csharp
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
    
    //Deploy the NonFungibleToken and SDKExampleNFT contracts if they are not already deployed
    StartCoroutine(DeployContracts());
}
```

This initializes the FlowSDK to connect to the emulator, creates and registers a DevWalletProvioder, then
starts a coroutine to deploy our contract if needed.

Contracts can be deployed via the FlowControl Tools window, but we will deploy them via code for ease
of use.

The DeployContracts coroutine:

```csharp
public IEnumerator DeployContracts()
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
```

We start by waiting one second.  This ensures that the emulator has finished initializing and
the required service account has been populated.

Next we fetch the emulator_service_account Account.  This Account object will contain the contracts
that are deployed to the account.  We check if both the required contracts are deployed,
and if they are not, we deploy them.

Upon first running the scene, you will be presented with two popups by DevWallet.  This authorizes
the transactions that will deploy the contracts.  You will not see these popups during subsequent
runs because the contracts will already be present on the account.  If you purge the emulator
data, you will see the popups again the next time you play the scene.

When using Testnet or Mainnet, the NonFungibleToken contract will already be deployed at a known location.
Launching the emulator with the --contracts flag will also deploy this contract.  I this case we are running
without --contracts, so we will deploy the NonFungibleToken contract ourselves.

### Listing, minting, and storing NFTs

Now that the contracts are in place, the Authenticate button will be clickable.  This uses the
registered wallet provider (DevWalletProvider) to authenticate.  Unless you create another account
using the FlowControl Tools panel, only emulator_service_account will be available.

After clicking Authenticate, it will prompt you to select an account to authenticate as.  Choose
emulator_service_account.  This is done with the following functions:

```csharp
    public void Authenticate()
    {
        FlowSDK.GetWalletProvider().Authenticate("", OnAuthSuccess, OnAuthFailed);
    }
    
    private void OnAuthFailed()
    {
        Debug.LogError("Authentication failed!");
        accountText.text = $"Account:  {FlowSDK.GetWalletProvider().GetAuthenticatedAccount()?.Address??"None"}";
        if (FlowSDK.GetWalletProvider().GetAuthenticatedAccount() == null)
        {
            mintPanelButton.interactable = false;
            listButton.interactable = false;
        }
    }

    private void OnAuthSuccess(string obj)
    {
        accountText.text = $"Account:  {FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address}";
        mintPanelButton.interactable = true;
        listButton.interactable = true;
    }
```

If authentication succeeds, a coroutine is started that will  make the Mint button available.

Clicking on the Mint button displays the Minting panel that will allow you to customize the NFT that will
be minted:

```csharp
public void ShowMintPanel()
    {
        textInputField.text = "";
        URLInputField.text = "";
        mintPanel.SetActive(true);
    }
```

### Minting
Clicking Mint in the Mint panel will trigger the creation of the NFT with the supplied text.


```csharp
public void MintNFT()
    {
        if(FlowSDK.GetWalletProvider() != null && FlowSDK.GetWalletProvider().IsAuthenticated())
        {
            StartCoroutine(MintNFTCoroutine());
        }
        
        mintPanel.SetActive(false);
    }
```

```csharp
    public IEnumerator MintNFTCoroutine()
    {
        statusText.text = "Minting...";
        List<CadenceBase> args = new List<CadenceBase>
        {
            Convert.ToCadence(new Dictionary<string, string>
            {
                ["Text"] = textInputField.text,
                ["URL"] = URLInputField.text
            }, "{String:String}")
        };

        Task<FlowTransactionResponse> txResponse = Transactions.Submit(mintTransaction.text, args);
        
        while(!txResponse.IsCompleted)
        {
            yield return null;
        }

        if (txResponse.Result.Error != null)
        {
            statusText.text = "Error, see log";
            Debug.LogError(txResponse.Result.Error.Message);
            yield break;
        }

        Task<FlowTransactionResult> txResult = Transactions.GetResult(txResponse.Result.Id);

        while (!txResult.IsCompleted)
        {
            yield return null;
        }

        if (txResult.Result.Error != null)
        {
            statusText.text = "Error, see log";
            Debug.LogError(txResult.Result.Error.Message);
            yield break;
        }
        
        statusText.text = "";
    }
```

Because transactions can take a while, they are done in coroutines to prevent the interface from locking
up.

First we construct a list of arguments we are going to pass to the transaction in MintAndSave.cdc.  This
list consists of a single Dictionary containing the "Text" and "URL" keys and String values from the Mint
panel.  We use Cadence.Convert to convert from a Dictionary<string, string> into a Cadence {String:String}
for the argument.

The MintAndSave.cdc file contains the transaction that will be executed.

```cadence
import SDKExampleNFT from 0xf8d6e0586b0a20c7
import NonFungibleToken from 0xf8d6e0586b0a20c7

transaction(md: {String:String}) {
    let acct : AuthAccount
    
    prepare(signer: AuthAccount) {
        self.acct = signer
    }
    
    execute {
        // Create collection if it doesn't exist
        if self.acct.borrow<&SDKExampleNFT.Collection>(from: SDKExampleNFT.CollectionStoragePath) == nil
        {
            // Create a new empty collection
            let collection <- SDKExampleNFT.createEmptyCollection()
            // save it to the account
            self.acct.save(<-collection, to: SDKExampleNFT.CollectionStoragePath)
            // link a public capability for the collection
            self.acct.link<&{SDKExampleNFT.CollectionPublic, NonFungibleToken.CollectionPublic}>(
                SDKExampleNFT.CollectionPublicPath,
                target: SDKExampleNFT.CollectionStoragePath
            )
        }
        
        //Get a reference to the minter
        let minter = getAccount(0xf8d6e0586b0a20c7)
            .getCapability(SDKExampleNFT.MinterPublicPath)
            .borrow<&{SDKExampleNFT.PublicMinter}>()
        
        
        //Get a CollectionPublic reference to the collection
        let collection = self.acct.getCapability(SDKExampleNFT.CollectionPublicPath)
            .borrow<&{NonFungibleToken.CollectionPublic}>()
              
        //Mint a new NFT and deposit into the authorizers account
        minter?.mintNFT(recipient: collection!, metadata: md)
    }
}
```

This transaction checks to see if an SDKExampleNFT collection exists on the account, creating/saving/linking it if it does
not.  Then it calls the contract to mint a new NFT with the desired metadata and saves it to the collection.

### Listing NFTs

The List button calls the UpdateNFTPanelCoroutine function that is responsible for populating the panel with information
about the SDKExampleNFT resources in the account you are authenticated as.

```csharp
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
        foreach (KeyValuePair<string,string> pair in nft.Value)
        {
            text += $"    {pair.Key}: {pair.Value}\n";
        }
        
        prefab.GetComponentInChildren<TMP_Text>().text = text;
    }
}
```

When running a script, you can query any account.  In this case we will only query the account
that is authenticated with the wallet provider.

It executes the script defined in GetNFTsOnAccount.cdc:

```cadence
import SDKExampleNFT from 0xf8d6e0586b0a20c7

pub fun main(addr:Address): {UInt64:{String:String}} {

    //Get a capability to the SDKExampleNFT collection if it exists.  Return an empty dictionary if it does not
    let collectionCap = getAccount(addr).getCapability<&{SDKExampleNFT.CollectionPublic}>(SDKExampleNFT.CollectionPublicPath)
    if(collectionCap == nil)
    {
        return {}
    }
    
    //Borrow a reference to the capability, returning an empty dictionary if it can not borrow
    let collection = collectionCap.borrow()
    if(collection == nil)
    {
        return {}
    }

    //Create a variable to store the information we extract from the NFTs
    var output : {UInt64:{String:String}} = {}
    
    //Iterate through the NFTs, extracting id and metadata from each.
    for id in collection?.getIDs()! {
        log(collection!.borrowSDKExampleNFT(id:id))
        log(collection!.borrowSDKExampleNFT(id:id)!.metadata)
        output[id] = collection!.borrowSDKExampleNFT(id:id)!.metadata;
    }
    
    //Return the constructed data
    return output
}
```

This ensures that an SDKExampleNFT.Collection resource exists at the proper path, then creates and returns
a ```{UInt64:{String:String}}``` containing the information of all SDKExampleNFTs in the collection.  We use
Cadence.Convert to convert this into a C# ```Dictionary<UInt64, Dictionary<string,string>>```

After that we Instantiate prefabs to display the data of each of the returned NFTs.