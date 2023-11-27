# FCL

## Introduction

FCL (Flow Client Library) is a wallet provider which implements the `IWallet` interface, and also implements the [FCL specification](https://github.com/onflow/flips/blob/main/application/20221108-fcl-specification.md). It's not an actual wallet itself - it queries Flow's Discovery Service for Flow supported wallets on the given platform and allows users to connect the game to one of these supported wallets. FCL then facilitates interactions between the game and the wallet. 

## What are Wallets? 

A wallet is a piece of software or hardware that stores the private key associated with a Flow account. The term *custodian* is used to refer to the party that stores the private key. Hardware Wallets (eg Ledger), typically USB devices, allow users to be their own custodian, whereas hosted software wallets (eg Dapper Wallet) act as the custodian on behalf of the user. 

For more information about Wallets and Flow accounts, see [https://developers.flow.com/flow/dapp-development/user-accounts-and-wallets](https://developers.flow.com/flow/dapp-development/user-accounts-and-wallets). 

## What is a Wallet Provider? 

In terms of the Flow SDK for Unity, a Wallet Provider is a class which implements the `IWallet` interface and allows users to interact with specific hardware or software wallets. This includes authenticating with a wallet, retrieving the user's Flow account address from the wallet, and requesting the wallet to sign transactions on behalf of the user. 

As of v3.0.0, the Flow SDK for Unity contains four wallet providers: 

- [Dev Wallet](https://developers.flow.com/tools/unity-sdk/guides/dev-wallet) - a mock wallet provider to make development easier
- [Wallet Connect](https://developers.flow.com/tools/unity-sdk/guides/wallet-connect) - connects to wallets which support Wallet Connect
- [Niftory](https://developers.flow.com/tools/unity-sdk/guides/niftory-wallet) -  a web2 middleware wallet which provides out-of-the-box walletless onboarding
- [FCL](https://developers.flow.com/tools/unity-sdk/guides/fcl) - connects to Flow supported wallets via the FCL standard

You can also implement your own wallet provider by implementing the `IWallet` interface. 

## How to implement FCL

To implement FCL, you must register the FclProvider with the SDK like any other wallet provider. However, to support wallets which use Wallet Connect, you must also register your project in the Wallet Connect dashboard to obtain a Project ID. 

### Obtain Wallet Connect Project ID

1. Go to [https://cloud.walletconnect.com/sign-in](https://cloud.walletconnect.com/sign-in) and sign in, or create an account if you don't have one. 
2. Click on New Project and provide a name. 
3. Copy the Project ID. 

### Registering

Create an instance of FclProvider and initialize it with the required config, then register it with the Flow SDK. Here is an example:

```csharp
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Fcl;
using DapperLabs.Flow.Sdk.Crypto;

IWallet walletProvider = new FclProvider();
walletProvider.Init(new FclConfig
{
    Description = "An example project to showcase Wallet Connect", // a description for your project
    IconUri = "https://walletconnect.com/meta/favicon.ico", // URL for an icon for your project
    Location = "",
    Title = "Flow Unity Example", // the name of your project
    Url = "https://dapperlabs.com" // URL for your project
    WalletConnectProjectId = "xxxxxxxxxxxxxxxxxxxxx" // the Project ID from the previous step
});
FlowSDK.RegisterWalletProvider(walletProvider);
```

The description, icon, name and URL that you provide will appear in your user's wallet apps, when they connect the game to their wallet. 

### Authenticating

The `IWallet.Authenticate` method is as follows: 

```csharp
public void Authenticate(string username, System.Action<string> OnAuthSuccess, System.Action OnAuthFailed);
```

`username` is ignored in the FCL provider. 
`OnAuthSuccess` is a function that will be called when you have successfully authenticated with a wallet. The callback function must take a `string` argument, which will contain the authenticated account's Flow address. 
`OnAuthFailed` is a function that will be called if authentication failed, for example if the user rejected the request in their wallet app. 

Here is an example of authenticating from game code: 

```csharp
FlowSDK.GetWalletProvider().Authenticate("", (string flowAddress) => 
{
    Debug.Log($"Authenticated - Flow account address is {flowAddress}");
}, () => 
{
    Debug.Log("Authentication failed.");
});
```

### Signing Transactions

If you are using the Flow SDK to sign transactions then you do not need to worry about this, as it is handled automatically. When you submit a transaction, the SDK will request FCL to sign the transaction as the authenticated user. The user will receive a notification in their wallet app to approve the transaction. 

For full disclosure, here are the methods on the `IWallet` interface to sign a transaction: 

```csharp
public Task<byte[]> SignTransactionPayload(FlowTransaction txn);

public Task<byte[]> SignTransactionEnvelope(FlowTransaction txn);
```

In Flow, there are two parts of a transaction that can be signed - the Payload and the Authorization Envelope. The envelope must always be signed, and is the last thing to be signed by the Payer of the transaction fees. The Payload is only signed by the Proposer and\or the Authorizers IF they are not also the Payer (i.e. nobody signs the transaction twice). For more information on transaction signing, see [https://developers.flow.com/learn/concepts/transaction-signing](https://developers.flow.com/learn/concepts/transaction-signing). 

The following is an example of how to call `SignTransactionEnvelope`, but as mentioned, this is automatically done by the SDK's `Transactions.Submit` function. It is asynchronous so is therefore `await`ed, and returns the signature as a byte array. 

```csharp
byte[] signature = await FlowSDK.GetWalletProvider().SignTransactionEnvelope(txRequest);
```

## What your users will see

When your game calls `Authenticate`, a dialog box will appear, showing a list of Flow supported wallets that the user can choose from. What the user sees next depends on which wallet they select and what platform they are on, but they will go through a process to connect the game to the wallet that they chose. 
