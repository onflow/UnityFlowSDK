// Script: get-player-streak.cdc
// Purpose: Gets the current winning streak count for the give account address

import FlowWords from GAME_DEPLOY_ACCOUNT

pub fun main(account:Address) : UInt
{
    // get highscore resource capability from FlowWords account
    let hsCap = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{FlowWords.HighScoreInterface}>(/public/HighScores)
    let hs = hsCap.borrow() ?? panic("Cannot borrow HighScore Capability")

    // call function on highscores resource and return result
    return hs.GetPlayerWinningStreak(accId: account)
}
