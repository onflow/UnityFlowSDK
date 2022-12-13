import FlowSDKSampleMarketplace from 0xf3fcd2c1a78f5eee

// Define a struct containing the data we want to return
pub struct NFTSale {
    pub var id: UInt64
    pub var price: UFix64
    pub var address: Address

    init(id: UInt64, price: UFix64, address: Address) {
        self.id = id
        self.price = price
        self.address = address
    }
}

// Get a list of all NFTs for sale
pub fun main(): [NFTSale] {
    // return value is an array of the struct defined above
    var ret: [NFTSale] = []

    // get an array of Sale Collection capabilities from the marketplace
    let sales = FlowSDKSampleMarketplace.getTokensForSale()

    // for each Sale Collection Capability
    for saleCapability in sales {
        // borrow a reference from the capability
        let saleRef = saleCapability.borrow()!

        // get a list of NFT Ids from the Sale Collection
        let tokenIDs = saleRef.getIDs()

        // for each NFT Id
        for nftId in tokenIDs {
            // add the id, price and owner's address to our return value
            ret.append(NFTSale(
                // we already have the id
                id: nftId,
                // get the price from the Sale Collection
                price: saleRef.idPrice(tokenID: nftId)!,
                // get the owner's address from the Sale Collection
                address: saleRef.owner!.address
            ))
        }
    }

    return ret
}