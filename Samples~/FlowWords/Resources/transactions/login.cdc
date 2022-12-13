// Script: login.cdc
// Purpose: Creates the UserGameState resource in the user's account, if required. Updates the user's preferred name for the leaderboards, and ticks the main contract to ensure word of the day is up to date.

import FlowWords from GAME_DEPLOY_ACCOUNT

transaction(username: String) 
{
  let account: Address
  let privCap: Capability<&{FlowWords.UserGameStateInterface}>

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

    // update user's preferred name, for use on leaderboards
    if self.privCap.borrow()!.GetName() != username
    {
      let res <- acct.load<@FlowWords.UserGameState>(from: /storage/FlowWordsGameState) ?? panic("Resource does not exist")
      res.SetName(username: username)
      acct.save<@FlowWords.UserGameState>(<-res, to: /storage/FlowWordsGameState)
    }

    // force tick the main contract, to update word of the day
    let gameCapability = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{FlowWords.GameInterface}>(/public/Game)
    gameCapability.borrow()!.GetCurrentGameId()
  }

  post
  {
    // check that capabilities have been granted correctly.
    self.privCap.check(): "Private GameState Link invalid"
    getAccount(self.account).getCapability<&FlowWords.UserGameState{FlowWords.UserGameStatePublicInterface}>(/public/FlowWordsGameState).check(): "Public GameState Link invalid"
  }
}
 