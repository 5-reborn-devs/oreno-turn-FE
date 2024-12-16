using UnityEngine;

public class MoveUpAndDown : MonoBehaviour
{
    public float speed = 1.0f; // 움직이는 속도
    public float height = 1.0f; // 움직이는 높이
    //private bool isActive = false;

    private Vector3 startPosition;

    void Start()
    {
        //gameObject.SetActive(false); // 시작 시 비활성화
        // 오브젝트의 시작 위치를 저장합니다.
        startPosition = transform.position;
    }

    void Update()
    {   
        //if (isActive){
        // 오브젝트의 Y 좌표를 주기적으로 변경합니다.
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * height;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    //}
    }

    public void SetPhase(PhaseType phase) { 
        if (phase == PhaseType.End) { gameObject.SetActive(true); 
        } else { gameObject.SetActive(false); } 
    } 
    
    // private void SetActiveState(bool state) { 
    //     isActive = state; gameObject.SetActive(state); 
    // }


}
