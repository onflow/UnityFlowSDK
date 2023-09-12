// Script: get-player-guess-distribution.cdc
// Purpose: Gets the player guess distribution count across all completed games for the given account Address

import FlowWords from GAME_DEPLOY_ACCOUNT

pub fun main(account:Address) : [UInt; 6]
{
    // get highscore resource capability from FlowWords account
    let hsCap = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{FlowWords.HighScoreInterface}>(/public/HighScores)
    let hs = hsCap.borrow() ?? panic("Cannot borrow HighScore Capability")

    // call function on highscores resource and return result
    return hs.GetPlayerGuessDistribution(accId: account)
}
