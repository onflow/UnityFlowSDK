#if UNITY_IOS

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    internal class BuildPostProcessor
    {

        [PostProcessBuild]
        internal static void ChangeXcodePlist(BuildTarget buildTarget, string path)
        {

            if (buildTarget == BuildTarget.iOS)
            {

                string plistPath = path + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromFile(plistPath);

                PlistElementDict rootDict = plist.root;

                PlistElementArray arr = rootDict.CreateArray("LSApplicationQueriesSchemes");
                arr.AddString("dapper-pro");
                arr.AddString("lilico");

                File.WriteAllText(plistPath, plist.WriteToString());
            }
        }
    }
}

#endif