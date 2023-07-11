# iOS Support

To use the Flow SDK in iOS projects, there are a couple more settings you must configure in your Unity project. 

## Provisioning

This is required for all iOS projects, so anyone with experience setting up iOS projects in Unity should be familiar with it. If you are new to developing for iOS on Unity, please follow [Unity's documentation on how to set up a project for iOS](https://docs.unity3d.com/Manual/iphone-GettingStarted.html). 

With iOS selected as the active platform, open the Player Settings (Edit - Project Settings - Player). Scroll down to **Identification** and enter your provisioning details. The fastest way to get up and running is to enter your **Signing Team ID** and check **Automatically Sign**. For a description of all these fields, please refer to [Unity's documentation](https://docs.unity3d.com/Manual/class-PlayerSettingsiOS.html#Identification). 

## IL2CPP Code Generation setting

If your version of Unity is older than 2022.1 (keeping in mind the minimum supported version is 2021.3) you will need to change the following setting: 

1. Open Build Settings (under File). 
2. If iOS is not already the active platform, select iOS and click **Switch Platform**. 
3. Change **IL2CPP Code Generation** to **Faster (smaller) builds**. 

The reason this must be changed is because the Flow SDK utilises generic sharing of value types. For a detailed description on this problem and how it has been fixed in 2022.1, please read this [Unity blog post](https://blog.unity.com/engine-platform/il2cpp-full-generic-sharing-in-unity-2022-1-beta). 

## Managed Stripping Level

Similar to the previous setting, sometimes automatic code stripping on iOS can strip out functions that the optimizer thinks aren't needed, but they actually are. We highly recommend you change this setting to avoid any of these issues. 

1. Open Project Settings (under Edit). 
2. Go to the Player tab. 
3. Expand Other Settings and scroll down to Optimization. 
4. Change **Managed Stripping Level** to **Minimal**. 