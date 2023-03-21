import NonFungibleToken from 0x1d7e57aa55817448

pub fun main(addr: Address, path: StoragePath) : [UInt64] {
    //Get the AuthAccount for the given address.
    //The AuthAccount is needed because we're going to be looking into the Storage of the user
    var acct = getAuthAccount(addr)
    
    //Get a reference to an interface of type NonFungibleToken.Collection public backed by the resource located at path
    var ref = acct.borrow<&{NonFungibleToken.CollectionPublic}>(from: path)!
    
    //Return the list of NFT IDs contained in this collection
    return ref!.getIDs()
}