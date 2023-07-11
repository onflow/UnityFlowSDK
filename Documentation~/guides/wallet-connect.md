# Wallet Connect

## Introduction

Wallet Connect is a production ready wallet provider which implements the `IWallet` interface. It's not an actual wallet itself - it's a bridge that connects your Unity game with Flow Wallets that support Wallet Connect. 

## What are Wallets? 

A wallet is a piece of software or hardware that stores the private key associated with a Flow account. The term *custodian* is used to refer to the party that stores the private key. Hardware Wallets (eg Ledger), typically USB devices, allow users to be their own custodian, whereas hosted software wallets (eg Dapper Wallet) act as the custodian on behalf of the user. 

For more information about Wallets and Flow accounts, see [https://developers.flow.com/flow/dapp-development/user-accounts-and-wallets](https://developers.flow.com/flow/dapp-development/user-accounts-and-wallets). 

## What is a Wallet Provider? 

In terms of the Flow SDK for Unity, a Wallet Provider is a class which implements the `IWallet` interface and allows users to interact with specific hardware or software wallets. This includes authenticating with a wallet, retrieving the user's Flow account address from the wallet, and requesting the wallet to sign transactions on behalf of the user. 

As of v2.0.0, the Flow SDK for Unity contains two wallet providers - [Dev Wallet](https://developers.flow.com/tools/unity-sdk/guides/dev-wallet) and Wallet Connect. Dev Wallet is a mock wallet provider to make development easier, while Wallet Connect connects to real wallets and is therefore used for production. You could also implement your own wallet provider by implementing the `IWallet` interface. 

## How to implement Wallet Connect

To implement Wallet Connect, you must first register your project in the Wallet Connect dashboard to obtain a Project ID, then register the provider with the Flow SDK. 

### Obtain Project ID

1. Go to [https://cloud.walletconnect.com/sign-in](https://cloud.walletconnect.com/sign-in) and sign in, or create an account if you don't have one. 
2. Click on New Project and provide a name. 
3. Copy the Project ID. 

### Registering

Create an instance of WalletConnectProvider and initialize it with the required config, then register it with the Flow SDK. Here is an example:

```csharp
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.WalletConnect;
using DapperLabs.Flow.Sdk.Crypto;

IWallet walletProvider = ScriptableObject.CreateInstance<WalletConnectProvider>();
walletProvider.Init(new WalletConnectConfig 
{
    ProjectId = "xxxxxxxxxxxxxxxxxxxxx", // the Project ID from the previous step
    ProjectDescription = "An example project to showcase Wallet Connect", // a description for your project
    ProjectIconUrl = "https://walletconnect.com/meta/favicon.ico", // URL for an icon for your project
    ProjectName = "Dapper Unity Example", // the name of your project
    ProjectUrl = "https://dapperlabs.com" // URL for your project
});
FlowSDK.RegisterWalletProvider(walletProvider);
```

The description, icon, name and URL that you provide will appear in your user's wallet apps, when they connect the game to their wallet. 

### Authenticating

The `IWallet.Authenticate` method is as follows: 

```csharp
public void Authenticate(string username, System.Action<string> OnAuthSuccess, System.Action OnAuthFailed);
```

`username` is ignored in the Wallet Connect provider. 
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

If you are using the Flow SDK to sign transactions then you do not need to worry about this, as it is handled automatically. When you submit a transaction, the SDK will request Wallet Connect to sign the transaction as the authenticated user. The user will receive a notification in their wallet app to approve the transaction. 

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

When your game calls `Authenticate`, a dialog box will appear. The contents of the dialog box will depend on what platform the user is on. 

### Desktop builds (Windows, MacOS)

The authenticate dialog box will contain a QR code. The user must scan the QR code with their wallet app, after which their app will ask them to approve the connection with your game. Once they have approved the connection, they will receive any transaction requests submitted by the game in their wallet app. They can approve or reject these transaction requests. 

### Mobile builds (iOS, Android)

The authenticate dialog box will contain a list of wallet apps that support Flow and Wallet Connect. If an app is installed on the device, it will say `Installed` next to the name. Selecting an app will either open the app if it's installed, or direct the user to download the app from the App Store (iOS) or Play Store (Android). If the app was installed and is opened, the user can approve the connection with your game. Once they have approved the connection, they will receive any transaction requests submitted by the game in their wallet app. They can approve or reject these transaction requests. 

## Customising the Authentication Dialogs

You can customise the UI of both the QR Code (desktop builds) and Wallet Select (mobile builds) dialogs, allowing you to keep the same UI theme as the rest of your game. To do this, supply your custom prefabs to the Wallet Connect Config object during initialization, such as in the following example: 

```csharp
// Register WalletConnect
IWallet walletProvider = new WalletConnectProvider();
walletProvider.Init(new WalletConnectConfig
{
    ProjectId = "xxxxxxxxxxxxxxxxxxxxx", 
    ProjectDescription = "An example project to showcase Wallet Connect", 
    ProjectIconUrl = "https://walletconnect.com/meta/favicon.ico", 
    ProjectName = "Dapper Unity Example", 
    ProjectUrl = "https://dapperlabs.com" 
    QrCodeDialogPrefab = qrCodeCustomPrefab, // custom prefab for QR Code dialog (desktop builds)
    WalletSelectDialogPrefab = walletSelectCustomPrefab // custom prefab for Wallet Select dialog (mobile builds)
});
FlowSDK.RegisterWalletProvider(walletProvider);
```

> **Note**: \
Your custom QR Code prefab must have the `QRCodeDialog` script added as a component. \
Your custom Wallet Select prefab must have the `WalletSelectDialog` script added as a component. 
