using UnityEngine;

public class cycleRotate : MonoBehaviour
{
    public float rotationSpeed = 100f; // 초당 회전 속도 (단위: 도/초)

    // Update is called once per frame
    void Update()
    {
        // Z축을 기준으로 오브젝트를 회전시킴
		transform.Rotate(Vector3.forward * -rotationSpeed * Time.deltaTime);
    }
}
