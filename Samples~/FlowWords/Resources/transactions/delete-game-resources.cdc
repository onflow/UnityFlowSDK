// Script: delete-game-resources.cdc
// Purpose: This is a transaction for administration purposes, which removes all game related resources, and associated links, from the game account.

import FlowWords from GAME_DEPLOY_ACCOUNT

transaction()
{
  let account : AuthAccount

  prepare(acct: AuthAccount)
  {
      self.account = acct

      // get game resources
      let wordList <- acct.load<@FlowWords.WordList>(from: /storage/WordList)
      let highScores <- acct.load<@FlowWords.HighScores>(from: /storage/HighScores)
      let game <- acct.load<@FlowWords.Game>(from: /storage/Game)

      // destroy game resources
      destroy wordList
      destroy highScores
      destroy game

      // remove capabilities
      acct.unlink(/private/HighScoresAdmin)
      acct.unlink(/public/HighScores)
      acct.unlink(/public/WordList)
      acct.unlink(/public/Game)
  }

  post
  {
    // check resources / capabilities are removed.
    self.account.borrow<&FlowWords.WordList>(from: /storage/WordList) == nil : "WordInterface lives."
    self.account.borrow<&FlowWords.HighScores>(from: /storage/HighScores) == nil : "HighScores lives."
    self.account.borrow<&FlowWords.Game>(from: /storage/Game) == nil : "Game lives."
  }

}