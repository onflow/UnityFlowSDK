import FlowSDKSampleNFT from 0xf3fcd2c1a78f5eee

// Print the NFTs owned by account.
pub fun main(account: Address): [UInt64] {
    // Get the public account object
    let nftOwner = getAccount(account)

    // Find the public Receiver capability for their Collection
    let capability = nftOwner.getCapability<&{FlowSDKSampleNFT.NFTReceiver}>(FlowSDKSampleNFT.CollectionPublicPath)

    // borrow a reference from the capability
    let receiverRef = capability.borrow()
        ?? panic("Could not borrow the receiver reference")

    return receiverRef.getIDs()
}
