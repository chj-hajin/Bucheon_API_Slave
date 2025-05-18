using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MaskController: PlayerPrefs로 마스크 위치/크기 저장 및 편집 모드 지원
/// </summary>
public class MaskController : MonoBehaviour
{
    [Header("Mask Settings")]
    public RectTransform[] maskRects;

    [Header("Edit Mode Settings")]
    public float moveStep = 1f;   // WASD 이동 단위
    public float sizeStep = 1f;   // ZX,CV 크기 조절 단위

    private bool isEditMode = false;
    private int selectedMaskIndex = -1;

    void Awake()
    {
        // Apply saved mask settings on startup
        ApplyAllMasks();
    }

    void Update()
    {
        // Toggle edit mode: T
        if (Input.GetKeyDown(KeyCode.T))
        {
            isEditMode = !isEditMode;
            selectedMaskIndex = -1;
            Debug.Log($"[Mask] EditMode {(isEditMode ? "ON" : "OFF")} ");
        }
        if (!isEditMode)
            return;

        // Exit edit mode: 0
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            isEditMode = false;
            selectedMaskIndex = -1;
            SaveAllMaskPrefs();
            Debug.Log("[Mask] EditMode OFF");
            return;
        }

        // Select mask by number keys 1-9
        for (int i = 0; i < maskRects.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectMask(i);
        }

        // Handle edit input for selected mask
        if (selectedMaskIndex >= 0)
            HandleEditInput(maskRects[selectedMaskIndex]);
    }

    void SelectMask(int idx)
    {
        selectedMaskIndex = idx;
        Debug.Log($"[Mask] Selected Mask #{idx + 1}");
    }

    void HandleEditInput(RectTransform rect)
    {
        bool changed = false;
        Vector2 pos = rect.anchoredPosition;
        Vector2 size = rect.sizeDelta;

        // Move: WASD (single-step)
        if (Input.GetKeyDown(KeyCode.W)) { pos.y += moveStep; changed = true; }
        if (Input.GetKeyDown(KeyCode.S)) { pos.y -= moveStep; changed = true; }
        if (Input.GetKeyDown(KeyCode.A)) { pos.x -= moveStep; changed = true; }
        if (Input.GetKeyDown(KeyCode.D)) { pos.x += moveStep; changed = true; }

        // Height: Z/X
        if (Input.GetKeyDown(KeyCode.Z)) { size.y += sizeStep; changed = true; }
        if (Input.GetKeyDown(KeyCode.X)) { size.y -= sizeStep; changed = true; }

        // Width: C/V
        if (Input.GetKeyDown(KeyCode.C)) { size.x += sizeStep; changed = true; }
        if (Input.GetKeyDown(KeyCode.V)) { size.x -= sizeStep; changed = true; }

        if (changed)
        {
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            SaveMaskPrefs(rect);
            Debug.Log($"[Mask] Updated Mask #{selectedMaskIndex + 1} Pos={pos}, Size={size}");
        }
    }

    void SaveMaskPrefs(RectTransform rect)
    {
        string key = rect.name;
        PlayerPrefs.SetFloat($"MaskX_{key}", rect.anchoredPosition.x);
        PlayerPrefs.SetFloat($"MaskY_{key}", rect.anchoredPosition.y);
        PlayerPrefs.SetFloat($"MaskW_{key}", rect.sizeDelta.x);
        PlayerPrefs.SetFloat($"MaskH_{key}", rect.sizeDelta.y);
    }

    /// <summary>
    /// Save all masks to PlayerPrefs
    /// </summary>
    void SaveAllMaskPrefs()
    {
        foreach (var rect in maskRects)
            SaveMaskPrefs(rect);
        PlayerPrefs.Save();
    }

    void ApplyAllMasks()
    {
        foreach (var rect in maskRects)
        {
            string key = rect.name;
            float x = PlayerPrefs.GetFloat($"MaskX_{key}", rect.anchoredPosition.x);
            float y = PlayerPrefs.GetFloat($"MaskY_{key}", rect.anchoredPosition.y);
            float w = PlayerPrefs.GetFloat($"MaskW_{key}", rect.sizeDelta.x);
            float h = PlayerPrefs.GetFloat($"MaskH_{key}", rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }
    }
}
