using UnityEngine;

[System.Serializable]
public class MaskBox
{
    public float x;        // ���� ���� X
    public float y;        // ��� ���� Y
    public float width;
    public float height;
}

[System.Serializable]
public class MaskData
{
    public MaskBox[] masks;
}
