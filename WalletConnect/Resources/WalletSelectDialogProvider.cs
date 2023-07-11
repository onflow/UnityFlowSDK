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

        internal void Init(WalletSelectDialog.WalletProviderData walletProvider, Action<WalletSelectDialog.WalletProviderData> OnSelectedWallet)
        {
            ProviderName.text = walletProvider.Name;
            ProviderIcon.texture = walletProvider.Icon;
            InstalledIndicator.SetActive(walletProvider.IsInstalled);

            SelectButton.onClick.AddListener(() => { OnSelectedWallet(walletProvider); });
        }
    }
}
