// Script: get-player-cumulativescore.cdc
// Purpose: Gets the cumulative highscore for the account address provided

import CONTRACT_NAME from GAME_DEPLOY_ACCOUNT

pub fun main(account:Address) : UInt
{
    // get highscore resource capability from FlowWords account
    let hsCap = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{CONTRACT_NAME.HighScoreInterface}>(/public/HighScores)
    let hs = hsCap.borrow() ?? panic("Cannot borrow HighScore Capability")

    // call function on highscores resource and return result
    return hs.GetPlayerCumulativeScore(accId: account)
}
