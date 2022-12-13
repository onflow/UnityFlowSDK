using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
#if UNITY_2020_1_OR_NEWER
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace DapperLabs.Flow.Sdk.Unity
{
    [ScriptedImporter(1, "cdc")]
    public class CDCImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            TextAsset subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}