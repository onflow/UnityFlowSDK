import HybridCustody from HYBRID_CUSTODY_ACCOUNT

pub fun main(child: Address): [Address] {
    let acct = getAuthAccount(child)
    let manager = acct.borrow<&HybridCustody.Manager>(from: HybridCustody.ManagerStoragePath)
        ?? panic("manager not found")
    return manager.getOwnedAddresses()
}