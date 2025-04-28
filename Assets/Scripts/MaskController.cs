using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MaskController : MonoBehaviour
{
    public RectTransform[] maskRects; // 마스크 오브젝트 2개
    public string jsonFileName = "mask_config.json";

    public void LoadAndApplyMask()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(LoadAndroidJson(path));
#else
        if (!File.Exists(path))
        {
            Debug.LogError("마스크 설정 파일 없음: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        MaskData maskData = JsonUtility.FromJson<MaskData>(json);

        for (int i = 0; i < maskRects.Length && i < maskData.masks.Length; i++)
        {
            ApplyMask(maskRects[i], maskData.masks[i]);
        }
#endif
    }

    void ApplyMask(RectTransform rect, MaskBox box)
    {
        rect.anchorMin = new Vector2(0, 1); // 좌측 상단 기준
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(box.x, -box.y); // Y축 반전 필요
        rect.sizeDelta = new Vector2(box.width, box.height);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    IEnumerator LoadAndroidJson(string path)
    {
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Android JSON 로드 실패: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            MaskData maskData = JsonUtility.FromJson<MaskData>(json);

            for (int i = 0; i < maskRects.Length && i < maskData.masks.Length; i++)
            {
                ApplyMask(maskRects[i], maskData.masks[i]);
            }
        }
    }
#endif
}
