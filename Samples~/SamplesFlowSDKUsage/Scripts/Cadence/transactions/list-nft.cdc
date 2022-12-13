import FlowSDKSampleToken from 0xf3fcd2c1a78f5eee
import FlowSDKSampleNFT from 0xf3fcd2c1a78f5eee
import FlowSDKSampleMarketplace from 0xf3fcd2c1a78f5eee

// This transaction creates a new Sale Collection object,
// lists an NFT for sale, puts it in account storage,
// and creates a public capability to the sale so that others can buy the token.
// It must be signed by the account listing the NFT for sale. 

transaction(nftId: UInt64, salePrice: UFix64) {

    prepare(acct: AuthAccount) {

        // Borrow a reference to the stored Vault
        let receiver = acct.getCapability<&FlowSDKSampleToken.Vault{FlowSDKSampleToken.Receiver}>(FlowSDKSampleToken.VaultPublicPath)

        // borrow a reference to the nftTutorialCollection in storage
        var collectionCapability = acct.getCapability<&FlowSDKSampleNFT.Collection>(/private/FlowSDKSampleNFTCollection)
        if (collectionCapability.check() == false) {
          collectionCapability = acct.link<&FlowSDKSampleNFT.Collection>(/private/FlowSDKSampleNFTCollection, target: FlowSDKSampleNFT.CollectionStoragePath)
          ?? panic("Unable to create private link to NFT Collection")
        }

        var sale <- acct.load<@FlowSDKSampleMarketplace.SaleCollection>(from: /storage/NFTSale)
        if (sale == nil) {
          // Create a new Sale object,
          // initializing it with the reference to the owner's vault
          sale <-! FlowSDKSampleMarketplace.createSaleCollection(ownerCollection: collectionCapability, ownerVault: receiver)
        }

        // List the token for sale by moving it into the sale object
        sale?.listForSale(tokenID: nftId, price: salePrice)

        // Store the sale object in the account storage
        acct.save(<-sale!, to: /storage/NFTSale)

        // Create a public capability to the sale so that others can call its methods
        var capability = acct.getCapability<&FlowSDKSampleMarketplace.SaleCollection{FlowSDKSampleMarketplace.SalePublic}>(/public/NFTSale)
        if (capability.check() == false) {
          capability = acct.link<&FlowSDKSampleMarketplace.SaleCollection{FlowSDKSampleMarketplace.SalePublic}>(/public/NFTSale, target: /storage/NFTSale)
          ?? panic("Unable to create public link to Sale Collection")

          let ref1 = capability.borrow()
          ?? panic("error ref1")
        }

        let ref2 = capability.borrow()
          ?? panic("error ref2")

        // Let the marketplace know about the sale so users can get a list of all sales
        FlowSDKSampleMarketplace.listSaleCollection(collection: capability)
    }
}
