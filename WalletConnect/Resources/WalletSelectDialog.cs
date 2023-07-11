using System;
using UnityEngine;
using UnityEngine.UI;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    /// <summary>
    /// The script which must be added to the Wallet Select Dialog prefab. 
    /// </summary>
    public class WalletSelectDialog : MonoBehaviour
    {
        [Header("Loading Indicator")]
        public GameObject LoadingIndicator;

        [Header("Dialog")]
        public GameObject Dialog;
        public Text DialogHeaderText;
        public Transform DialogScrollViewContent;

        [Header("Prefabs")]
        public GameObject WalletSelectProviderPrefab;

        private Action<WalletSelectDialog.WalletProviderData> _OnSelectedWallet = null;
        private bool _Initialised = false;
        private float _loadingTimer = 0.0f;

        private void OnEnable()
        {
            if (_Initialised == false)
            {
                // hide dialog and show loading indicator
                Dialog.SetActive(false);
                LoadingIndicator.SetActive(true);
                _loadingTimer = 10.0f;
            }
        }

        private void Update()
        {
            if (_loadingTimer > 0.0f)
            {
                _loadingTimer -= Time.deltaTime;
                if (_loadingTimer <= 0.0f)
                {
                    Debug.LogError("Wallet Select dialog timed out.");
                    Destroy(gameObject);
                }
            }
        }

        /// <example>
        /// Generates the list of wallet providers. 
        /// </example>
        /// <param name="header">Header for the dialog.</param>
        /// <param name="providers">Array of WalletSelectDialog.WalletProviders to present to the user.</param>
        /// <param name="OnSelectedWallet">Callback to call when user selects a wallet provider.</param>
        public bool Init(string header, WalletProviderData[] providers, Action<WalletSelectDialog.WalletProviderData> OnSelectedWallet)
        {
            if (WalletSelectProviderPrefab == null)
            {
                Debug.LogError("<b>WalletSelectProviderPrefab</b> component reference not assigned on WalletSelectDialog. Unable to list wallets.", this);
                return false;
            }

            _OnSelectedWallet = OnSelectedWallet;

            DialogHeaderText.text = header;

            for (int i = 0; i < providers.Length; i++)
            {
                GameObject newProvider = Instantiate(WalletSelectProviderPrefab, DialogScrollViewContent);
                newProvider.GetComponent<WalletSelectDialogProvider>().Init(providers[i], _OnSelectedWallet);
            }

            _Initialised = true;
            Dialog.SetActive(true);
            LoadingIndicator.SetActive(false);

            _loadingTimer = 0.0f;

            return true;
        }

        /// <summary>
        /// Data to construct a Wallet Provider. 
        /// </summary>
        public struct WalletProviderData
        {
            public string Name;
            public Texture2D Icon;
            public bool IsInstalled;
            public string BaseUri;
            public string ConnectUri;
        }

        /// <summary>
        /// Callback for when the Close button is pressed. 
        /// </summary>
        public void OnCloseButtonClicked()
        {
            Destroy(gameObject);
        }
    }
}
