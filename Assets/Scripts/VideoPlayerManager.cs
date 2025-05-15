using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerManager : MonoBehaviour
{
    [Header("Video Components")]
    public VideoPlayer videoPlayer;
    public RawImage rawImage;

    [Header("Mask (Optional)")]
    public MaskController maskController;

    [Header("Video Clips (1번→index=0, 2번→index=1, ...)")]
    public VideoClip[] videoClips;

    private void Start()
    {
        maskController.LoadAndApplyMask();
    }

    /// <summary>
    /// 기본 재생 (스케줄용)
    /// </summary>
    public void StartVideo()
    {
        if (maskController != null)
            maskController.LoadAndApplyMask();

        StartCoroutine(PlayVideoCoroutine());
    }
    /// <summary>
    /// 지정 인덱스(1,2,3) 영상 즉시 재생
    /// </summary>
    
    public void PlayVideoClip(int index)
    {
        if (index < 1 || index > videoClips.Length)
        {
            Debug.LogWarning($"[VideoPlayerManager] 잘못된 인덱스: {index}");
            return;
        }

        videoPlayer.clip = videoClips[index - 1];
        StartVideo();
    }

    private IEnumerator PlayVideoCoroutine()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        rawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
        videoPlayer.isLooping = true;
        Debug.Log($"[VideoPlayerManager] 재생: {videoPlayer.clip.name} ({DateTime.Now:HH:mm:ss})");
    }

    /// <summary>
    /// 지정 시각에 맞춰 재생 예약
    /// </summary>
    public void ScheduleVideo(DateTime startTime)
    {
        float delay = (float)(startTime - DateTime.Now).TotalSeconds;
        if (delay < 0f) delay = 0f;
        StartCoroutine(DelayedStart(delay));
        Debug.Log("[VideoPlayerManager] 재생 예약: " + delay + "초 후");
    }

    private IEnumerator DelayedStart(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StartVideo();
    }
}
