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
        // 마스크를 넣을 RectTransform에 적용하기
        rect.anchorMin = new Vector2(0, 1); // 좌측 상단 기준
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(box.x, -box.y); // Y축 반전 필요
        rect.sizeDelta = new Vector2(box.width, box.height);

        // 마스크 빌드 한 뒤에 사이즈 바꾸는 것 적용하기....전체 화면에서 
        PlayerPrefs.SetFloat("MaskX" + rect.name, box.x);
        PlayerPrefs.SetFloat("MaskY" + rect.name, -box.y); // Y축 반전 필요
    }

    // 마스크 사이즈 적용하기 

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
