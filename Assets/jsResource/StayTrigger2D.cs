using UnityEngine;
using System.Collections;

public class StayTrigger2D : MonoBehaviour
{
    public float requiredStayTime = 3.0f; // 트리거를 발생시키기 위해 필요한 시간
    private bool isStaying = false; // 캐릭터가 충돌 영역에 머물고 있는지 여부
    private float stayTimer = 0.0f; // 머문 시간을 추적하는 타이머
    public SocketManager socketManager; // SocketManager 인스턴스를 저장할 변수
    public CircleCollider2D hitBox;
    public SpriteRenderer image;
    public ParticleSystem particleSystem;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // 캐릭터가 Player 태그를 가지고 있는지 확인
        {
 
            isStaying = true;
            stayTimer = 0.0f;
            StartCoroutine(StayCheck());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isStaying = false;
            stayTimer = 0.0f;
            StopCoroutine(StayCheck());
        }
    }

    IEnumerator StayCheck()
    {
        while (isStaying)
        {
            stayTimer += Time.deltaTime;
            if (stayTimer >= requiredStayTime) // 어떤한유저 가 충족했한게 맞음
            {
                TriggerEvent();
                yield break; // 코루틴 종료
            }
            yield return null;
        }
    }

    void TriggerEvent()
    {
        
        Debug.Log("트리거 발생!");
        SendReactionPacket();
        gameObject.SetActive(false);
        
 
    }

    void SendReactionPacket()
    {
        var packet = new GamePacket();
        packet.ReactionRequest = new C2SReactionRequest();
        packet.ReactionRequest.ReactionType = ReactionType.NotUseCard; // 적절한 ReactionType 값을 사용
        SocketManager.instance.Send(packet);
        Debug.Log("패킷 전송됨!");
    }

    // void Send(GamePacket packet)
    // {
    //     if (socketManager != null)
    //     {
    //         socketManager.Send(packet);
    //         Debug.Log("패킷 전송 로직 실행됨");
    //     }
    //     else
    //     {
    //         Debug.LogError("SocketManager 인스턴스가 설정되지 않았습니다.");
    //     }
    // }
}
