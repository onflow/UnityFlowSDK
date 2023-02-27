using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    public class WalletSelectDialogProvider : MonoBehaviour
    {
        public RawImage ProviderIcon;
        public Text ProviderName;
        public GameObject InstalledIndicator;
        public Button SelectButton;

        private WalletSelectDialog.WalletProvider WalletProvider;

        /// <summary>
        /// Initialises WalletSelectDialog Provider button using provided walletProvider object.
        /// </summary>
        /// <param name="walletProvider">The Wallet Provider object to be represented on this button.</param>
        internal void Init(WalletSelectDialog.WalletProvider walletProvider, Action<string> OnSelectedWallet)
        {
            WalletProvider = walletProvider;

            ProviderName.text = walletProvider.Name;
            ProviderIcon.texture = walletProvider.Icon;
            InstalledIndicator.SetActive(walletProvider.IsInstalled);

            SelectButton.onClick.AddListener(() => { OnSelectedWallet(walletProvider.Uri); });
        }
    }
}
