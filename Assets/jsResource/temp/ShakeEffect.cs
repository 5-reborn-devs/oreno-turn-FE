using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    public Material shakeMaterial;
    private float shakeAmount = 0.0f;
    private float shakeDuration = 0.0f;
    private bool isShaking = false;

    void Start()
    {
        GetComponent<Camera>().SetReplacementShader(shakeMaterial.shader, null);
    }

    void Update()
    {
        if (isShaking)
        {
            shakeDuration -= Time.deltaTime;
            if (shakeDuration <= 0.0f)
            {
                isShaking = false;
                shakeAmount = 0.0f;
                GetComponent<Camera>().ResetReplacementShader();
                Debug.Log("Shake effect ended."); // 디버그 로그 추가
            }
            shakeMaterial.SetFloat("_Amount", shakeAmount);
        }
    }

    public void Shake(float duration, float amount)
    {
        shakeDuration = duration;
        shakeAmount = amount;
        isShaking = true;
        Debug.Log("Shake effect started."); // 디버그 로그 추가
    }
}
