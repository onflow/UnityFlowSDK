using System;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    /// <summary>
    /// The script which must be added to the QR Code Dialog prefab. 
    /// </summary>
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
        private Action _OnCancelled = null;

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
        /// Generates the QR code. 
        /// </summary>
        /// <param name="uri">The WalletConnect uri to be presented as a QR code.</param>
        /// <param name="onCancelled">Callback for when users cancel the QR Code dialog.</param>
        public bool Init(string uri, Action onCancelled)
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
            _OnCancelled = onCancelled;

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

        /// <summary>
        /// Callback when Copy Uri button is clicked. 
        /// </summary>
        public void OnCopyUriClicked()
        {
            UriCopyButtonText.text = "<b>Copied</b>";
            GUIUtility.systemCopyBuffer = _Uri;
        }

        /// <summary>
        /// Callback when mouse hovers over Copy Uri button. 
        /// </summary>
        public void OnCopyUriMouseHoverEnter()
        {
            UriCopyButtonText.text = "<b>Copy to Clipboard</b>";
        }

        /// <summary>
        /// Callback when mouse leaves Copy Uri button. 
        /// </summary>
        public void OnCopyUriMouseHoverExit()
        {
            UriCopyButtonText.text = "Copy to Clipboard";
        }

        /// <summary>
        /// Callback when Close button is clicked.
        /// </summary>
        public void OnCloseButtonClicked()
        {
            _OnCancelled();
            Destroy(gameObject);
        }
    }
}
