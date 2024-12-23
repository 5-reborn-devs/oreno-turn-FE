using UnityEngine;

public class ExampleUsageTransform : MonoBehaviour
{
    public CameraShakeByTransform cameraShake;

    void Start()
    {
        if (cameraShake == null)
        {
            cameraShake = GameObject.Find("CameraParent").GetComponent<CameraShakeByTransform>();
        }
        if (cameraShake == null)
        {
            Debug.LogError("CameraShakeByTransform script not found on CameraParent object.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed."); // 디버그 로그 추가
            if (cameraShake != null)
            {
                StartCoroutine(cameraShake.Shake(0.5f, 0.1f)); // 0.5초 동안 magnitude 0.1로 쉐이크
            }
        }
    }
}
