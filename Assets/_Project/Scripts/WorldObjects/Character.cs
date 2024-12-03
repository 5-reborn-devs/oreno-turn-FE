using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.AI;
using System;
using Unity.Multiplayer.Playmode;


public class Character : FSMController<CharacterState, CharacterFSM, CharacterDataSO>
{
    [SerializeField] public eCharacterType characterType;
    [SerializeField] private SpriteAnimation anim;
    [SerializeField] private Rigidbody2D rig;
    [SerializeField] private GameObject selectCircle;
    [SerializeField] private SpriteRenderer minimapIcon;
    [SerializeField] private GameObject range;
    [SerializeField] private GameObject targetMark;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject death;
    [SerializeField] private CircleCollider2D collider;
    [SerializeField] public GameObject stop;

    [SerializeField] private float speed = 6;

    [HideInInspector] public UserInfo userInfo;

    public bool isPlayable { get => characterType == eCharacterType.playable; }
    public Vector2 dir;
    public float Speed { get => speed; }
    public bool isInside;
    public Vector2 targetPosition; // 내가 처음에 찍은 마우스 좌표 추가한 거

    private void Awake()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        if (characterType == eCharacterType.npc) minimapIcon.gameObject.SetActive(false);
    }

    public override async void Init(BaseDataSO data)
    {
        this.data = (CharacterDataSO)data;
        fsm = new CharacterFSM(CreateState<CharacterIdleState>().SetElement(anim, rig, this));
        minimapIcon.sprite = await ResourceManager.instance.LoadAsset<Sprite>(data.rcode, eAddressableType.Thumbnail);
    }

    public void SetCharacterType(eCharacterType characterType)
    {
        this.characterType = characterType;
        rig.mass = characterType == eCharacterType.playable ? 10 : 10000;
        agent.enabled = characterType != eCharacterType.playable;
        var tags = CurrentPlayer.ReadOnlyTags();
        if (tags.Length == 0)
        {
            tags = new string[1] { "player1" };
        }
        if (isPlayable)
        {
            if (tags[0].Equals("player1") &&
                (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.WindowsEditor))
            {
                UIGame.instance.stick.gameObject.SetActive(false);
            }
            else
            {
                UIGame.instance.stick.OnHandleChanged += MoveCharacter;
            }
        }
    }

    public void SetMovePosition(Vector3 pos)
    {
        if (characterType == eCharacterType.playable) return;
        rig.MovePosition(pos);
        agent.SetDestination(pos);
        var isLeft = agent.velocity.x < 0;
        isLeft = data.isLeft ? !isLeft : isLeft;
        if (agent.velocity.x != 0)
            anim.SetFlip(isLeft);
        if (agent.velocity == Vector3.zero)
            ChangeState<CharacterIdleState>().SetElement(anim, rig, this);
        else
            ChangeState<CharacterWalkState>().SetElement(anim, rig, this);
    }

    public void SetPosition(Vector3 pos)
    {
        agent.enabled = false;
        transform.position = pos;
        agent.enabled = true;
    }

    public void OnChangeState<T>() where T : CharacterState
    {
        ChangeState<T>().SetElement(anim, rig, this);
    }

    public bool IsState<T>()
    {
        return fsm.IsState<T>();
    }

    public void SetTargetMark()
    {
        targetMark.SetActive(true);
    }

    public void OnVisibleMinimapIcon(bool visible)
    {
        if(characterType == eCharacterType.non_playable)
            minimapIcon.gameObject.SetActive(visible && !isInside);
        else
            minimapIcon.gameObject.SetActive(false);
    }

    public void OnSelect()
    {
        selectCircle.SetActive(!selectCircle.activeInHierarchy);
    }

    public void OnVisibleRange()
    {
        range.SetActive(!range.activeInHierarchy);
    }

    public void MoveCharacter(Vector2 Vecotr2tranPos)
    {
        if (fsm.IsState<CharacterStopState>() || fsm.IsState<CharacterPrisonState>() || fsm.IsState<CharacterDeathState>()) return;
        // 캐릭터의 현재 위치와 목표 위치 간의 방향 벡터 계산
        Vector2 currentPos = GameManager.instance.userCharacter.transform.position; // 내 캐릭터의 현재 위치 


        this.targetPosition = Vecotr2tranPos; // 내가 찍은 월드 좌표 character.targetPosition 으로 접근 가능하게 만듬
        //정규화한 dir 생성 방향 20,13  1,1 우상단이냐 우하단이냐 좌상단이냐 좌하단이냐 정보만알수있게 만들어줍니다.
        Vector2 dir = new Vector2(Vecotr2tranPos.x - currentPos.x, Vecotr2tranPos.y - currentPos.y).normalized; 
        this.dir = dir;
        var isLeft = dir.x < 0;
        isLeft = data.isLeft ? !isLeft : isLeft;
        if (dir.x != 0)
            anim.SetFlip(isLeft);
        if (dir == Vector2.zero) ChangeState<CharacterIdleState>().SetElement(anim, rig, this); // 정지
        else
        {
            ChangeState<CharacterWalkState>().SetElement(anim, rig, this); // 걷는
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            if (characterType == eCharacterType.playable)
            {
                GameManager.instance.SetMapInside(true);
            }
            isInside = true;
            if (userInfo != null)
                OnVisibleMinimapIcon(Util.GetDistance(UserInfo.myInfo.index, userInfo.index, DataManager.instance.users.Count)
                    + userInfo.slotFar <= UserInfo.myInfo.slotRange && userInfo.id != UserInfo.myInfo.id); // 가능한 거리에 있는 유저 아이콘만 표시

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            if (characterType == eCharacterType.playable)
            {
                GameManager.instance.SetMapInside(false);
            }
            isInside = false;
            if (userInfo != null)
                OnVisibleMinimapIcon(Util.GetDistance(UserInfo.myInfo.index, userInfo.index, DataManager.instance.users.Count)
                    + userInfo.slotFar <= UserInfo.myInfo.slotRange && userInfo.id != UserInfo.myInfo.id); // 가능한 거리에 있는 유저 아이콘만 표시
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.TryGetComponent<Character>(out var character))
        {
            if(!SocketManager.instance.isConnected && character == GameManager.instance.userCharacter &&
                userInfo.handCards.Find(obj => obj.rcode == "CAD00001"))
            {
                GameManager.instance.SendSocketUseCard(character.userInfo, userInfo, "CAD00001");
            }
        }
    }

    private void Update()
    {
        if(fsm != null)
            fsm.UpdateState();
    }

    public async void SetDeath()
    {
        death.SetActive(true);
        collider.enabled = false;
        targetMark.SetActive(true);
        targetMark.GetComponent<SpriteRenderer>().sprite = await ResourceManager.instance.LoadAsset<Sprite>("Role_" + userInfo.roleType.ToString(), eAddressableType.Thumbnail);
        minimapIcon.gameObject.SetActive(false);
        ChangeState<CharacterDeathState>();
    }

    protected override T ChangeState<T>()
    {
        if (!IsState<CharacterDeathState>())
            return base.ChangeState<T>();
        else
            return currentState == null ? null : (T)currentState;
    }
}