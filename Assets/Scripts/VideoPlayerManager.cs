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

    [Header("Video Clips (1→index0, 2→1, …)")]
    public VideoClip[] videoClips;

    private IEnumerator PlayVideoCoroutine()
    {
        // 1) 비디오 준비
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        // 2) RenderTexture 생성 및 할당 (원본 해상도 유지)
        int w = (int)videoPlayer.clip.width;
        int h = (int)videoPlayer.clip.height;

        if (videoPlayer.targetTexture != null)
        {
            var old = videoPlayer.targetTexture;
            videoPlayer.targetTexture = null;
            old.Release();
            Destroy(old);
        }

        var rt = new RenderTexture(w, h, 0);
        videoPlayer.targetTexture = rt;
        rawImage.texture = rt;

        // 3) RawImage를 화면 전체로 스트레치
        var rect = rawImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 4) 재생
        videoPlayer.Play();
        videoPlayer.isLooping = true;
        Debug.Log($"[VideoPlayerManager] Play ▶ {videoPlayer.clip.name} ({w}×{h}), fullscreen stretch");
    }

    void Update()
    {
        // 숫자 키 입력으로 즉시 재생
        if (Input.GetKeyDown(KeyCode.Alpha1))
            PlayVideoClip(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            PlayVideoClip(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            PlayVideoClip(3);
    }

    public void PlayVideoClip(int index)
    {
        if (videoClips == null || index < 1 || index > videoClips.Length)
        {
            Debug.LogWarning($"[VideoPlayerManager] 잘못된 인덱스: {index}");
            return;
        }
        videoPlayer.clip = videoClips[index - 1];
        StopAllCoroutines();
        StartCoroutine(PlayVideoCoroutine());
    }

    public void StartVideo() => PlayVideoClip(1);

    public void ScheduleVideo(DateTime startTime)
    {
        float delay = (float)(startTime - DateTime.Now).TotalSeconds;
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
