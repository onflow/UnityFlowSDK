// Script: login.cdc
// Purpose: Creates the UserGameState resource in the user's account, if required. Updates the user's preferred name for the leaderboards, ticks the main contract to ensure word of the day is up to date, and returns the users gamestate

import CapabilityFilter from HYBRID_CUSTODY_ACCOUNT
import CapabilityFactory from HYBRID_CUSTODY_ACCOUNT
import NFTCollectionPublicFactory from HYBRID_CUSTODY_ACCOUNT
import NFTProviderAndCollectionFactory from HYBRID_CUSTODY_ACCOUNT
import NFTProviderFactory from HYBRID_CUSTODY_ACCOUNT
import FlowWords from GAME_DEPLOY_ACCOUNT
import FlowWordsNFT from GAME_DEPLOY_ACCOUNT
import NonFungibleToken from GAME_DEPLOY_ACCOUNT

access(all) fun withoutPrefix(_ input: Address): String{
    var address = input.toString()

    //get rid of 0x
    if address.length > 1 && address.utf8[1] == 120 {
        address = address.slice(from: 2, upTo: address.length)
    }

    //ensure even length
    if address.length % 2 == 1{
        address="0".concat(address)
    }
    return address
}

transaction(username: String) 
{
  let account: Address
  let privCap: Capability<&{FlowWords.UserGameStateInterface}>
  let gameCapability: Capability<&{FlowWords.GameInterface}>
  let nftCollectionCapability: Capability<&{NonFungibleToken.CollectionPublic}>

  prepare(acct: AuthAccount)
  {
    // get account address for use in post transaction validation
    self.account = acct.address
    
    // check if user game state resource already exists. if not, create one.
    if acct.borrow<&FlowWords.UserGameState>(from: /storage/FlowWordsGameState) == nil
    {
      // create resource
      acct.save<@FlowWords.UserGameState>(<-FlowWords.CreateGameState(name: username), to: /storage/FlowWordsGameState)
      acct.link<&FlowWords.UserGameState{FlowWords.UserGameStateInterface}>(/private/FlowWordsGameState, target: /storage/FlowWordsGameState)
      acct.link<&FlowWords.UserGameState{FlowWords.UserGameStatePublicInterface}>(/public/FlowWordsGameState, target: /storage/FlowWordsGameState)      
    }

    // get private capability to user game state for use in post transaction validation
    self.privCap = acct.getCapability<&FlowWords.UserGameState{FlowWords.UserGameStateInterface}>(/private/FlowWordsGameState)
    if(self.privCap.check() == false)
    { 
      panic("Private GameState Link invalid")
    }
    
    // update user's preferred name, for use on leaderboards
    if self.privCap.borrow()!.GetName() != username
    {
      let res <- acct.load<@FlowWords.UserGameState>(from: /storage/FlowWordsGameState) ?? panic("Resource does not exist")
      res.SetName(username: username)
      acct.save<@FlowWords.UserGameState>(<-res, to: /storage/FlowWordsGameState)
    }

    // force tick the main contract, to update word of the day
    self.gameCapability = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{FlowWords.GameInterface}>(/public/Game)

    // Create NFT collection if it doesn't exist
    if acct.borrow<&FlowWordsNFT.Collection>(from: FlowWordsNFT.CollectionStoragePath) == nil
    {
      // Create a new empty collection
      let collection <- FlowWordsNFT.createEmptyCollection()
      // save it to the account
      acct.save(<-collection, to: FlowWordsNFT.CollectionStoragePath)
      // link a public capability for the collection
      acct.link<&{FlowWordsNFT.CollectionPublic, NonFungibleToken.CollectionPublic}>(
          FlowWordsNFT.CollectionPublicPath,
          target: FlowWordsNFT.CollectionStoragePath
      )
    }

    self.nftCollectionCapability = acct.getCapability<&{NonFungibleToken.CollectionPublic}>(FlowWordsNFT.CollectionPublicPath)

    // Setup the AllowlistFilter
    if acct.borrow<&CapabilityFilter.AllowlistFilter>(from: CapabilityFilter.StoragePath) == nil {
      acct.save(<-CapabilityFilter.create(Type<@CapabilityFilter.AllowlistFilter>()), to: CapabilityFilter.StoragePath)

      // Ensure the AllowlistFilter is linked to the expected PublicPath
      acct.unlink(CapabilityFilter.PublicPath)
      acct.link<&CapabilityFilter.AllowlistFilter{CapabilityFilter.Filter}>(CapabilityFilter.PublicPath, target: CapabilityFilter.StoragePath)

      // Get a reference to the filter
      let filter = acct.borrow<&CapabilityFilter.AllowlistFilter>(from: CapabilityFilter.StoragePath)
          ?? panic("filter does not exist")

      // Add the given type identifiers to the AllowlistFilter
      // **Note:** the whole transaction fails if any of the given identifiers are malformed
      let identifier = "A.".concat(withoutPrefix(GAME_DEPLOY_ACCOUNT)).concat(".FlowWordsNFT")
      let c = CompositeType(identifier) ?? panic("Couldn't construct CompositeType for identifier: ".concat(identifier))
      filter.addType(c)
    }

    // Check for a stored Manager, saving if not found
    if acct.borrow<&AnyResource>(from: CapabilityFactory.StoragePath) == nil {
      let f <- CapabilityFactory.createFactoryManager()
      acct.save(<-f, to: CapabilityFactory.StoragePath)

      // Check for Capabilities where expected, linking if not found
      if !acct.getCapability<&CapabilityFactory.Manager{CapabilityFactory.Getter}>(CapabilityFactory.PrivatePath).check() {
        acct.unlink(CapabilityFactory.PublicPath)
        acct.link<&CapabilityFactory.Manager{CapabilityFactory.Getter}>(CapabilityFactory.PublicPath, target: CapabilityFactory.StoragePath)
      }

      assert(
        acct.getCapability<&CapabilityFactory.Manager{CapabilityFactory.Getter}>(CapabilityFactory.PublicPath).check(),
        message: "CapabilityFactory is not setup properly"
      )

      let manager = acct.borrow<&CapabilityFactory.Manager>(from: CapabilityFactory.StoragePath)
        ?? panic("manager not found")

      /// Add generic NFT-related Factory implementations to enable castable Capabilities from this Manager
      manager.addFactory(Type<&{NonFungibleToken.CollectionPublic}>(), NFTCollectionPublicFactory.Factory())
      manager.addFactory(Type<&{NonFungibleToken.Provider, NonFungibleToken.CollectionPublic}>(), NFTProviderAndCollectionFactory.Factory())
      manager.addFactory(Type<&{NonFungibleToken.Provider}>(), NFTProviderFactory.Factory())
    }
  }

  execute
  {
    // mint an NFT for the user if they don't have one
    let collection = self.nftCollectionCapability.borrow() ?? panic("Cannot get nft collection")
    if collection.getIDs().length == 0 {
      // Get a reference to the minter
      let minter = getAccount(GAME_DEPLOY_ACCOUNT)
          .getCapability(FlowWordsNFT.MinterPublicPath)
          .borrow<&{FlowWordsNFT.PublicMinter}>() ?? panic("Cannot get minter")

      // Mint a new NFT and deposit into the authorizers account
      minter.mintNFT(recipient: collection!, metadata: {"example" : "https://www.freepik.com/photos/example"})
    }

    // call GetUserGameState on game contract
    let game = self.gameCapability.borrow() ?? panic("Cannot get Game capability!")

    // get game id and game state
    game.GetCurrentGameId()
    log(game.GetUserGameState(userStateCapability: self.privCap))
  }

  post
  {
    // check that capabilities have been granted correctly.
    getAccount(self.account).getCapability<&FlowWords.UserGameState{FlowWords.UserGameStatePublicInterface}>(/public/FlowWordsGameState).check(): "Public GameState Link invalid"
  }
}
 