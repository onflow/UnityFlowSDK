using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.AssetImporters;
using UnityEditor.UI;
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
            string text = File.ReadAllText(ctx.assetPath);
            CadenceAsset subAsset = null;
            if(Regex.IsMatch(text, @"^\s*pub\s+fun\s+main\s*\(", RegexOptions.Multiline))
            {
                subAsset = ScriptableObject.CreateInstance<CadenceScriptAsset>();
            }
            else if (Regex.IsMatch(text, @"^\s*(pub.*|access.*)*\s*contract\s*\(*.*{", RegexOptions.Multiline))
            {
                subAsset = ScriptableObject.CreateInstance<CadenceContractAsset>();
            }
            else if (Regex.IsMatch(text, @"\s*(pub.*|access.*)*\s*transaction\s*\(*.*{", RegexOptions.Singleline))
            {
                subAsset = ScriptableObject.CreateInstance<CadenceTransactionAsset>();
            }
            else
            {
                subAsset = ScriptableObject.CreateInstance<CadenceAsset>();
            }
            
            subAsset.text =  File.ReadAllText(ctx.assetPath);
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}