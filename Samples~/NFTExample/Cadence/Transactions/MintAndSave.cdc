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