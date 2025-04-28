using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MaskController : MonoBehaviour
{
    public RectTransform[] maskRects; // ����ũ ������Ʈ 2��
    public string jsonFileName = "mask_config.json";

    public void LoadAndApplyMask()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(LoadAndroidJson(path));
#else
        if (!File.Exists(path))
        {
            Debug.LogError("����ũ ���� ���� ����: " + path);
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
        rect.anchorMin = new Vector2(0, 1); // ���� ��� ����
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(box.x, -box.y); // Y�� ���� �ʿ�
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
                Debug.LogError("Android JSON �ε� ����: " + www.error);
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
