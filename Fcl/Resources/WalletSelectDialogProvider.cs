using System;
using Fcl.Net.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DapperLabs.Flow.Sdk.Fcl
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
        private FclServiceMethod Method;
        private string Endpoint;
        private string Uid;

        internal void Init(FclWalletProvider walletProvider, Action<FclServiceMethod, string, string> OnSelectedWallet)
        {
            ProviderName.text = walletProvider.Name;

            try
            {
                Texture2D tex = new Texture2D(1, 1);
                string base64 = walletProvider.Logo.Substring(22);
                tex.LoadImage(Convert.FromBase64String(base64));
                tex.Apply();
                ProviderIcon.texture = tex;
                //ProviderIcon = Sprite.Create(newImage, new Rect(0, 0, newImage.width, newImage.height), ProviderIcon.pivot);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Fcl: WalletSelectDialogProvider: Exception thrown creating Logo: {ex.Message}");
            }
            
            InstalledIndicator.SetActive(false);
            Method = walletProvider.Method;
            Endpoint = walletProvider.Endpoint;
            Uid = walletProvider.Uid;

            SelectButton.onClick.AddListener(() => { OnSelectedWallet(Method, Endpoint, Uid); });
        }
    }
}
