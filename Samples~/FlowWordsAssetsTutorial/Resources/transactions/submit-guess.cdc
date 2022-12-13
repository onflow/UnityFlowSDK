// Script: submit-guess.cdc
// Purpose: Transaction to submit a guess for the signing account, for the current game

import CONTRACT_NAME from GAME_DEPLOY_ACCOUNT

transaction(guess: String)
{
  let userGameStateCap: Capability<&{CONTRACT_NAME.UserGameStateInterface}>

  prepare(acct: AuthAccount)
  {
    // get UserGameState capability for signing player
    self.userGameStateCap = acct.getCapability<&{CONTRACT_NAME.UserGameStateInterface}>(/private/FlowWordsGameState)
  }

  execute
  {
    // get game capability
    let gameCap = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{CONTRACT_NAME.GameInterface}>(/public/Game)
    let game = gameCap.borrow() ?? panic("Cannot get Game capability!")

    // submit guess using signer's UserGameState capability
    game.SubmitGuess(guess:guess, userStateCapability: self.userGameStateCap)
  }
}