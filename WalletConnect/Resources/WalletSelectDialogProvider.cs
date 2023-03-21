using System;
using UnityEngine;
using UnityEngine.UI;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    /// <summary>
    /// A Wallet Provider in the list of providers in the Wallet Select Dialog. 
    /// </summary>
    public class WalletSelectDialogProvider : MonoBehaviour
    {
        public RawImage ProviderIcon;
        public Text ProviderName;
        public GameObject InstalledIndicator;
        public Button SelectButton;
        private string Uri;

        internal void Init(WalletSelectDialog.WalletProviderData walletProvider, Action<string> OnSelectedWallet)
        {
            ProviderName.text = walletProvider.Name;
            ProviderIcon.texture = walletProvider.Icon;
            InstalledIndicator.SetActive(walletProvider.IsInstalled);
            Uri = walletProvider.Uri;

            SelectButton.onClick.AddListener(() => { OnSelectedWallet(Uri); });
        }
    }
}
