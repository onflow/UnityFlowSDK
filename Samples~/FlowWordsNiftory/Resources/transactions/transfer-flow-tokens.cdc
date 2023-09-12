// Script: transfer-flow-tokens.cdc
// Purpose: Transaction to transfer flow tokens from the signer to the specified account
import FungibleToken from 0xee82856bf20e2aa6
import FlowToken from 0x0ae53cb6e3f42a79

transaction(recipient: Address, amount: UFix64)
{
  let temporaryVault: @FungibleToken.Vault
  

  prepare(acct: AuthAccount)
  {
    let vaultRef = acct.borrow<&FlowToken.Vault>(from: /storage/flowTokenVault)
        ?? panic("Could not borrow owner's vault reference")
    
    self.temporaryVault <- vaultRef.withdraw(amount: amount)
  }

  execute
  {
    let receiver = getAccount(recipient)

    let receiverRef = receiver.getCapability(/public/flowTokenReceiver)
                            .borrow<&FlowToken.Vault{FungibleToken.Receiver}>()
                            ?? panic("Could not borrow receiver's flow token receiver")

    receiverRef.deposit(from: <-self.temporaryVault)
  }
}