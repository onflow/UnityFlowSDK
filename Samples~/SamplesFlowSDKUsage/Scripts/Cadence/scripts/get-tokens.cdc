import FlowSDKSampleToken from 0xf3fcd2c1a78f5eee

// Print the balance of Tokens owned by account.
pub fun main(account: Address): UFix64 {
    // Get the public account object
    let owner = getAccount(account)

    // Find the public Receiver capability for their Collection
    let capability = owner.getCapability<&FlowSDKSampleToken.Vault{FlowSDKSampleToken.Receiver, FlowSDKSampleToken.Balance}>(FlowSDKSampleToken.VaultPublicPath)

    // borrow a reference from the capability
    let vaultRef = capability.borrow()
        ?? panic("Could not borrow the receiver reference")

    return vaultRef.balance
}
