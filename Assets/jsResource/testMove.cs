using UnityEngine;
using System.Collections;

public class MoveObject : MonoBehaviour
{
    public Vector3 startPoint = new Vector3(-2100, -1209, 0); // 시작 위치
    public Vector3 endPoint = new Vector3(33, 23, 0); // 끝 위치
    public float duration = 3.0f; // 이동하는 데 걸리는 시간 (초)

    void Start()
    {
        // 코루틴 시작
        StartCoroutine(MoveOverTime(startPoint, endPoint, duration));
    }

    IEnumerator MoveOverTime(Vector3 start, Vector3 end, float time)
    {
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            // 위치 보간
            transform.position = Vector3.Lerp(start, end, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 최종 위치로 이동
        transform.position = end;
    }
}
