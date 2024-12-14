using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;
        Debug.Log("Original Position: " + originalPosition); // 디버그 로그 추가

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPosition.z);
            Debug.Log("Shaking: x=" + x + ", y=" + y); // 디버그 로그 추가

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
        Debug.Log("Position Reset: " + originalPosition); // 디버그 로그 추가
    }
}
