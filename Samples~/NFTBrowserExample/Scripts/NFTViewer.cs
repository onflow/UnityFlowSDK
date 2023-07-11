using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Convert = DapperLabs.Flow.Sdk.Cadence.Convert;

namespace NFTViewerExample
{
    public class NFTViewer : MonoBehaviour
    {
        //The cadence scripts that gets the information we're interest in
        public CadenceScriptAsset getCollections;
        public CadenceScriptAsset getNftIdsForCollection;
        public CadenceScriptAsset getDisplayDataForIDs;
        public CadenceScriptAsset getFullDataForID;

        //Prefab of the button each collection will have
        public GameObject collectionButtonPrefab;

        //Reference to the panel that the collectionButtonPrefabs will be children of
        public GameObject collectionPanel;

        //Reference to the panel that the NFT images will be displayed in
        public GameObject nftContentPanel;

        //Prefab that will hold the NFT image and allow clicking on it
        public GameObject nftImagePrefab;

        //Reference to the panel that will be populated with the selected NFTs data and displayed.
        public GameObject nftDisplayPanel;

        //Input field where target address is input
        public InputField addressInput;
        public Text nftCount;

        //Text that is displayed when loading
        public GameObject loadingTextObject;

        //List that contains the data returned by cadence after conversion
        public List<CadencePathValue> collectionPaths = new List<CadencePathValue>();

        //Cache of textures downloaded, keyed by url
        public Dictionary<string, Texture> urlTextures = new Dictionary<string, Texture>();

        //Placeholder textures to be displayed while a texture is loading or if a texture is missing
        public Texture loadingTexture;
        public Texture notFoundTexture;

        //The account we're currently looking at
        public string address;

        // Start is called before the first frame update
        void Start()
        {
            addressInput.text = address;
            
            //Connect to Flow Mainnet
            FlowSDK.Init(new FlowConfig
            {
                NetworkUrl = FlowConfig.MAINNETURL,
                Protocol = FlowConfig.NetworkProtocol.HTTP
            });
        }

        //Starts a coroutine to fetch account NFT info
        public void UpdateCollections()
        {
            StartCoroutine(UpdateCollectionsCoroutine());
        }

        //Gets a list of storage paths that implement NonFungibleToken.Collection
        private IEnumerator UpdateCollectionsCoroutine()
        {
            //Clear the NFT count
            nftCount.text = "";

            //Display the loading text
            loadingTextObject.SetActive(true);

            //Get the address from the address input
            address = addressInput.text;

            //Execute the cadence script that gets a list of collection paths
            Task<FlowScriptResponse> scriptResponseTask = Scripts.ExecuteAtLatestBlock(getCollections.text, new[] { new CadenceAddress(address) });

            //Wait until the script returns 
            yield return new WaitUntil(() => scriptResponseTask.IsCompleted);

            //Check for errors in script execution
            if (scriptResponseTask.Result.Error != null)
            {
                Debug.Log(scriptResponseTask.Result.Error.Message);
                Debug.Log(scriptResponseTask.Result.Error.StackTrace);
                yield break;
            }

            //Convert the returned cadence data into our C# classes.  The Cadence returns an array of storage paths ([StoragePath] in cadence),
            //so we'll put it in a List<CadencePathValue>
            collectionPaths = Convert.FromCadence<List<CadencePathValue>>(scriptResponseTask.Result.Value);

            //Clear out any collection buttons currently in the collection panel
            for (int i = 0; i < collectionPanel.transform.childCount; i++)
            {
                Destroy(collectionPanel.transform.GetChild(i).gameObject);
            }

            //Delete all NFT buttons currently being displayed
            for (int i = 0; i < nftContentPanel.transform.childCount; i++)
            {
                Destroy(nftContentPanel.transform.GetChild(i).gameObject);
            }

            //Iterate over each returned Collection, create a button for it, and process the NFTs it contains.
            foreach (CadencePathValue path in collectionPaths)
            {
                //Create the Collection button prefab, making it a child of the panel that should contain the buttons
                GameObject collectionButton = Instantiate(collectionButtonPrefab, collectionPanel.transform);

                //Store the Collection that that button references in the CollectionButton component on that button.
                collectionButton.GetComponent<CollectionButton>().path = path;
            }

            //Disable the loading text
            loadingTextObject.SetActive(false);
        }

        //Downloads an image from a URL and stores it in the texture cache
        public IEnumerator GetTexture(string url)
        {
            //If we already have an entry for this URL, then we've either already downloaded or the download is in progress
            if (urlTextures.ContainsKey(url))
            {
                yield break;
            }

            //If we have a URL, download it and convert it to a texture
            if (url != null)
            {
                //Make a null entry in the dictionary to mark the download as in progress
                urlTextures[url] = null;

                //Download the file
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
                yield return www.SendWebRequest();

                //If there's an error, set the texture to be the not found texture
                if (www.result != UnityWebRequest.Result.Success)
                {
                    urlTextures[url] = notFoundTexture;
                }

                //Create a texture from the downloaded file and save it in the texture cache
                urlTextures[url] = DownloadHandlerTexture.GetContent(www);
            }
        }

        //When a collection button is clicked, display the NFTs contained in that collection
        public void OnCollectionButtonClicked(CadencePathValue path)
        {
            //Clear out any NFTs currently being displayed
            for (int i = 0; i < nftContentPanel.transform.childCount; i++)
            {
                Destroy(nftContentPanel.transform.GetChild(i).gameObject);
            }

            //Start coroutine to fetch a list of NFTs at the desired path
            StartCoroutine(GetNFTsForPathCoroutine(path));
        }

        //Coroutine that gets a list of NFT IDs contained in a collection at a path
        IEnumerator GetNFTsForPathCoroutine(CadencePathValue path)
        {
            //Display loading text
            loadingTextObject.SetActive(true);

            //Execute the cadence script that gets a list of available NFT IDs passing in the Address of the account and the path we're interested in
            Task<FlowScriptResponse> scriptResponseTask = Scripts.ExecuteAtLatestBlock(getNftIdsForCollection.text, new[] { new CadenceAddress(address) as CadenceBase, new CadencePath(path) });

            //Wait until the script returns 
            yield return new WaitUntil(() => scriptResponseTask.IsCompleted);

            //Check for errors in script execution
            if (scriptResponseTask.Result.Error != null)
            {
                Debug.Log(scriptResponseTask.Result.Error.Message);
                Debug.Log(scriptResponseTask.Result.Error.StackTrace);
                yield break;
            }

            //Convert the returned CadenceBase into a List of UInt64
            List<UInt64> nftIDs = Convert.FromCadence<List<UInt64>>(scriptResponseTask.Result.Value);

            //Fetch display data for each of the NFT IDs we fetched previously
            scriptResponseTask = Scripts.ExecuteAtLatestBlock(getDisplayDataForIDs.text, new[] { new CadenceAddress(address) as CadenceBase, new CadencePath(path), Convert.ToCadence(nftIDs, "[UInt64]") });

            //Wait until the script returns 
            yield return new WaitUntil(() => scriptResponseTask.IsCompleted);

            //Check for errors in script execution
            if (scriptResponseTask.Result.Error != null)
            {
                Debug.Log(scriptResponseTask.Result.Error.Message);
                Debug.Log(scriptResponseTask.Result.Error.StackTrace);
                yield break;
            }

            //Convert the returned CadenceBase into a Dictionary<UInt64, Display> object
            Dictionary<UInt64, Display> displayData = Convert.FromCadence<Dictionary<UInt64, Display>>(scriptResponseTask.Result.Value);

            //Iterate through nft display data and create NFTButtons for each
            foreach (KeyValuePair<ulong, Display> display in displayData)
            {
                //Try and use Display.thumbnail.url as the url to download from
                string url = display.Value.thumbnail?.GetURL();

                GameObject nftImageGameObject = Instantiate(nftImagePrefab, nftContentPanel.transform);
                NFTButton nftButton = nftImageGameObject.GetComponent<NFTButton>();
                nftButton.path = path;
                nftButton.nftID = display.Key;
                nftButton.url = url;

                //Start the coroutine to download the image as a texture
                StartCoroutine(GetTexture(url));
            }

            //Disable the loading text
            loadingTextObject.SetActive(false);
            nftCount.text = $"{displayData.Count} NFTs";
        }

        //Handler for when an NFT button is clicked
        public void OnNFTButtonClicked(NFTButton nft)
        {
            StartCoroutine(DisplayNFTCoroutine(nft));
        }

        //When an NFT is clicked, display more information about that NFT
        public IEnumerator DisplayNFTCoroutine(NFTButton nft)
        {
            //Execute the cadence script that gathers data, passing in the Address of the account we're interested in, the path of the collection and the ID of the NFT we're interested in 
            Task<FlowScriptResponse> scriptResponseTask = Scripts.ExecuteAtLatestBlock(getFullDataForID.text, new[] { new CadenceAddress(address) as CadenceBase, new CadencePath(nft.path), Convert.ToCadence(nft.nftID, "UInt64") });

            //Wait until the script returns 
            yield return new WaitUntil(() => scriptResponseTask.IsCompleted);

            //Check for errors in script execution
            if (scriptResponseTask.Result.Error != null)
            {
                Debug.Log(scriptResponseTask.Result.Error.Message);
                Debug.Log(scriptResponseTask.Result.Error.StackTrace);
                yield break;
            }

            //Convert the returned CadenceBase into an NFT object
            NFT nftData = Convert.FromCadence<NFT>(scriptResponseTask.Result.Value);

            //Enable the panel that displays NFT details
            nftDisplayPanel.SetActive(true);
            NFTDisplay nftDisplay = nftDisplayPanel.GetComponent<NFTDisplay>();

            //Initialize the panel by passing in the NFT information
            nftDisplay.Init(nftData);
        }
    }

//Classes to make accessing cadence data returned from chain easier

    public class File
    {
        public string url;
        public string cid;
        public string path;

        public string GetURL()
        {
            if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(cid))
            {
                return $"https://ipfs.io/ipfs/{cid}";
            }

            return url;
        }
    }

    public class Display
    {
        public String name;
        public String description;
        public File thumbnail;
    }

    public class ExternalURL
    {
        public String url;
    }

    public class Media
    {
        public String mediaType;
        public File file;
    }

    public class Medias
    {
        public List<Media> items;
    }

    public class HTTPFile
    {
        public String url;
    }

    public class NFTCollectionDisplay
    {
        public String name;
        public String description;
        public ExternalURL externalURL;
        public Media squareImage;
        public Media bannerImage;
        public Dictionary<String, ExternalURL> socials;
    }

    public class Royalty
    {
        public Decimal cut;
        public String description;
        public CadenceCapabilityValue receiver;
    }

    public class Royalties
    {
        public List<Royalty> cutInfos;
    }

    public class Rarity
    {
        public Decimal? score;
        public Decimal? max;
        public String description;
    }

    public class Trait
    {
        public String name;
        public String displayType;
        public Rarity rarity;
    }

    public class Traits
    {
        public List<Trait> traits;
    }

    public class NFTView
    {
        public UInt64 id;
        public UInt64 uuid;
        public Display display;
        public ExternalURL externalURL;
        public NFTCollectionDisplay collectionDisplay;
        public Royalties royalties;
        public Traits traits;
    }

    public class IPFSFile
    {
        public string cid;
        public string path;
    }

    public class Edition
    {
        public String name;
        public UInt64 number;
        public UInt64? max;
    }

    public class Editions
    {
        public List<Edition> infoList;
    }

    public class Serial
    {
        public UInt64 number;
    }

    public class License
    {
        public string spdxIdentifier;
    }

    public class NFT
    {
        public NFTView NFTView;
        public Display Display;
        public HTTPFile HTTPFile;
        public IPFSFile IPFSFile;
        public Edition Edition;
        public Editions Editions;
        public Serial Serial;
        public Royalty Royalty;
        public Royalties Royalties;
        public Media Media;
        public Medias Medias;
        public License License;
        public ExternalURL ExternalURL;
        public NFTCollectionDisplay NFTCollectionDisplay;
        public Rarity Rarity;
        public Trait Trait;
        public Traits Traits;

        [NonSerialized] public Texture NFTTexture;
    }
}