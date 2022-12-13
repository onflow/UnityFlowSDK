import FlowSDKSampleNFT from 0xf3fcd2c1a78f5eee
import FlowSDKSampleToken from 0xf3fcd2c1a78f5eee

// This transaction initialises a new account by creating an empty
// NFT collection and an empty Token Vault, and creating the 
// required capabilities for them. New accounts must sign and submit
// this transaction before they can use the FlowSDK Sample NFTs or Tokens. 

transaction {

    prepare(acct: AuthAccount) {
        // store an empty NFT Collection in account storage
        acct.save(<-FlowSDKSampleNFT.createEmptyCollection(), to: FlowSDKSampleNFT.CollectionStoragePath)

        // publish a reference to the Collection in storage
        acct.link<&{FlowSDKSampleNFT.NFTReceiver}>(FlowSDKSampleNFT.CollectionPublicPath, target: FlowSDKSampleNFT.CollectionStoragePath)

        // create a new empty vault instance
        let vaultA <- FlowSDKSampleToken.createEmptyVault()

        // Store the vault in the account storage
        acct.save<@FlowSDKSampleToken.Vault>(<-vaultA, to: FlowSDKSampleToken.VaultStoragePath)

        // Create a public Receiver capability to the Vault
        let ReceiverRef = acct.link<&FlowSDKSampleToken.Vault{FlowSDKSampleToken.Receiver, FlowSDKSampleToken.Balance}>(FlowSDKSampleToken.VaultPublicPath, target: FlowSDKSampleToken.VaultStoragePath)
    }
}
