using UnityEngine;
public class FixedPosition : MonoBehaviour
{
    private Vector3 initialLocalPosition;
    void Start()
    {
        // �ʱ� ���� ��ġ ����
        initialLocalPosition = transform.localPosition;
    }
    void Update()
    {
        // ���� ��ġ�� �ʱ� ��ġ�� ����
        transform.localPosition = initialLocalPosition;
    }
}