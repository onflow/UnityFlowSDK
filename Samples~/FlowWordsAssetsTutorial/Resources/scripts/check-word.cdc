// Script: check-word.cdc
// Purpose: Checks a word against the FlowWords valid guess word list, to ensure word is actually valid before committing to a guess transaction.

import CONTRACT_NAME from GAME_DEPLOY_ACCOUNT
  
pub fun main(word:String):String
{
    // Get FlowWords game ACCOUNT
    let FlowWordsAcct = getAccount(GAME_DEPLOY_ACCOUNT)

    // Get Flurde acceptable word list INTERFACE CAPABILITY from game ACCOUNT
    let wordListCapability = FlowWordsAcct.getCapability<&{CONTRACT_NAME.WordInterface}>(/public/WordList)

    // Attempt to BORROW access to the word list INTERFACE
    let wordListReference = wordListCapability.borrow()
        ?? panic("Could not borrow a reference to the word list")

    // Call CheckWord() on word list INTERFACE to check if word is valid
    let output = wordListReference.CheckWord(word)

    // return result
    return output
}
