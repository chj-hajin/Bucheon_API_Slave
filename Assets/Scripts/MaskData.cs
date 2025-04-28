using UnityEngine;

[System.Serializable]
public class MaskBox
{
    public float x;        // 좌측 기준 X
    public float y;        // 상단 기준 Y
    public float width;
    public float height;
}

[System.Serializable]
public class MaskData
{
    public MaskBox[] masks;
}
