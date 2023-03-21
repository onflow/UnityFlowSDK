using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DapperLabs.Flow.Sdk.Unity;
using DapperLabs.Flow.Sdk.Crypto;
using DapperLabs.Flow.Sdk.DataObjects;
using System;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

// suppress async without await warning as DevWallet is synchronous
#pragma warning disable CS1998

namespace DapperLabs.Flow.Sdk.DevWallet
{
    /// <summary>
    /// This wallet implementation is only to be used for development purposes (emulator and testnet). 
    /// It's an example of how to implement the IWallet interface, and provides a way to sign transactions
    /// during development against an emulator or testnet. Calling Authenticate() will display a list of 
    /// accounts from the Accounts tab of the Flow Control Window. The selected account will be used to
    /// sign any transactions. 
    /// WARNING: DO NOT use this for production - it is NOT secure. 
    /// </summary>
    public class DevWalletProvider : IWallet
    {
        SdkAccount authorizedAccount = null;
        System.Action<string> OnAuthSuccessCallback = null;
        System.Action OnAuthFailedCallback = null;
        GameObject accountDialog = null;
        GameObject approveDialog = null;

        private void Awake()
        {
            Debug.LogWarning("WARNING: You are using DevWallet - this is only intended to be used for development purposes on emulator and testnet. DO NOT use this for Production, as it is NOT secure.");
        }

        /// <summary>
        /// Not used by DevWallet. 
        /// </summary>
        /// <param name="config">Ignored.</param>
        void IWallet.Init(WalletConfig config)
        {

        }

        /// <summary>
        /// Displays a list of accounts from the Accounts tab of the Flow Control Window. 
        /// </summary>
        /// <param name="username">The username of the account to be authenticated. If blank, a dialog will appear to select an account.</param>
        /// <param name="OnAuthSuccess">Called when the user selects an account and clicks Ok.</param>
        /// <param name="OnAuthFailed">Called when the user clicks Cancel.</param>
        async Task IWallet.Authenticate(string username, System.Action<string> OnAuthSuccess, System.Action OnAuthFailed)
        {
            OnAuthSuccessCallback = OnAuthSuccess;
            OnAuthFailedCallback = OnAuthFailed;

            if (username != "")
            {
                authorizedAccount = FlowControl.GetSdkAccountByName(username);

                if (authorizedAccount == null && OnAuthFailedCallback != null)
                {
                    OnAuthFailedCallback();
                }
                
                if (OnAuthSuccessCallback != null && authorizedAccount != null)
                {
                    OnAuthSuccessCallback(authorizedAccount.Address);
                }

                return;
            }

            UnityEngine.Object prefab = Resources.Load("AccountDialogPrefab");

            // Instantiate the prefab which shows a list of dev accounts to select from
            if (accountDialog == null)
            {
                accountDialog = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            }

            // Get the script component and initialize it
            AccountDialog accDialogScript = accountDialog.GetComponentInChildren<AccountDialog>();
            accDialogScript.Init(FlowControl.GatewayCache.Values.ToArray()[0], OnAuthenticateSuccess, OnAuthenticateFailed);
        }

        private void OnAuthenticateSuccess(string authAccount)
        {
            authorizedAccount = FlowControl.GetSdkAccountByAddress(authAccount);
            if (OnAuthSuccessCallback != null)
            {
                OnAuthSuccessCallback(authAccount);
            }

            UnityEngine.Object.Destroy(accountDialog);
        }

        private void OnAuthenticateFailed()
        {
            if (OnAuthFailedCallback != null)
            {
                OnAuthFailedCallback();
            }

            UnityEngine.Object.Destroy(accountDialog);
        }

        /// <summary>
        /// Signs a flow transaction's payload as the authenticated user. 
        /// </summary>
        /// <param name="txn">The transaction to be signed.</param>
        /// <returns>The signature in bytes.</returns>
        async Task<byte[]> IWallet.SignTransactionPayload(FlowTransaction txn)
        {
            return await SignTransaction(txn, true);
        }

        /// <summary>
        /// Signs a flow transaction's authorization envelope as the authenticated user.
        /// </summary>
        /// <param name="txn">The transaction to be signed.</param>
        /// <returns>The signature in bytes.</returns>
        async Task<byte[]> IWallet.SignTransactionEnvelope(FlowTransaction txn)
        {
            return await SignTransaction(txn, false);
        }

        private async Task<byte[]> SignTransaction(FlowTransaction txn, bool signPayload)
        {
            if (authorizedAccount == null)
            {
                Debug.LogError("Cannot approve a transaction - you must authenticate first.");
                return null;
            }

#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
            {
                if (signPayload)
                {
                    return SignPayload(txn, authorizedAccount.PrivateKey);
                }
                else
                {
                    return SignAuthorizationEnvelope(txn, authorizedAccount.PrivateKey);
                }
            }
#endif

            // show prefab to approve the transaction
            UnityEngine.Object prefab = Resources.Load("TransactionDialogPrefab");

            // Instantiate the prefab which shows a list of dev accounts to select from
            if (approveDialog == null)
            {
                approveDialog = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            }

            // Get the script component and initialize it
            TransactionDialog approveTxnScript = approveDialog.GetComponentInChildren<TransactionDialog>();

            bool isComplete = false;
            byte[] signature = null;

            approveTxnScript.Init(txn.Script, () =>
            {
                if (signPayload)
                {
                    signature = SignPayload(txn, authorizedAccount.PrivateKey);
                }
                else
                {
                    signature = SignAuthorizationEnvelope(txn, authorizedAccount.PrivateKey);
                }

                isComplete = true;
            }, () =>
            {
                isComplete = true;
            });

            while (!isComplete)
            {
                await Task.Delay(1000);
            }

            UnityEngine.Object.Destroy(approveDialog);

            return signature;
        }

        /// <summary>
        /// Gets if a user is authenticated. 
        /// </summary>
        /// <returns>True if a user is authenticated.</returns>
        bool IWallet.IsAuthenticated()
        {
            return authorizedAccount != null;
        }

        /// <summary>
        /// Unauthenticates the user.
        /// </summary>
        void IWallet.Unauthenticate()
        {
            authorizedAccount = null;
            OnAuthSuccessCallback = null;
        }

        private byte[] SignPayload(FlowTransaction txn, string privateKeyHex)
        {
            ISigner signer = Utilities.CreateSigner(privateKeyHex, SignatureAlgo.ECDSA_P256, HashAlgo.SHA3_256);

            byte[] canonicalPayload = Rlp.EncodedCanonicalPayload(txn);
            byte[] message = DomainTag.AddTransactionDomainTag(canonicalPayload);
            return signer.Sign(message);
        }

        private byte[] SignAuthorizationEnvelope(FlowTransaction txn, string privateKeyHex)
        {
            ISigner signer = Utilities.CreateSigner(privateKeyHex, SignatureAlgo.ECDSA_P256, HashAlgo.SHA3_256);
            
            byte[] canonicalAuthorizationEnvelope = Rlp.EncodedCanonicalAuthorizationEnvelope(txn);
            byte[] message = DomainTag.AddTransactionDomainTag(canonicalAuthorizationEnvelope);
            return signer.Sign(message);
        }

        /// <summary>
        /// Retrieves the Flow Control Account of the authenticated user. 
        /// </summary>
        /// <returns>The Flow Control Account of the authenticated user.</returns>
        SdkAccount IWallet.GetAuthenticatedAccount()
        {
            if (authorizedAccount == null)
            {
                return null;
            }

            return authorizedAccount;
        }
    }
}

#pragma warning restore CS1998
