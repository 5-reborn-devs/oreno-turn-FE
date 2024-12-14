using UnityEngine;
using System.Collections;

public class spaceShake : MonoBehaviour
{
    public CameraShake cameraShake;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed, starting shake.");
            StartCoroutine(cameraShake.Shake(0.5f, 0.1f)); // 0.5초 동안 magnitude 0.3으로 쉐이크
        }
    }
}
