import NonFungibleToken from 0x1d7e57aa55817448

pub fun main(addr: Address) : [StoragePath] {
    //Get the AuthAccount for the given address.
    //The AuthAccount is needed because we're going to be looking into the Storage of the user
    var acct = getAuthAccount(addr)
    
    //Array that we will fill with all valid storage paths
    var paths : [StoragePath] = []
    
    //Uses the storage iteration API to iterate through all storage paths on the account
    acct.forEachStored(fun (path: StoragePath, type:Type): Bool {
        //Check to see if the resource at this location is a subtype of NonFungibleToken.Collection.
        if type.isSubtype(of: Type<@NonFungibleToken.Collection>()) {
            //Add this path to the array
            paths.append(path)
        }
        
        //returning true tells the iterator to continue to the next entry
        return true
    });
    
    //Return the array that we built
    return paths
}