using UnityEngine;

public class ExampleUsageUI : MonoBehaviour
{
    public UIShake uiShake;

    void Start()
    {
        if (uiShake == null)
        {
            uiShake = GetComponent<UIShake>();
        }
        if (uiShake == null)
        {
            Debug.LogError("UIShake script not found on the UI object.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed."); // 디버그 로그 추가
            if (uiShake != null)
            {
                StartCoroutine(uiShake.Shake(0.5f, 0.1f)); // 0.5초 동안 magnitude 0.1로 쉐이크
            }
        }
    }
}
