import SDKExampleNFT from 0xf8d6e0586b0a20c7

pub fun main(addr:Address): {UInt64:{String:String}} {

    //Get a capability to the SDKExampleNFT collection if it exists.  Return an empty dictionary if it does not
    let collectionCap = getAccount(addr).getCapability<&{SDKExampleNFT.CollectionPublic}>(SDKExampleNFT.CollectionPublicPath)
    if(collectionCap == nil)
    {
        return {}
    }
    
    //Borrow a reference to the capability, returning an empty dictionary if it can not borrow
    let collection = collectionCap.borrow()
    if(collection == nil)
    {
        return {}
    }

    //Create a variable to store the information we extract from the NFTs
    var output : {UInt64:{String:String}} = {}
    
    //Iterate through the NFTs, extracting id and metadata from each.
    for id in collection?.getIDs()! {
        log(collection!.borrowSDKExampleNFT(id:id))
        log(collection!.borrowSDKExampleNFT(id:id)!.metadata)
        output[id] = collection!.borrowSDKExampleNFT(id:id)!.metadata;
    }
    
    //Return the constructed data
    return output
}