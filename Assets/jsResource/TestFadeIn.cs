using UnityEngine;
using System.Collections;

public class TestFadeIn : MonoBehaviour
{
    public float initialDelay = 2.0f; // 초기 지연 시간 (초)
    public int blinkCount = 2; // 깜빡이는 횟수
    public float blinkInterval = 0.5f; // 깜빡임 간격 (초)
    private Renderer objRenderer;

    void Start()
    {
        // Renderer 컴포넌트 가져오기
        objRenderer = GetComponent<Renderer>();
        
        // 코루틴 시작
        StartCoroutine(BlinkAndTurnOn(initialDelay, blinkCount, blinkInterval));
    }

    IEnumerator BlinkAndTurnOn(float delay, int blinks, float interval)
    {
        // 초기 지연 시간 동안 대기
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < blinks; i++)
        {
            // 오브젝트 깜빡이기 (비활성화)
            objRenderer.enabled = false;
            yield return new WaitForSeconds(interval);

            // 오브젝트 깜빡이기 (활성화)
            objRenderer.enabled = true;
            yield return new WaitForSeconds(interval);
        }

        // 최종적으로 오브젝트를 활성화 상태로 설정
        objRenderer.enabled = true;
    }
}
