using UnityEngine;

public class CursorDisabler : MonoBehaviour
{
    void Start()
    {
        // Ŀ�� �����
        Cursor.visible = false;
        // Ŀ���� ȭ�� �߾ӿ� �����ϰ� �������� ���ϰ� ��
        //Cursor.lockState = CursorLockMode.Locked;
    }
}
