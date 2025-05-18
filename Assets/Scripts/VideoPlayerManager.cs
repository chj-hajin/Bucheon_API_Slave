using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerManager : MonoBehaviour
{
    [Header("Video Components")]
    public VideoPlayer videoPlayer;
    public RawImage rawImage;

    [Header("Video Clips (1→index0, 2→1, …)")]
    public VideoClip[] videoClips;

    private IEnumerator PlayVideoCoroutine()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        rawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
        videoPlayer.isLooping = true;
        Debug.Log($"[VideoPlayerManager] Play ▶ {videoPlayer.clip?.name}");
    }

    public void PlayVideoClip(int index)
    {
        if (videoClips == null || index < 1 || index > videoClips.Length)
        {
            Debug.LogWarning($"[VideoPlayerManager] 잘못된 인덱스: {index}");
            return;
        }
        videoPlayer.clip = videoClips[index - 1];
        Debug.Log($"[VideoPlayerManager] Assigned clip: {videoPlayer.clip.name}");
        StopAllCoroutines();
        StartCoroutine(PlayVideoCoroutine());
    }

    /// <summary>
    /// 기본 첫 번째 영상 재생
    /// </summary>
    public void StartVideo()
    {
        PlayVideoClip(1);
    }

    /// <summary>
    /// 지정 시각에 재생 예약
    /// </summary>
    public void ScheduleVideo(System.DateTime startTime)
    {
        float delay = (float)(startTime - System.DateTime.Now).TotalSeconds;
        if (delay < 0) delay = 0;
        StartCoroutine(DelayedStart(delay));
        Debug.Log($"[VideoPlayerManager] scheduled in {delay} seconds");
    }

    private IEnumerator DelayedStart(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StartVideo();
    }
}
