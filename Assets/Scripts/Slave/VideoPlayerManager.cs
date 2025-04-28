using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public class VideoPlayerManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public MaskController maskController;


    public void StartVideo()
    {
        if (maskController != null)
            maskController.LoadAndApplyMask();

        StartCoroutine(PlayVideo());
    }

    IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        rawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
        videoPlayer.isLooping = true;
    }

}
