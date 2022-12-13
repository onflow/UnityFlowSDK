// Script: get-player-maxstreak.cdc
// Purpose: Gets the maximum winning streak of all time for the given account address

import CONTRACT_NAME from GAME_DEPLOY_ACCOUNT

pub fun main(account:Address) : UInt
{
    // get highscore resource capability from FlowWords account
    let hsCap = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{CONTRACT_NAME.HighScoreInterface}>(/public/HighScores)
    let hs = hsCap.borrow() ?? panic("Cannot borrow HighScore Capability")

    // call function on highscores resource and return result
    return hs.GetPlayerMaxWinningStreak(accId: account)
}
