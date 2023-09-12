//Import the NonFungibleToken contract from a known location.  This defines the interface we will implement
import NonFungibleToken from GAME_DEPLOY_ACCOUNT

//Our unique NFT that implements the NonFungibleToken interface
pub contract FlowWordsNFT: NonFungibleToken {

    //Keeps track of how many of this NFT have been minted
    pub var totalSupply: UInt64

    //Some events we can emit when performing various actions
    pub event ContractInitialized()
    pub event Withdraw(id: UInt64, from: Address?)
    pub event Deposit(id: UInt64, to: Address?)

    //Defines the well known storage paths for the collection and minter
    pub let CollectionStoragePath: StoragePath
    pub let CollectionPublicPath: PublicPath
    pub let MinterStoragePath: StoragePath
    pub let MinterPublicPath : PublicPath

    //Our NFT that implements the INFT interface
    pub resource NFT: NonFungibleToken.INFT {
        //The unique ID of this NFT
        pub let id: UInt64
        
        //A dictionary that can hold arbitrary string data for our NFT
        pub let metadata: {String:String}

        //Creates an NFT instance
        init(
            id: UInt64,
            metadata: {String:String},
        ) {
            self.id = id
            self.metadata = metadata
        }
    }

    //Our own interface for our collection.  It is mostly the same as the NonFungibleToken.CollectionPublic
    //interface, with the addition of a borrowFlowWordsNFT function that will return a reference to our specific NFT
    //type instead of a generic NonFungibleToken.NFT.
    pub resource interface CollectionPublic {
        pub fun deposit(token: @NonFungibleToken.NFT)
        pub fun getIDs(): [UInt64]
        pub fun borrowNFT(id: UInt64): &NonFungibleToken.NFT
        pub fun borrowFlowWordsNFT(id: UInt64): &FlowWordsNFT.NFT? 
    }

    //The collection that stores the NFTs
    pub resource Collection: CollectionPublic, NonFungibleToken.Provider, NonFungibleToken.Receiver, NonFungibleToken.CollectionPublic {
        // dictionary of FlowWordsNFT resources
        // NFT is a resource type with an `UInt64` ID field
        pub var ownedNFTs: @{UInt64: NonFungibleToken.NFT}

        init () {
            self.ownedNFTs <- {}
        }
        
        // withdraw removes an NFT from the collection and moves it to the caller
        pub fun withdraw(withdrawID: UInt64): @NonFungibleToken.NFT {
            let token <- self.ownedNFTs.remove(key: withdrawID) ?? panic("missing NFT")

            emit Withdraw(id: token.id, from: self.owner?.address)

            return <-token
        }

        // deposit takes a NFT and adds it to the collections dictionary
        // and adds the ID to the id array
        pub fun deposit(token: @NonFungibleToken.NFT) {
            let token <- token as! @FlowWordsNFT.NFT

            let id: UInt64 = token.id

            // add the new token to the dictionary which removes the old one
            let oldToken <- self.ownedNFTs[id] <- token

            emit Deposit(id: id, to: self.owner?.address)

            destroy oldToken
        }

        // getIDs returns an array of the IDs that are in the collection
        pub fun getIDs(): [UInt64] {
            return self.ownedNFTs.keys
        }

        // borrowNFT gets a reference to an NFT in the collection
        // so that the caller can read its metadata and call its methods
        pub fun borrowNFT(id: UInt64): &NonFungibleToken.NFT {
            return (&self.ownedNFTs[id] as &NonFungibleToken.NFT?)!
        }
 
        //A version of borrowNFT 
        pub fun borrowNFTSafe(id: UInt64): &NFT? {
            post {
                result == nil || result!.id == id: "The returned reference's ID does not match the requested ID"
            }
            return nil
        }

        // Gets a reference to the NFT in the collection as this specific NFT type
        pub fun borrowFlowWordsNFT(id: UInt64): &FlowWordsNFT.NFT? {
            if self.ownedNFTs[id] != nil {
                // Create an authorized reference to allow downcasting
                let ref = (&self.ownedNFTs[id] as auth &NonFungibleToken.NFT?)!
                return ref as! &FlowWordsNFT.NFT
            }

            return nil
        }

        destroy() {
            destroy self.ownedNFTs
        }
    }

    // public function that anyone can call to create a new empty collection
    pub fun createEmptyCollection(): @FlowWordsNFT.Collection {
        return <- create Collection()
    }

    pub resource interface PublicMinter {
        pub fun mintNFT(recipient: &{NonFungibleToken.CollectionPublic}, metadata: {String:String}) 
    }

    // Resource that an admin or something similar would own to be
    // able to mint new NFTs
    pub resource NFTMinter: PublicMinter {

        // mintNFT mints a new NFT with a new ID
        // and deposit it in the recipients collection using their collection reference
        pub fun mintNFT(
            recipient: &{NonFungibleToken.CollectionPublic},
            metadata: {String:String}
        ) 
        {
            // create a new NFT
            var newNFT <- create FlowWordsNFT.NFT(
                id: FlowWordsNFT.totalSupply,
                metadata: metadata,
            )

            // deposit it in the recipient's account using their reference
            recipient.deposit(token: <-newNFT)

            FlowWordsNFT.totalSupply = FlowWordsNFT.totalSupply + UInt64(1)
        }
    }

    init() {
        // Initialize the total supply
        self.totalSupply = 0

        // Set the named paths
        self.CollectionStoragePath = /storage/FlowWordsNFTCollection
        self.CollectionPublicPath = /public/FlowWordsNFTCollection
        self.MinterStoragePath = /storage/FlowWordsNFTMinter
        self.MinterPublicPath = /public/FlowWordsNFTMinter

        // Create a Minter resource and save it to storage
        let minter <- create NFTMinter()
        self.account.save(<-minter, to: self.MinterStoragePath)

        // Link the minter capability to a public path
        self.account.link<&{PublicMinter}>(self.MinterPublicPath, target: self.MinterStoragePath)

        emit ContractInitialized()
    }
}