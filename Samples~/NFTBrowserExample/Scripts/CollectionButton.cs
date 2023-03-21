using DapperLabs.Flow.Sdk.Cadence;
using TMPro;
using UnityEngine;

namespace NFTViewerExample
{
    public class CollectionButton : MonoBehaviour
    {
        public CadencePathValue path;

        // Start is called before the first frame update
        void Start()
        {
            GetComponentInChildren<TMP_Text>().text = path.Identifier;
        }

        public void OnClicked()
        {
            //Let the NFTViewer know that a collection was selected.
            FindObjectOfType<NFTViewer>().OnCollectionButtonClicked(path);
        }
    }
}