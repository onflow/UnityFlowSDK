import FlowSDKSampleToken from 0xf3fcd2c1a78f5eee

// This transaction mints 25 Tokens for acc1 and 50 Tokens for acc2. 
// It must be signed by the account that owns the Token Minter. 

transaction(acc1: Address, acc2: Address) {

  // Public Vault Receiver References for both accounts
  let acct1Capability: Capability<&AnyResource{FlowSDKSampleToken.Receiver}>
  let acct2Capability: Capability<&AnyResource{FlowSDKSampleToken.Receiver}>

  // Private minter references for this account to mint tokens
  let minterRef: &FlowSDKSampleToken.VaultMinter

  prepare(acct: AuthAccount) {
    
    let account1 = getAccount(acc1)
    let account2 = getAccount(acc2)

    // Retrieve public Vault Receiver references for both accounts
    self.acct1Capability = account1.getCapability<&AnyResource{FlowSDKSampleToken.Receiver}>(FlowSDKSampleToken.VaultPublicPath)

    self.acct2Capability = account2.getCapability<&AnyResource{FlowSDKSampleToken.Receiver}>(FlowSDKSampleToken.VaultPublicPath)

    // Get the stored Minter reference
    self.minterRef = acct.borrow<&FlowSDKSampleToken.VaultMinter>(from: FlowSDKSampleToken.MinterStoragePath)
        ?? panic("Could not borrow owner's vault minter reference")
  }

  execute {
    // Mint tokens for both accounts
    self.minterRef.mintTokens(amount: 50.0, recipient: self.acct2Capability)
    self.minterRef.mintTokens(amount: 25.0, recipient: self.acct1Capability)
  }
}
