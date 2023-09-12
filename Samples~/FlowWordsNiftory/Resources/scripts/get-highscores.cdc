// Script: get-highscores.cdc
// Purpose: Gets the entire global high scores list

import FlowWords from GAME_DEPLOY_ACCOUNT

pub fun main() : [FlowWords.Scores]
{
    // get highscore resource capability from FlowWords account
    let hsCap = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{FlowWords.HighScoreInterface}>(/public/HighScores)
    let hs = hsCap.borrow() ?? panic("Cannot borrow HighScore Capability")

    // call function on highscores resource and return result
    return hs.GetHighScores()
}
