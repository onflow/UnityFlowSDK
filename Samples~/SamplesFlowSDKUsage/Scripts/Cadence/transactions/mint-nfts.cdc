import FlowSDKSampleNFT from 0xf3fcd2c1a78f5eee

// This transaction mints two NFTs for acc1 and one NFT for acc2. 
// It must be signed by the account that owns the NFT Minter. 

transaction(acc1: Address, acc2: Address) {

    // The reference to the Minter resource stored in account storage
    let minterRef: &FlowSDKSampleNFT.NFTMinter

    prepare(acct: AuthAccount) {

        // Borrow a capability for the NFTMinter in storage
        self.minterRef = acct.borrow<&FlowSDKSampleNFT.NFTMinter>(from: FlowSDKSampleNFT.MinterStoragePath)
            ?? panic("could not borrow minter reference")
    }

    execute {

        // Get the recipient's public account object
        let recipient1 = getAccount(acc1)
        let recipient2 = getAccount(acc2)

        // Get the Collection reference for the receiver
        // getting the public capability and borrowing a reference from it
        let receiverRef1 = recipient1.getCapability(FlowSDKSampleNFT.CollectionPublicPath)
                                    .borrow<&{FlowSDKSampleNFT.NFTReceiver}>()
                                    ?? panic("Could not borrow nft receiver reference")

        let receiverRef2 = recipient2.getCapability(FlowSDKSampleNFT.CollectionPublicPath)
                                    .borrow<&{FlowSDKSampleNFT.NFTReceiver}>()
                                    ?? panic("Could not borrow nft receiver reference")

        // Use the minter reference to mint an NFT, which deposits
        // the NFT into the collection that is sent as a parameter.
        let newNFT <- self.minterRef.mintNFT()
        receiverRef1.deposit(token: <-newNFT)

        let newNFT2 <- self.minterRef.mintNFT()
        receiverRef2.deposit(token: <-newNFT2)

        let newNFT3 <- self.minterRef.mintNFT()
        receiverRef1.deposit(token: <-newNFT3)
    }
}
