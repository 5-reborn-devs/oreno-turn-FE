using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWalkState : CharacterState
{
    float syncFrame = 0;
    public override void OnStateEnter()
    {
        if (anim != null)
        {
            anim.ChangeAnimation("walk");
        }
    }

    public override void OnStateExit()
    {
        anim.ChangeAnimation("idle");
    }

    public override void OnStateUpdate()
    {
        // ������Ʈ 
        if (anim != null && !anim.IsAnim("walk"))
        {
            anim.ChangeAnimation("walk");
        }
        Vector2 currentPos = character.transform.position; // �� ���� ��ġ
        Vector2 targetPos = character.targetPosition; // ���� ���� ��ǥ
        float distance = (targetPos - currentPos).magnitude; // �ѻ����� �Ÿ��� ���ϴ� magnitude (Vector2 �Լ�)
        if(distance < 0.1f) // �Ÿ��� ��������� dir(����)�� 0���� �ʱ�ȭ ���Ѽ� �������� �ʰ� �ϰ� anim�� idle�� change          
        {
            character.dir = Vector2.zero; // ���Ⱚ�� ���� ������ ���ּ� 
            if(anim != null && !anim.IsAnim("idle"))
            {
                anim.ChangeAnimation("idle");
            }
        }
        rigidbody.linearVelocity = character.dir * character.Speed;
        if (SocketManager.instance.isConnected && character.dir != Vector2.zero)
        {
            syncFrame++;
            if (syncFrame > 3)
            {
                GamePacket packet = new GamePacket();
                packet.PositionUpdateRequest = new C2SPositionUpdateRequest() { X = character.transform.position.x, Y = character.transform.position.y };
                SocketManager.instance.Send(packet);
                syncFrame = 0;
            }
        }
    }
}
