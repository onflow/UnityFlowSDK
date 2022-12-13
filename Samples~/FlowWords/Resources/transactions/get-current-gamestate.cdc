// Script: get-current-gamestate.cdc
// Purpose: Retrieves the current gamestate for the user.

import FlowWords from GAME_DEPLOY_ACCOUNT

transaction()
{
  let userGameStateCap: Capability<&{FlowWords.UserGameStateInterface}>

  prepare(acct: AuthAccount)
  {
    // get capability to own private gamestate
    self.userGameStateCap = acct.getCapability<&{FlowWords.UserGameStateInterface}>(/private/FlowWordsGameState)
  }

  execute
  {
    // get capability from game contract
    let gameCap = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{FlowWords.GameInterface}>(/public/Game)
    let game = gameCap.borrow() ?? panic("Cannot get Game capability!")

    // call GetUserGameState on game contract
    log(game.GetUserGameState(userStateCapability: self.userGameStateCap))
  }
}