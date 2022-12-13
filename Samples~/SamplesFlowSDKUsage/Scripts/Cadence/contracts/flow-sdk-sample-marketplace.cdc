import FlowSDKSampleToken from "./flow-sdk-sample-token.cdc"
import FlowSDKSampleNFT from "./flow-sdk-sample-nft.cdc"

// This contract is a learning tool and is not meant to be used in production.
// See the NFTStorefront contract for a generic marketplace smart contract that 
// is used by many different projects on the Flow blockchain:
//
// https://github.com/onflow/nft-storefront

pub contract FlowSDKSampleMarketplace {

    // Event that is emitted when a new NFT is put up for sale
    pub event ForSale(id: UInt64, price: UFix64, owner: Address?)

    // Event that is emitted when the price of an NFT changes
    pub event PriceChanged(id: UInt64, newPrice: UFix64, owner: Address?)

    // Event that is emitted when a token is purchased
    pub event TokenPurchased(id: UInt64, price: UFix64, seller: Address?, buyer: Address?)

    // Event that is emitted when a seller withdraws their NFT from the sale
    pub event SaleCanceled(id: UInt64, seller: Address?)

    // Data structure to store active sales. Needed if you want a centralised list of sales to get, ie to show a user
    pub var tokensForSale: {Address: Capability<&SaleCollection{SalePublic}>}

    // Interface that users will publish for their Sale collection
    // that only exposes the methods that are supposed to be public
    //
    pub resource interface SalePublic {
        pub fun purchase(tokenID: UInt64, recipient: Capability<&AnyResource{FlowSDKSampleNFT.NFTReceiver}>, buyTokens: @FlowSDKSampleToken.Vault)
        pub fun idPrice(tokenID: UInt64): UFix64?
        pub fun getIDs(): [UInt64]
    }

    // SaleCollection
    //
    // NFT Collection object that allows a user to put their NFT up for sale
    // where others can send Tokens to purchase it
    //
    pub resource SaleCollection: SalePublic {

        /// A capability for the owner's collection
        access(self) var ownerCollection: Capability<&FlowSDKSampleNFT.Collection>

        // Dictionary of the prices for each NFT by ID
        access(self) var prices: {UInt64: UFix64}

        // The Token vault of the owner of this sale.
        // When someone buys an NFT, this resource can deposit
        // Tokens into their account.
        access(account) let ownerVault: Capability<&AnyResource{FlowSDKSampleToken.Receiver}>

        init (ownerCollection: Capability<&FlowSDKSampleNFT.Collection>, 
              ownerVault: Capability<&AnyResource{FlowSDKSampleToken.Receiver}>) {

            pre {
                // Check that the owner's collection capability is correct
                ownerCollection.check(): 
                    "Owner's NFT Collection Capability is invalid!"

                // Check that the Token vault capability is correct
                ownerVault.check(): 
                    "Owner's Receiver Capability is invalid!"
            }
            self.ownerCollection = ownerCollection
            self.ownerVault = ownerVault
            self.prices = {}
        }

        // cancelSale gives the owner the opportunity to cancel a sale in the collection
        pub fun cancelSale(tokenID: UInt64) {
            // remove the price
            self.prices.remove(key: tokenID)
            self.prices[tokenID] = nil

            // Nothing needs to be done with the actual NFT because it is already in the owner's collection
        }

        // listForSale lists an NFT for sale in this collection
        pub fun listForSale(tokenID: UInt64, price: UFix64) {
            pre {
                self.ownerCollection.borrow()!.idExists(id: tokenID):
                    "NFT to be listed does not exist in the owner's collection"
            }
            // store the price in the price array
            self.prices[tokenID] = price

            emit ForSale(id: tokenID, price: price, owner: self.owner?.address)
        }

        // changePrice changes the price of a token that is currently for sale
        pub fun changePrice(tokenID: UInt64, newPrice: UFix64) {
            self.prices[tokenID] = newPrice

            emit PriceChanged(id: tokenID, newPrice: newPrice, owner: self.owner?.address)
        }

        // purchase lets a user send Tokens to purchase an NFT that is for sale
        pub fun purchase(tokenID: UInt64, recipient: Capability<&AnyResource{FlowSDKSampleNFT.NFTReceiver}>, buyTokens: @FlowSDKSampleToken.Vault) {
            pre {
                self.prices[tokenID] != nil:
                    "No NFT matching this ID for sale!"
                buyTokens.balance >= (self.prices[tokenID] ?? 0.0):
                    "Not enough Tokens to buy the NFT!"
                recipient.borrow != nil:
                    "Invalid NFT receiver capability!"
            }

            // get the value out of the optional
            let price = self.prices[tokenID]!

            self.prices[tokenID] = nil

            let vaultRef = self.ownerVault.borrow()
                ?? panic("Could not borrow reference to owner token vault")

            // deposit the purchasing tokens into the owners vault
            vaultRef.deposit(from: <-buyTokens)

            // borrow a reference to the object that the receiver capability links to
            // We can force-cast the result here because it has already been checked in the pre-conditions
            let receiverReference = recipient.borrow()!

            // deposit the NFT into the buyers collection
            receiverReference.deposit(token: <-self.ownerCollection.borrow()!.withdraw(withdrawID: tokenID))

            emit TokenPurchased(id: tokenID, price: price, seller: self.owner?.address, buyer: receiverReference.owner?.address)
        }

        // idPrice returns the price of a specific NFT in the sale
        pub fun idPrice(tokenID: UInt64): UFix64? {
            return self.prices[tokenID]
        }

        // getIDs returns an array of NFT IDs that are for sale
        pub fun getIDs(): [UInt64] {
            return self.prices.keys
        }
    }

    // createCollection returns a new Sale Collection resource to the caller
    pub fun createSaleCollection(ownerCollection: Capability<&FlowSDKSampleNFT.Collection>, 
                                 ownerVault: Capability<&AnyResource{FlowSDKSampleToken.Receiver}>): @SaleCollection {
        return <- create SaleCollection(ownerCollection: ownerCollection, ownerVault: ownerVault)
    }

    // listSaleCollection lists a users sale reference in the dictionary
    pub fun listSaleCollection(collection: Capability<&SaleCollection{SalePublic}>) {
        let saleRef = collection.borrow()
            ?? panic("Invalid sale collection capability")

        self.tokensForSale[saleRef.owner!.address] = collection
    }

    // removeSaleCollection removes a user's sale collection from the dictionary
    // of sale references
    pub fun removeSaleCollection(owner: Address) {
        self.tokensForSale[owner] = nil
    }

    // getTokensForSale returns an array of all sale collection capabilities
    pub fun getTokensForSale(): [Capability<&SaleCollection{SalePublic}>] {
        return self.tokensForSale.values
    }

    init() {
        self.tokensForSale = {}
    }
}
