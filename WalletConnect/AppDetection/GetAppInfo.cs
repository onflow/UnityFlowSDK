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

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    internal class GetAppInfo
    {

        internal GetAppInfo()
        {
            Initilized();
        }

        internal bool CheckInstalledApp(string APPID)
        {
            return CheckApps(APPID);
        }

#if UNITY_IOS

        [DllImport("__Internal")]
        private static extern void InitilizeAppCheck();

        [DllImport("__Internal")]
        private static extern bool CheckApp(string URL);


        void Initilized()
        {   
            InitilizeAppCheck();
        }

        internal bool CheckApps(string APP)
        {
            bool check = CheckApp(APP);
            return check;
        }
#elif UNITY_ANDROID
        private AndroidJavaObject GetApps = null;
	    private AndroidJavaObject activityContext = null;

	    void Initilized()
	    {
		    if (Application.platform == RuntimePlatform.Android) {
			    if (GetApps == null) {
				    using (AndroidJavaClass activityClass = new AndroidJavaClass ("com.unity3d.player.UnityPlayer")) {
					    activityContext = activityClass.GetStatic<AndroidJavaObject> ("currentActivity");
				    }

				    using (AndroidJavaClass pluginClass = new AndroidJavaClass ("com.sixteenbitpluggin.appcheck.SearchApps")) {
					    Debug.Log ("Checks " +pluginClass);
					    if (pluginClass != null) {
						    GetApps = pluginClass.CallStatic<AndroidJavaObject> ("instance");
						    GetApps.Call ("setContext", activityContext);
					    }
				    }
			    }
		    }
	    }

	    internal bool CheckApps(string APP)
	    {
            if (GetApps == null)
                Initilized();
            return GetApps.Call<bool>("appInstalledOrNot", APP);
	    }
#else
        void Initilized()
        {
            Debug.Log("Initilized on AppCheck");
        }

        private bool CheckApps(string APP)
        {
            return false;
        }
#endif
    }
}
