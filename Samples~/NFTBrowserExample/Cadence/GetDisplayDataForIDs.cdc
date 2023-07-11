import NonFungibleToken from 0x1d7e57aa55817448
import MetadataViews from 0x1d7e57aa55817448

pub fun main(addr: Address, path: StoragePath, ids: [UInt64]) : {UInt64:AnyStruct?} {
    //Array to hold the NFT display data that we will return
    //We use AnyStruct? because that is the type that is returned by resolveView.
    var returnData: {UInt64:AnyStruct?} = {}

    //Get account for address
    var acct = getAuthAccount(addr)
    
    //Get a reference to a capability to the storage path as a NonFungibleToken.CollectionPublic
    var ref = acct.borrow<&{NonFungibleToken.CollectionPublic}>(from: path)!
    
    //Loop through the requested IDs
    for id in ids {       
        //Get a reference to the NFT we're interested in
        var nftRef = ref.borrowNFT(id: id)
        
        //If for some reason we couldn't borrow a reference, continue onto the next NFT
        if nftRef == nil {
            continue
        }

        //Fetch the information we're interested in and store it in our NFT structure
        returnData[id] = nftRef.resolveView(Type<MetadataViews.Display>())
    }
    
    return returnData
}