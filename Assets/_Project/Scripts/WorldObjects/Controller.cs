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
            yield return new WaitUntil(() => Input.GetMouseButtonUp(0)); // ���콺 ��Ŭ�� ���� Ÿ�� ���� 
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
    //        Debug.Log("����������!!" + dir);
    //        GameManager.instance.userCharacter?.MoveCharacter(dir);
    //    }
    //    fsm.UpdateState();
    //}

    private void Update()
    {
        if (Input.GetMouseButtonUp(1)) // ���콺 ������ ������ ��
        {
            Vector3 mousePos = Input.mousePosition; // Ŭ���� ���콺 ��ġ �� 3D�� ������ (��ũ�� ��ǥ) 
            Vector3 transPos = GameManager.instance.mainCamera.ScreenToWorldPoint(mousePos); // ���� ��ǥ�� ��ȯ
            transPos.z = 0; // 2D �����̹Ƿ� z ���� 0���� ����

            Vector2 Vecotr2tranPos = new Vector2(transPos.x, transPos.y); // �� ��ǥ��ġ�� Vector2���·� �� ����
            GameManager.instance.userCharacter?.MoveCharacter(Vecotr2tranPos);
        }
        fsm.UpdateState();
    }
}
