using UnityEngine;
using System.Collections;

public class UIShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 originalPosition = rectTransform.localPosition;

        float elapsed = 0.0f;
        Debug.Log("Shake started"); // 디버그 로그 추가

        while (elapsed < duration)
        {
            float x = Random.Range(-0.01f, 0.01f) * magnitude;
            float y = Random.Range(-0.01f, 0.01f) * magnitude;

            rectTransform.localPosition = new Vector3(x, y, originalPosition.z);
            elapsed += Time.deltaTime;

            yield return null;
        }

        rectTransform.localPosition = originalPosition;
    }
}

