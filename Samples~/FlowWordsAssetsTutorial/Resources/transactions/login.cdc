// Script: login.cdc
// Purpose: Creates the UserGameState resource in the user's account, if required. Updates the user's preferred name for the leaderboards, ticks the main contract to ensure word of the day is up to date, and returns the users gamestate

import CONTRACT_NAME from GAME_DEPLOY_ACCOUNT

transaction(username: String) 
{
  let account: Address
  let privCap: Capability<&{CONTRACT_NAME.UserGameStateInterface}>

  prepare(acct: AuthAccount)
  {
    // get account address for use in post transaction validation
    self.account = acct.address
    
    // check if user game state resource already exists. if not, create one.
    if acct.borrow<&CONTRACT_NAME.UserGameState>(from: /storage/FlowWordsGameState) == nil
    {
      // create resource
      acct.save<@CONTRACT_NAME.UserGameState>(<-CONTRACT_NAME.CreateGameState(name: username), to: /storage/FlowWordsGameState)
      acct.link<&CONTRACT_NAME.UserGameState{CONTRACT_NAME.UserGameStateInterface}>(/private/FlowWordsGameState, target: /storage/FlowWordsGameState)
      acct.link<&CONTRACT_NAME.UserGameState{CONTRACT_NAME.UserGameStatePublicInterface}>(/public/FlowWordsGameState, target: /storage/FlowWordsGameState)      
    }

    // get private capability to user game state for use in post transaction validation
    self.privCap = acct.getCapability<&CONTRACT_NAME.UserGameState{CONTRACT_NAME.UserGameStateInterface}>(/private/FlowWordsGameState)
    if(self.privCap.check() == false)
    { 
      panic("Private GameState Link invalid")
    }
    
    // update user's preferred name, for use on leaderboards
    if self.privCap.borrow()!.GetName() != username
    {
      let res <- acct.load<@CONTRACT_NAME.UserGameState>(from: /storage/FlowWordsGameState) ?? panic("Resource does not exist")
      res.SetName(username: username)
      acct.save<@CONTRACT_NAME.UserGameState>(<-res, to: /storage/FlowWordsGameState)
    }
  }

  execute
  {
    // call GetUserGameState on game contract
    let gameCapability = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{CONTRACT_NAME.GameInterface}>(/public/Game)
    let game = gameCapability.borrow() ?? panic("Cannot get Game capability!")

    // get game id and game state
    game.GetCurrentGameId()
    log(game.GetUserGameState(userStateCapability: self.privCap))
  }

  post
  {
    // check that capabilities have been granted correctly.
    getAccount(self.account).getCapability<&CONTRACT_NAME.UserGameState{CONTRACT_NAME.UserGameStatePublicInterface}>(/public/FlowWordsGameState).check(): "Public GameState Link invalid"
  }
}
 