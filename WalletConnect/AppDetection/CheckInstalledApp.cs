// MIT License
   
// Copyright (c) 2019 the16bitgamer
   
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
   
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
   
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;
using UnityEngine.UI;

namespace DapperLabs.Flow.Sdk.WalletConnect 
{
    public class CheckInstalledApp : MonoBehaviour {

        //We are Checking if Robotipede is installed on your current Device

        //For Android App we check the Bundle ID.
        public string AppToCheckAndroid = "com.SixteenBitGames.Bezerk";

        //For iOS we needed a bit of a work around, so we are checking if the app can open a specific URL and we are checking if that URL can be opened
        public string AppToCheckiOS = "SixteenBitApp06";

        public string WebStoreForAndroid = "https://play.google.com/store/apps/details?id=com.SixteenBitGames.Bezerk";
        public string WebStoreForiOS = "https://itunes.apple.com/us/app/new-berzerk/id1171452875?ls=1&mt=8";

        public Text textBx;
        public Image img;

        GetAppInfo appCheck;

        void Start() {
            appCheck = new GetAppInfo();
        }

        void Update()
        {
            if (appCheck != null)
            {
                string appToCheck = AppToCheckiOS;
                if (CheckAndroid)
                {
                    appToCheck = AppToCheckAndroid;
                }

                if (appCheck.CheckInstalledApp(appToCheck))
                {
                    AppInstalled();
                }
                else
                {
                    AppNotInstalled();
                }
            }
        }

        public void DownloadApp()
        {
            string webStore = WebStoreForiOS;
            if (CheckAndroid)
            {
                webStore = WebStoreForAndroid;
            }

            Application.OpenURL(webStore);
        }

        void AppInstalled()
        {
            textBx.text = "App is Installed";
            img.color = new Color(0, 1, 1);
        }
        void AppNotInstalled()
        {
            textBx.text = "App is Not Installed";
            img.color = new Color(1, 0, 0);
        }

    #if UNITY_ANDROID
        bool CheckAndroid
        {
            get { return true; }
        }
    #else
        bool CheckAndroid
        {
            get {   return false; }
        }
    #endif
    }
}
