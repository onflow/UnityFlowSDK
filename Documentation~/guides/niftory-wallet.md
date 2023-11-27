# Niftory Wallet

## Introduction

Niftory is a production ready wallet provider which implements the `IWallet` interface. It's not an actual wallet itself - it's a bridge that connects your Unity game with the Niftory ecosystem.
Through this IWallet interface, Niftory are able to provide a walletless onboarding solution for applications built on Flow in Unity.

For more information about Niftory, see [https://niftory.com](https://niftory.com). 

## What are Wallets? 

A wallet is a piece of software or hardware that stores the private key associated with a Flow account. The term *custodian* is used to refer to the party that stores the private key. In the case of Niftory, Niftory are the custodians, and as such hold the keys to your user's wallets. Niftory user wallets are accessed via a combination of your application's unique App/Client ID, and a user access token which is unique to each user. Hosted software wallets (such as Niftory wallets) act as the custodian on behalf of the user, whereas Hardware Wallets (eg Ledger), typically USB devices, allow users to be their own custodian.

For more information about Wallets and Flow accounts, see [https://developers.flow.com/flow/dapp-development/user-accounts-and-wallets](https://developers.flow.com/flow/dapp-development/user-accounts-and-wallets). 

## What is a Wallet Provider? 

In terms of the Flow SDK for Unity, a Wallet Provider is a class which implements the `IWallet` interface and allows users to interact with specific hardware or software wallets. This includes authenticating with a wallet, retrieving the user's Flow account address from the wallet, and requesting the wallet to sign transactions on behalf of the user. 

The Flow SDK for Unity contains multiple wallet providers - [Dev Wallet](https://developers.flow.com/tools/unity-sdk/guides/dev-wallet), Niftory, FCL, and Wallet Connect. Dev Wallet is a mock wallet provider to make development easier, while the other wallet interfaces connect to real wallets and are therefore used for production. You could also implement your own wallet provider by implementing the `IWallet` interface. 

## How to implement Niftory

To implement Niftory, you must first create a Niftory account, and register your project on Niftory's website to obtain a Client ID, you can then use this Client ID to register the Niftory provider with the Flow SDK in your Unity project. 

### Obtain Project ID

1. Go to [https://admin.niftory.com](https://admin.niftory.com) to sign in with your email address. This will prompt you to create an account if you don't have one. 
2. If this is your first sign in, you will be prompted to create your first app. Give it a name, and select FLOW as the blockchain. (If this is not your first sign in, click on your account menu in the bottom left corner of the screen, and select the 'Create new App' option.)
3. At the bottom of the 'Your App' page, click the 'Deploy Contract' button. This is required for your ClientID to begin creating wallets for your users. 
4. In the 'Your App Credentials' section, at the top right, you will see a plus (+) button. Click this button, and create a new 'Device' type credential.
4. Copy your new device credential Client ID for use in the next step. 

### Registering

Create an instance of WalletConnectProvider and initialize it with the required config, then register it with the Flow SDK. Here is an example:

```csharp
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Niftory;
using DapperLabs.Flow.Sdk.Crypto;

IWallet walletProvider = new NiftoryProvider();
walletProvider.Init(new NiftoryConfig
{
    ClientId = "<your client id>",  // enter your client id from the previous step
    AuthUrl = "https://auth.staging.niftory.com",   // staging is the testnet url, auth.niftory.com is for use on mainnet.
    GraphQLUrl = "https://graphql.api.staging.niftory.com"  // as above, graphql.api.niftory.com is for mainnet
});
FlowSDK.RegisterWalletProvider(walletProvider);
``` 

### Authenticating

The `IWallet.Authenticate` method is as follows: 

```csharp
public void Authenticate(string username, System.Action<string> OnAuthSuccess, System.Action OnAuthFailed);
```

`username` is ignored in the Niftory provider. 
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

Signing transactions locally is not yet supported for the Niftory provider. Instead, once you have authenticated the user, call `Transactions.Submit` or one of it's helper variants such as `Transactions.SubmitAndWaitUntilSealed` to submit a transaction to the blockchain.

Calling `SignTransactionEnvelope` or `SignTransactionPayload` on the NiftoryProvider will throw an exception. As a result, multiple party signing is not currently supported in Niftory provider.

The following is an example of how to call `SubmitAndWaitUntilSealed`:

```csharp
FlowTransactionResult result = await Transactions.SubmitAndWaitUntilSealed("transaction", new CadenceString("param1"), new CadenceString("param2"));

// handle errors - this must be checked.
if (result.Error != null || result.ErrorMessage != string.Empty || result.Status == FlowTransactionStatus.EXPIRED)
{
    Debug.LogError("Transaction Failed!")
    // ... your error handling here ...
}
else
{
    // success - process events
    List<FlowEvent> events = result.Events;
    // ... your code here ...
}
```

## What your users will see

When your game calls `Authenticate`, a dialog box will appear. The contents of the dialog box will depend on what platform the user is on. 

### Desktop builds (Windows, MacOS)

The authenticate dialog box will contain a QR code and a clickable link. The user must either scan the QR code with their mobile device, or click the hyperlink. This will navigate the user to a webpage, where they will be asked to sign in with an email address. The user will then be sent a verify link in their email inbox. \
Your users will need to complete the sign in, including clicking the verify link, on a single device (e.g. If the QR code is scanned with a phone, then the email link must be verified on that same phone). Once your user has verified their email, they will remain authenticated until `NiftoryProvider.Unauthenticate` is called, or one week has passed without your user signing in (calling `NiftoryProvider.Authenticate`). Once Authenticated, subsequent blockchain transactions will not require any interaction from your users.

### Mobile builds (iOS, Android)

The authenticate dialog box will contain a QR code and a clickable link. The user must either scan the QR code with another device, or click the hyperlink. This will navigate the user to a webpage, where they will be asked to sign in with an email address. The user will then be sent a verify link in their email inbox. If users are verifying the link on the same device that the game is being played on, the user will have to switch back to your game manually to continue. \
Your users will need to complete the sign in, including clicking the verify link, on a single device (e.g. If the QR code is scanned with a phone, then the email link must be verified on that same phone). Once your user has verified their email, they will remain authenticated until `NiftoryProvider.Unauthenticate` is called, or one week has passed without your user signing in (calling `NiftoryProvider.Authenticate`). Once Authenticated, subsequent blockchain transactions will not require any interaction from your users. 

## Customising the Authentication Dialogs

You can customise the UI of the QR Code dialog, allowing you to keep the same UI theme as the rest of your game. To do this, supply your custom prefabs to the Niftory Config object during initialization, such as in the following example: 

```csharp
IWallet walletProvider = new NiftoryProvider();
walletProvider.Init(new NiftoryConfig
{
    ClientId = "<your client id>",  // enter your client id from the previous step
    AuthUrl = "https://auth.staging.niftory.com",   // staging is the testnet url, auth.niftory.com is for use on mainnet.
    GraphQLUrl = "https://graphql.api.staging.niftory.com",  // as above, graphql.api.niftory.com is for mainnet
    QrCodeDialogPrefab = qrCodeCustomPrefab // custom prefab for QR Code dialog (desktop builds)
});
FlowSDK.RegisterWalletProvider(walletProvider);
```

> **Note**: \
Your custom QR Code prefab must have the `QRCodeDialog` script added as a component.
