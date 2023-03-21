import NonFungibleToken from 0x1d7e57aa55817448
import MetadataViews from 0x1d7e57aa55817448

//Structure that will hold all the data we want for an NFT
pub struct NFTData {
    pub(set) var NFTView: AnyStruct?
    pub(set) var Display : AnyStruct?
    pub(set) var HTTPFile: AnyStruct?
    pub(set) var IPFSFile: AnyStruct?
    pub(set) var Edition: AnyStruct?
    pub(set) var Editions: AnyStruct?
    pub(set) var Serial: AnyStruct?
    pub(set) var Royalty: AnyStruct?
    pub(set) var Royalties: AnyStruct?
    pub(set) var Media: AnyStruct?
    pub(set) var Medias: AnyStruct?
    pub(set) var License: AnyStruct?
    pub(set) var ExternalURL: AnyStruct?
    pub(set) var NFTCollectionDisplay: AnyStruct?
    pub(set) var Rarity: AnyStruct?
    pub(set) var Trait: AnyStruct?
    pub(set) var Traits: AnyStruct?
    
    init() {
        self.NFTView = nil
        self.Display = nil
        self.HTTPFile = nil
        self.IPFSFile = nil
        self.Edition = nil
        self.Editions = nil
        self.Serial = nil
        self.Royalty = nil
        self.Royalties = nil
        self.Media = nil
        self.Medias = nil
        self.License = nil
        self.ExternalURL = nil
        self.NFTCollectionDisplay = nil
        self.Rarity = nil
        self.Trait = nil
        self.Traits = nil
    }
}

pub fun main(addr: Address, path: StoragePath, id: UInt64) : NFTData? {
    //Get account for address
    var acct = getAuthAccount(addr)
    
    //Get a reference to a capability to the storage path as a NonFungibleToken.CollectionPublic
    var ref = acct.borrow<&{NonFungibleToken.CollectionPublic}>(from: path)!
    
    //Get a reference to the NFT we're interested in
    var nftRef = ref.borrowNFT(id: id)
    
    //If for some reason we couldn't borrow a reference, continue onto the next NFT
    if nftRef == nil {
        return nil
    }

    var nftData : NFTData = NFTData() 

    //Fetch the information we're interested in and store it in our NFT structure
    nftData.Display = nftRef.resolveView(Type<MetadataViews.Display>())
    nftData.NFTView = nftRef.resolveView(Type<MetadataViews.NFTView>())
    nftData.HTTPFile = nftRef.resolveView(Type<MetadataViews.HTTPFile>())
    nftData.IPFSFile = nftRef.resolveView(Type<MetadataViews.IPFSFile>())
    nftData.Edition = nftRef.resolveView(Type<MetadataViews.Edition>())
    nftData.Editions = nftRef.resolveView(Type<MetadataViews.Editions>())
    nftData.Serial = nftRef.resolveView(Type<MetadataViews.Serial>())
    nftData.Media = nftRef.resolveView(Type<MetadataViews.Media>())
    nftData.Rarity = nftRef.resolveView(Type<MetadataViews.Rarity>())
    nftData.Trait = nftRef.resolveView(Type<MetadataViews.Trait>())
    nftData.Traits = nftRef.resolveView(Type<MetadataViews.Traits>())
    nftData.Medias = nftRef.resolveView(Type<MetadataViews.Medias>())
    nftData.ExternalURL = nftRef.resolveView(Type<MetadataViews.ExternalURL>())
    nftData.Royalty = nftRef.resolveView(Type<MetadataViews.Royalty>())
    nftData.Royalties = nftRef.resolveView(Type<MetadataViews.Royalties>())
    nftData.License = nftRef.resolveView(Type<MetadataViews.License>())
    nftData.NFTCollectionDisplay = nftRef.resolveView(Type<MetadataViews.NFTCollectionDisplay>())
    
    return nftData
}