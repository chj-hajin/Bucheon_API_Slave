using UnityEngine;

public class CursorDisabler : MonoBehaviour
{
    void Start()
    {
        // 커서 숨기기
        Cursor.visible = false;
        // 커서를 화면 중앙에 고정하고 움직이지 못하게 함
        //Cursor.lockState = CursorLockMode.Locked;
    }
}
