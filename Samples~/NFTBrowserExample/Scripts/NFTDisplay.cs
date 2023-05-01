using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NFTViewerExample
{
    public class NFTDisplay : MonoBehaviour
    {
        //The NFT we're currently displaying
        public NFT nft;

        //The render texture we'll use to display video content
        public RenderTexture videoRenderTexture;

        //The VideoPlayer used to play back videos
        public VideoPlayer videoPlayer;

        //The RawImage that displays images and videos
        public RawImage image;

        //The index of the Media file we are currently displaying
        public int index = 0;

        //References to the previous and next buttons
        public Button previous;
        public Button next;

        public Text mediaCount;

        //Configures this display panel to show information for the passed in NFT
        public void Init(NFT nftData)
        {
            //Save the nft to a variable for access by other functions
            nft = nftData;

            //Set the text to the NFT's data
            GetComponentInChildren<Text>().text = $"<b>Name:</b> {nftData.Display?.name}\n\n<b>Description:</b> {nftData.Display?.description}\n\n<b>Serial:</b> {nftData.Serial?.number}\n\n<b>External URL:</b> {nftData.ExternalURL?.url}";

            //Set the index to 0
            index = 0;

            //Initialize the media count
            mediaCount.text = $"{index + 1}/{nft.Medias.items.Count}";

            //See if there are media files to display.  If not, we'll use the NFT Display thumbnail and disable the next and previous buttons
            if (nft.Medias?.items == null || nft.Medias.items.Count == 0)
            {
                previous.gameObject.SetActive(false);
                next.gameObject.SetActive(false);
                image.texture = FindObjectOfType<NFTViewer>().urlTextures[nft.Display.thumbnail.GetURL()];
                return;
            }

            //If there's only a single media file, we'll disable the navigation buttons and display that file
            if (nft.Medias.items.Count == 1)
            {
                previous.gameObject.SetActive(false);
                next.gameObject.SetActive(false);
                UpdateImageTexture();
                return;
            }

            //If we're here, then there are multiple media files.  Enable navigation buttons and display the first media
            previous.gameObject.SetActive(true);
            next.gameObject.SetActive(true);
            UpdateImageTexture();

            //Get a reference to the NFTViewer component in the scene
            NFTViewer nftViewer = FindObjectOfType<NFTViewer>();

            //Start preloading all images
            foreach (Media mediasItem in nft.Medias.items)
            {
                //Only preload images, videos will be streamed
                if (mediasItem.mediaType.Contains("image"))
                {
                    StartCoroutine(nftViewer.GetTexture(mediasItem.file.GetURL()));
                }
            }

        }

        //Updates the Image texture that is displayed
        public void UpdateImageTexture()
        {
            //Get the Media at the selected index
            Media media = nft?.Medias?.items[index];

            //If there's no media, just return
            if (media == null)
            {
                return;
            }

            //See if this media is a video file
            if (media.mediaType.Contains("video"))
            {
                //Clear the render texture to black so it doesn't show a frame of a previously displayed video
                RenderTexture originalRenderTexture = RenderTexture.active;
                RenderTexture.active = videoRenderTexture;
                GL.Clear(true, true, new Color(0, 0, 0, 1));
                RenderTexture.active = originalRenderTexture;

                //Set the image to the video render texture
                image.texture = videoRenderTexture;

                //Set the URL that the video player should use
                videoPlayer.url = media.file.GetURL();

                //Start playing the video
                videoPlayer.Play();
                return;
            }

            //If this is an image rather than a video, display it
            if (media.mediaType.Contains("image"))
            {
                StartCoroutine(UpdateImageCoroutine(media.file.GetURL()));
            }
        }

        //Coroutine that displays an image texture for a URL.
        IEnumerator UpdateImageCoroutine(string url)
        {
            NFTViewer nftViewer = FindObjectOfType<NFTViewer>();

            //Display the loading texture until the download is complete
            image.texture = nftViewer.loadingTexture;

            //Start downloading the texture
            StartCoroutine(nftViewer.GetTexture(url));

            //Wait until the texture is downloaded
            yield return new WaitUntil(() => nftViewer.urlTextures.ContainsKey(url) && nftViewer.urlTextures[url] != null);

            //Set the RawImage texture to the texture associated with the URL
            image.texture = nftViewer.urlTextures[url];
        }

        //Switches to displaying the next media file
        public void ShowNextMedia()
        {
            //If there's a video playing, stop it
            StopVideo();
            //Increment the current index, wrapping back to 0 if we're at the end already
            index = (index + 1) % nft.Medias.items.Count;
            //Update the image/video texture
            UpdateImageTexture();
            mediaCount.text = $"{index + 1}/{nft.Medias.items.Count}";
        }

        public void ShowPreviousMedia()
        {
            //If there's a video playing, stop it
            StopVideo();
            //Decrement the media index, wrapping to the end if we're at the beginning 
            index = index - 1;
            if (index < 0)
            {
                index = nft.Medias.items.Count - 1;
            }

            //Update image/video texture
            UpdateImageTexture();
            mediaCount.text = $"{index + 1}/{nft.Medias.items.Count}";
        }

        //Stop any playing video
        public void StopVideo()
        {
            videoPlayer.Stop();
        }
    }
}