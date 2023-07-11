## Version 2.0.0

- Added new wallet provider Wallet Connect
- Added iOS as a supported platform
- Added Cadence Convert feature for simple de/serialization
- Changed how wallet providers are instantiated (see Breaking Changes)

### Breaking Changes in 2.0.0

Previously, wallet providers (ie DevWallet) were instantiated as follows: 

```csharp
FlowSDK.RegisterWalletProvider(ScriptableObject.CreateInstance<DevWalletProvider>());
```

From 2.0.0, this should now be: 

```csharp
FlowSDK.RegisterWalletProvider(new DevWalletProvider());
```

## Version 1.0.3

- Add Example NFT sample

## Version 1.0.2

- Fixed an issue where the latest Flow emulator would cause the editor to freeze

## Version 1.0.1

- Added wallet authentication to contract and account creation tools
- Fixed all Unity warnings in all samples
- Removed unused and commented code
- Added README files to all samples
- Emulator listens on all local IP addresses
- Improved error handling from all API requests
- Improved error handling and feedback from Flow Control tools
- Limited Events.GetForBlockIds request to 50 block ids
- Fixed login panel on Flow Words Tutorial sample

## Version 1.0.0 

- Initial release of the Flow SDK for Unity