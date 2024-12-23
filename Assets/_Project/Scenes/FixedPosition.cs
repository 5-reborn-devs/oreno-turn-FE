using UnityEngine;
public class FixedPosition : MonoBehaviour
{
    private Vector3 initialLocalPosition;
    void Start()
    {
        // 초기 로컬 위치 저장
        initialLocalPosition = transform.localPosition;
    }
    void Update()
    {
        // 로컬 위치를 초기 위치로 고정
        transform.localPosition = initialLocalPosition;
    }
}