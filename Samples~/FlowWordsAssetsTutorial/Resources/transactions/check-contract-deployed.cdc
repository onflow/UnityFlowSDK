// Script: check-contract-deployed.cdc
// Purpose: Tries to borrow a capability to the game contract and logs the result to the console.

import CONTRACT_NAME from GAME_DEPLOY_ACCOUNT

transaction() 
{
  prepare(acct: AuthAccount)
  {
    // attempt to borrow capability to the game contract.
    let gameCapability = getAccount(GAME_DEPLOY_ACCOUNT).getCapability<&{CONTRACT_NAME.GameInterface}>(/public/Game)
    gameCapability.borrow() ?? panic("Game Contract not Found")

    log("Game Contract Found!")
  }
}
 