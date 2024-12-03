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
        // 업데이트 
        if (anim != null && !anim.IsAnim("walk"))
        {
            anim.ChangeAnimation("walk");
        }
        Vector2 currentPos = character.transform.position; // 내 현재 위치
        Vector2 targetPos = character.targetPosition; // 내가 찍은 좌표
        float distance = (targetPos - currentPos).magnitude; // 둘사이의 거리를 구하는 magnitude (Vector2 함수)
        if(distance < 0.1f) // 거리가 가까워지면 dir(방향)을 0으로 초기화 시켜서 움직이지 않게 하고 anim을 idle로 change          
        {
            character.dir = Vector2.zero; // 방향값을 내가 강제로 없애서 
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
