using DapperLabs.Flow.Sdk.Cadence;
using UnityEngine;
using UnityEngine.UI;

namespace NFTViewerExample
{
    public class CollectionButton : MonoBehaviour
    {
        public CadencePathValue path;

        // Start is called before the first frame update
        void Start()
        {
            GetComponentInChildren<Text>().text = path.Identifier;
        }

        public void OnClicked()
        {
            //Let the NFTViewer know that a collection was selected.
            FindObjectOfType<NFTViewer>().OnCollectionButtonClicked(path);
        }
    }
}