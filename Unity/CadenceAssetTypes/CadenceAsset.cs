using UnityEngine;

namespace DapperLabs.Flow.Sdk.Unity
{
    public class CadenceAsset : ScriptableObject
    {
        public string text;
        
        public override string ToString()
        {
            return text;
        }
    }
}