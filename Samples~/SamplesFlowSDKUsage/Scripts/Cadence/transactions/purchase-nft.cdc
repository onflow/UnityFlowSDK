import FlowSDKSampleToken from 0xf3fcd2c1a78f5eee
import FlowSDKSampleNFT from 0xf3fcd2c1a78f5eee
import FlowSDKSampleMarketplace from 0xf3fcd2c1a78f5eee

// This transaction uses the signer's Tokens to purchase an NFT
// from the Sale collection of the account specified by address.

transaction(address: Address, nftId: UInt64, nftPrice: UFix64) {

    // Capability to the buyer's NFT collection where they
    // will store the bought NFT
    let collectionCapability: Capability<&AnyResource{FlowSDKSampleNFT.NFTReceiver}>

    // Vault that will hold the Tokens that will be used to
    // buy the NFT
    let temporaryVault: @FlowSDKSampleToken.Vault

    prepare(acct: AuthAccount) {

        // get the references to the buyer's fungible token Vault and NFT Collection Receiver
        self.collectionCapability = acct.getCapability<&AnyResource{FlowSDKSampleNFT.NFTReceiver}>(FlowSDKSampleNFT.CollectionPublicPath)

        let vaultRef = acct.borrow<&FlowSDKSampleToken.Vault>(from: FlowSDKSampleToken.VaultStoragePath)
            ?? panic("Could not borrow owner's vault reference")

        // withdraw tokens from the buyers Vault
        self.temporaryVault <- vaultRef.withdraw(amount: nftPrice)
    }

    execute {
        // get the read-only account storage of the seller
        let seller = getAccount(address)

        // get the reference to the seller's sale
        let saleRef = seller.getCapability(/public/NFTSale)
                            .borrow<&AnyResource{FlowSDKSampleMarketplace.SalePublic}>()
                            ?? panic("Could not borrow seller's sale reference")

        // purchase the NFT the the seller is selling, giving them the capability
        // to your NFT collection and giving them the tokens to buy it
        saleRef.purchase(tokenID: nftId, recipient: self.collectionCapability, buyTokens: <-self.temporaryVault)
    }
}

