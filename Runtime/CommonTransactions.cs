using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.Constants;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Crypto;
using Org.BouncyCastle.Crypto;

namespace DapperLabs.Flow.Sdk
{
	/// <summary>
	/// Contains helpers for commonly used transactions to support FlowControl functions.
	/// </summary>
    public static class CommonTransactions
    {
	    /// <summary>
	    /// Holds a hex encoded Asymmetric Cipher Key Pair 
	    /// </summary>
	    public struct KeyPair
	    {
		    public string privateKey;
		    public string publicKey;
	    }

	    /// <summary>
	    /// Generates new Asymmetric Cipher Key Pair in hex
	    /// </summary>
	    /// <returns>A Keypair struct</returns>
	    public static KeyPair GenerateKeypair()
	    {
		    AsymmetricCipherKeyPair newKeys = CryptoUtils.GenerateKeyPair();
		    return new KeyPair
		    {
			    publicKey = CryptoUtils.DecodePublicKeyToHex(newKeys),
			    privateKey = CryptoUtils.DecodePrivateKeyToHex(newKeys)
		    };
	    }

	    /// <summary>
	    /// Create a new %Flow account
	    /// </summary>
	    /// <param name="newAccountName">The name of the flow account.  This is arbitrary and only used as a convenience
	    /// to keep track of accounts.</param>
	    /// <returns>A Task that will resolve to an SdkAccount object representing the newly created account</returns>
        public static async Task<SdkAccount> CreateAccount(string newAccountName)
        {
            AsymmetricCipherKeyPair newKeys = CryptoUtils.GenerateKeyPair();
            string publicKey = CryptoUtils.DecodePublicKeyToHex(newKeys);
            string privateKey = CryptoUtils.DecodePrivateKeyToHex(newKeys);

            const string txScript = @"
				transaction(publicKey: String) {
				    prepare(signer: AuthAccount) {
				        let key = PublicKey(
				            publicKey: publicKey.decodeHex(),
				            signatureAlgorithm: SignatureAlgorithm.ECDSA_P256
				        )

				        let account = AuthAccount(payer: signer)

				        account.keys.add(
				            publicKey: key,
				            hashAlgorithm: HashAlgorithm.SHA3_256,
				            weight: 1000.0
				        )
				    }
				}";

            List<CadenceBase> txArgs = new List<CadenceBase> { new CadenceString(publicKey) };


            FlowTransactionResponse response = await Transactions.Submit(txScript, txArgs);

            if (response.Error != null)
            {
                return new SdkAccount
                {
	                Error = new FlowError($"Error submitting account transaction. {response.Error.Message}", response.Error.Exception)
                };
            }
            
            while (true)
            {
	            for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(1000);
                    FlowTransactionResult result = await Transactions.GetResult(response.Id);
            
                    if (result.Error != null)
                    {
	                    continue;
                    }
            
                    switch (result.Status)
                    {
	                    case FlowTransactionStatus.SEALED:
	                    {
		                    FlowEvent ev = result.Events.FirstOrDefault(w => w.Type == EventTypes.AccountCreated);
	                    
		                    if (ev == null || ev.Payload == null)
		                    {
			                    return new SdkAccount
			                    {
				                    Error = new FlowError("Flow Create account event not found.")
			                    };
		                    }

		                    CadenceComposite composite = (CadenceComposite)ev.Payload;
		                    CadenceAddress newAccountAddress = composite.CompositeFieldAs<CadenceAddress>("address");

		                    return new SdkAccount
		                    {
			                    Address = newAccountAddress.Value,
			                    Name = newAccountName,
			                    PrivateKey = privateKey
		                    };
	                    }
	                    case FlowTransactionStatus.EXPIRED:
		                    return new SdkAccount
		                    {
			                    Error = result.Error
		                    };
                    }
                }
                
                return new SdkAccount
                {
	                Error = new FlowError("Create account transaction took too long to complete.")
                };
            }
        }

	    /// <summary>
	    /// Deploys the given contract to the given %Flow account.
	    /// </summary>
	    /// <param name="name">Name of the contract to deploy</param>
	    /// <param name="code">Text contents of the contract to deploy</param>
	    /// <returns>A Task the will resolve to a FlowTransactionResponse for the transaction deploying the contract.</returns>
        public static async Task<FlowTransactionResponse> DeployContract(string name, string code)
	    {
		    const string txScript = @"
				transaction(name: String, code: String) {
					prepare(signer: AuthAccount) {
						signer.contracts.add(name: name, code: code.decodeHex())
					}
				}";

            byte[] codeBytes = Encoding.UTF8.GetBytes(code);
            string codeHex = BitConverter.ToString(codeBytes).Replace("-", "").ToLower();

            List<CadenceBase> txArgs = new List<CadenceBase>
            {
	            new CadenceString(name),
	            new CadenceString(codeHex)
            };

			return await Transactions.Submit(txScript, txArgs);
		}

	    /// <summary>
	    /// Removes the contract with the given name from the given account.
	    /// </summary>
	    /// <remarks>Once a contract is deleted from an account, that name will not be available for use by new contracts on that account.</remarks>
	    /// <param name="name">Name of the contract that will be removed.</param>
	    /// <returns>A Task that will resolve to a FlowTransactionResponse for the transaction removing the contract.</returns>
        public static async Task<FlowTransactionResponse> RemoveContract(string name)
	    {
		    const string txScript = @"
				transaction(name: String) {
					prepare(signer: AuthAccount) {
						signer.contracts.remove(name: name)
					}
				}";

            List<CadenceBase> txArgs = new List<CadenceBase> { new CadenceString(name) };

			return await Transactions.Submit(txScript, txArgs);
	    }

	    /// <summary>
	    /// Updates the contract with the given name on the given account with new contents.
	    /// </summary>
	    /// <remarks>For contract update limitations see:  https://developers.flow.com/cadence/language/contract-updatability#updating-a-contract</remarks>
	    /// <param name="name">The name of the contract to update</param>
	    /// <param name="code">The text of the contract that will replace the old contract text</param>
	    /// <returns>A Task that will resolve to a FlowTransactionResponse of the transaction updating the contract</returns>
        public static async Task<FlowTransactionResponse> UpdateContract(string name, string code)
	    {
		    const string txScript = @"
				transaction(name: String, code: String) {
					prepare(signer: AuthAccount) {
						signer.contracts.update__experimental(name: name, code: code.decodeHex())
					}
				}";

            byte[] codeBytes = Encoding.UTF8.GetBytes(code);
            string codeHex = BitConverter.ToString(codeBytes).Replace("-", "").ToLower();

            List<CadenceBase> txArgs = new List<CadenceBase>
            {
	            new CadenceString(name),
	            new CadenceString(codeHex)
            };
            
			return await Transactions.Submit(txScript, txArgs);
		}
    }
}