using System;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    public class QRCodeDialog : MonoBehaviour
    {
        [Header("Loading Indicator")]
        public GameObject LoadingIndicator;

        [Header("Dialog")]
        public GameObject Dialog;
        public RawImage QrCodeObject;
        public Text UriCopyButtonText;

        private bool _Initialised = false;
        private string _Uri = "";

        private void OnEnable()
        {
            if (_Initialised == false)
            {
                // hide dialog and show loading indicator
                Dialog.SetActive(false);
                LoadingIndicator.SetActive(true);
            }
        }

        /// <summary>
        /// Example: Init(exampleUri.text);
        /// </summary>
        /// <param name="uri">The WalletConnect uri to be presented as a QR code.</param>
        public bool Init(string uri)
        {
            if (QrCodeObject == null)
            {
                Debug.LogError("<b>QrCodeObject</b> component reference not assigned on QrCodeDialog. Unable to render QR code.", this);
                return false;
            }

            if (UriCopyButtonText == null)
            {
                Debug.LogError("<b>UriCopyButtonText</b> component reference not assigned on QrCodeDialog. Unable to render URI.", this);
                return false;
            }

            _Uri = uri;

            Texture2D myQR = generateQR(uri);
            QrCodeObject.texture = myQR;

            _Initialised = true;
            Dialog.SetActive(true);
            LoadingIndicator.SetActive(false);

            return true;
        }

        private static Color32[] Encode(string textForEncoding, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width
                }
            };
            return writer.Write(textForEncoding);
        }

        private Texture2D generateQR(string text)
        {
            var encoded = new Texture2D(256, 256);
            var color32 = Encode(text, encoded.width, encoded.height);
            encoded.SetPixels32(color32);
            encoded.Apply();
            return encoded;
        }

        public void OnCopyUriClicked()
        {
            UriCopyButtonText.text = "<b>Copied</b>";
            GUIUtility.systemCopyBuffer = _Uri;
        }

        public void OnCopyUriMouseHoverEnter()
        {
            UriCopyButtonText.text = "<b>Copy to Clipboard</b>";
        }

        public void OnCopyUriMouseHoverExit()
        {
            UriCopyButtonText.text = "Copy to Clipboard";
        }

        public void OnCloseButtonClicked()
        {
            Destroy(gameObject);
        }
    }
}
