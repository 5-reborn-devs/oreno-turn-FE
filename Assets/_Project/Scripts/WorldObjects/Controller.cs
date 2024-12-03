using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;

public class Controller : FSMController<ControlState, ControlFSM, BaseDataSO>
{

    private void Start()
    {
        fsm = new ControlFSM(CreateState<ControlDayState>());
        Init(null);
    }

    public override void Init(BaseDataSO data)
    {
        StartCoroutine(Click());
    }
    public IEnumerator Click()
    {
        Debug.Log("Click");
        while (true)
        {
            yield return new WaitUntil(() => Input.GetMouseButtonUp(0)); // 마우스 좌클릭 빵야 타겟 선정 
            var mousePosition = Input.mousePosition;
            Ray ray = GameManager.instance.mainCamera.ScreenPointToRay(mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20, Color.red, 5f);
            var hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit)
            {
                currentState.OnClickScreen(hit);
            }
            yield return new WaitUntil(() => !Input.GetMouseButtonUp(0));
        }
    }

    //private void Update()
    //{
    //    if (UIGame.instance != null && UIGame.instance.stick.OnHandleChanged == null)
    //    {
    //        var dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
    //        Debug.Log("나움직였어!!" + dir);
    //        GameManager.instance.userCharacter?.MoveCharacter(dir);
    //    }
    //    fsm.UpdateState();
    //}

    private void Update()
    {
        if (Input.GetMouseButtonUp(1)) // 마우스 우측을 눌렀을 때
        {
            Vector3 mousePos = Input.mousePosition; // 클릭한 마우스 위치 값 3D로 가져옴 (스크린 좌표) 
            Vector3 transPos = GameManager.instance.mainCamera.ScreenToWorldPoint(mousePos); // 월드 좌표로 변환
            transPos.z = 0; // 2D 게임이므로 z 값은 0으로 설정

            Vector2 Vecotr2tranPos = new Vector2(transPos.x, transPos.y); // 내 목표위치를 Vector2형태로 재 가공
            GameManager.instance.userCharacter?.MoveCharacter(Vecotr2tranPos);
        }
        fsm.UpdateState();
    }
}
