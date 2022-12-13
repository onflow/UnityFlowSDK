# Flow Unity SDK - FlowWords Sample

This sample requires some initial set-up, in order to get the emulator into a suitable state to run the game.

We assume you have already added the SDK package to your Unity project, and Imported the FlowWords Sample using the Unity Package Manager.
If you have not already done so, do this now.

1. Open Game.scene from the FlowWords sample folder. (Samples\Flow SDK\\\<version>\Flow Words\Scenes)
2. In the scene Heirarchy, Select the 'Flow Control' gameobject.
3. In the inspector for the Flow Control gameobject, Click the 'Open Flow Control Window' button.
4. On the Emulator Settings tab, click the ellipsis beside the Emulator Data Directory field, and navigate to: Assets\Samples\Flow SDK\\\<version>\Flow Words\Resources
5. The Start Emulator button should now be available. Click this button to Start the Emulator. (if this is not the case, please check that the emulator data directory is pointing to the sample's resources directory, where the flow.json file resides.)

>**FOR Windows:**
>
>6. In the project window, navigate to Assets\Samples\Flow SDK\\\<version>\Flow Words\Resources. Right click on emulator_test_data.bat, and click Show in Explorer.
>7. Double Click emulator_test_data.bat from the Explorer window to run. This will create some default user accounts and deploy the game contract.

>**FOR Mac:**
>
>6. In the project window, navigate to Assets\Samples\Flow SDK\\\<version>\Flow Words\Resources. Right click on emulator_test_data.command, and click Show in Finder.
>7. Double Click emulator_test_data.command from the Finder window to run. This will create some default user accounts and deploy the game contract.


8. You may now press Play in Unity to run the game sample.