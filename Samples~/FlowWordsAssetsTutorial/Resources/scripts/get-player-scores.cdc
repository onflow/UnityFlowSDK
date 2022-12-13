// Script: get-player-scores.cdc
// Purpose: Gets every score for every game the given account address has completed

import CONTRACT_NAME from GAME_DEPLOY_ACCOUNT

pub fun main(account:Address) : {UInt32 : UInt}
{
    // get highscore resource capability from FlowWords account
    let hsCap = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{CONTRACT_NAME.HighScoreInterface}>(/public/HighScores)
    let hs = hsCap.borrow() ?? panic("Cannot borrow HighScore Capability")

    // call function on highscores resource and return result
    return hs.GetPlayerScores(accId: account)
}
