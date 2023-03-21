# Simple NFT Viewer

This example project will show you how to build a simple viewer that will allow you to view NFTs that conform
to the [NFT](https://github.com/onflow/flow-nft) and [MetadataViews](https://github.com/onflow/flow-nft#nft-metadata) standards.

This tutorial will mostly ignore the C# code that actually displays the NFTs and focus on a high level summary of the steps used.

## Overview

When querying the blockchain we utilize four scripts:

* [GetCollections.cdc](Cadence/GetCollections.cdc) - Gets a list of Collections that conform to NFT.Collection for a given address
* [GetNftIdsForCollection.cdc](Cadence/GetNftIdsForCollection.cdc) - Gets a list of all NFT IDs that are contained in a given collection
* [GetDisplayDataForIDs.cdc](Cadence/GetDisplayDataForIDs.cdc) - Gets just the display data for a given NFT
* [GetFullDataForID.cdc](Cadence/GetFullDataForID.cdc) - Gets a more comprehensive set of data for a single NFT.

While we could use a single script to query for all the data, larger collections will cause the script to time out.  Instead
we query for just the data we need to reduce the chances of a timeout occurring.

## Finding Collections

First we need to get a list of all collections on an account that are a subtype of NFT.Collection.

```cadence
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
```

We use the [Storage Iteration API](https://developers.flow.com/cadence/language/accounts#storage-iteration) to look at 
everything the account has in it's storage and see if it is an NFT Collection.  We return a list of all found NFT Collections.

## Getting NFT IDs Contained in a Collection

We use this to create a list of collection paths a user can pick from.  When the user selects a path to view, we fetch a
list of IDs contained in that collection:

```cadence
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
```

## Getting Display Data for an NFT

After we get a list of the available NFT IDs, we need to get some basic data about the NFT to display the thumbnail icon.

```cadence
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
```
This gives us a dictionary that maps NFT IDs to Display structs (```{UInt64:MetadataViews.Display}```).  Because accessing this
information can be tedious in C#, we can define some C# classes to make our lives easier:

```csharp
public class File
{
    public string url;
    public string cid;
    public string path;
}

public class Display
{
    public String name;
    public String description;
    public File thumbnail;
}
```

This will allow us to use Cadence.Convert to convert from the CadenceBase that the script returns into a Display class.

This line in NFTViewer.cs is an example of converting using Cadence.Convert:

```csharp
Dictionary<UInt64, Display> displayData = Convert.FromCadence<Dictionary<UInt64, Display>>(scriptResponseTask.Result.Value);
```

You might ask whey we don't combine GetNftIdsForCollection.cdc and GetDisplayDataForIDs.cdc to get the Display data at the
same time we get the list of IDs.  This approach would work in many cases, but when an account contains large numbers of NFTs,
this could cause a script timeout.  Getting the list of IDs is a cheap call because the NFT contains this list in an array already.
By getting just the NFT IDs, we could implement paging and use multiple script calls to each fetch a portion of the display data.
This example doesn't currently do this type of paging, but could do so without modifying the cadence scripts.

## Getting Complete NFT Data

When a user selects a particular NFT to view in more detail, we need to fetch that detail.

```cadence
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
```
Here we define a struct NFTData that will contain all the different information we want and fill the struct via multiple
resolveView calls.

## C# Classes for Easy Converting

The end of NFTViewer.cs contains classes that we use to more easily convert from Cadence into C#.  One thing to note is that
the Cadence structs contain Optionals, like:

```var IPFSFile: AnyStruct?```

while the C# versions do not, such as 

```public IPFSFile IPFSFile;```

This is because we are declaring them as Classes, not Structs.  Classes in C# are reference types, which can automatically be
null.  We could have used Structs, in which case we'd have to use:

```public IPFSFile? IPFSFile```

This would wrap the IPFSFile struct in a Nullable, which would allow it to be null if the Cadence value was nil.

Another thing to note is the declaration of the C# File class:

```csharp
public class File
{
    public string url;
    public string cid;
    public string path;

    public string GetURL()
    {
        if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(cid))
        {
            return $"https://ipfs.io/ipfs/{cid}"; 
        }

        return url;
    }
}
```

Compare this to the File interface in the MetadataViews contract:

```cadence
    pub struct interface File {
        pub fun uri(): String
    }
```

The MetadataViews.File interface doesn't actually contain any fields, only a single method.  Because only two things in MetadataViews implement the
File interface (HTTPFile and IPFSFile), we chose to combine the possible fields into our File class.

```cadence
pub struct HTTPFile: File {
        pub let url: String
}

pub struct IPFSFile: File {
    pub let cid: String
    pub let path: String?
}
```

This allows Cadence.Convert to convert either an HTTPFile or an IPFSFile into a File object.  We can then check which fields
are populated to determine which it was initially.

This works fine for this simple viewer, but a more robust approach might be to create a ResolvedFile struct in the cadence script which
has a single uri field and populates it by calling the uri() function on whatever File type was retrieved.
