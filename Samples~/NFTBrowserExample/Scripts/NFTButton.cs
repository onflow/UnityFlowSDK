using System;
using System.Collections;
using DapperLabs.Flow.Sdk.Cadence;
using UnityEngine;
using UnityEngine.UI;

namespace NFTViewerExample
{
    public class NFTButton : MonoBehaviour
    {
        //The path of the storage that contains this NFT
        public CadencePathValue path;

        //The ID of the NFT
        public UInt64 nftID;

        //The url of the thumbnail to use
        public string url;

        //Notifies the NFTViewer that this NFT has been clicked
        public void OnClicked()
        {
            FindObjectOfType<NFTViewer>().OnNFTButtonClicked(this);
        }

        IEnumerator Start()
        {
            //Sets initial texture to the loading texture
            NFTViewer nftViewer = FindObjectOfType<NFTViewer>();
            GetComponent<RawImage>().texture = nftViewer.loadingTexture;

            //Waits until the texture has been downloaded
            yield return new WaitUntil(() => nftViewer.urlTextures.ContainsKey(url) && nftViewer.urlTextures[url] != null);

            //Updates the display with the downloaded texture
            GetComponent<RawImage>().texture = nftViewer.urlTextures[url];
        }
    }
}