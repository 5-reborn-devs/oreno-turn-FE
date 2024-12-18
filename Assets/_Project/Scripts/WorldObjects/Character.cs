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

    [SerializeField] private float speed = 4.5f;

    [HideInInspector] public UserInfo userInfo;

    public bool isPlayable { get => characterType == eCharacterType.playable; }
    public Vector2 dir;
    public float Speed { get => speed; }
    public bool isInside;
    public Vector2 targetPosition; // ���� ó���� ���� ���콺 ��ǥ �߰��� ��

    public float attackRange = 5f;

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
        if (states.ContainsKey(typeof(T).Name))
        {
            ChangeState<T>()?.SetElement(anim, rig, this);
        }
        else
        {
            ChangeState<T>()?.SetElement(anim, rig, this);
        }
        
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
    public void SelectFalse()
    {
        selectCircle.SetActive(false);
    }
    public void OnVisibleRange()
    {
        range.SetActive(!range.activeInHierarchy);
    }

    public void MoveCharacter(Vector2 Vecotr2tranPos)
    {
        if (fsm.IsState<CharacterStopState>() || fsm.IsState<CharacterPrisonState>() || fsm.IsState<CharacterDeathState>()) return;
        // ĳ������ ���� ��ġ�� ��ǥ ��ġ ���� ���� ���� ���
        Vector2 currentPos = GameManager.instance.userCharacter.transform.position; // �� ĳ������ ���� ��ġ 


        this.targetPosition = Vecotr2tranPos; // ���� ���� ���� ��ǥ character.targetPosition ���� ���� �����ϰ� ����
        //����ȭ�� dir ���� ���� 20,13  1,1 �����̳� ���ϴ��̳� �»���̳� ���ϴ��̳� �������˼��ְ� ������ݴϴ�.
        Vector2 dir = new Vector2(Vecotr2tranPos.x - currentPos.x, Vecotr2tranPos.y - currentPos.y).normalized; 
        this.dir = dir;
        var isLeft = dir.x < 0;
        isLeft = data.isLeft ? !isLeft : isLeft;
        if (dir.x != 0)
            anim.SetFlip(isLeft);
        if (dir == Vector2.zero) ChangeState<CharacterIdleState>().SetElement(anim, rig, this); // ����
        else
        {
            ChangeState<CharacterWalkState>().SetElement(anim, rig, this); // �ȴ�
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
                    + userInfo.slotFar <= UserInfo.myInfo.slotRange && userInfo.id != UserInfo.myInfo.id); // ������ �Ÿ��� �ִ� ���� �����ܸ� ǥ��

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
                    + userInfo.slotFar <= UserInfo.myInfo.slotRange && userInfo.id != UserInfo.myInfo.id); // ������ �Ÿ��� �ִ� ���� �����ܸ� ǥ��
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

    public bool IsTargetInRange(Vector2 targetPosition)
    {
        // 타겟과의 거리를 계산하여 사정거리 내에 있는지 확인
        float distance = Vector2.Distance(transform.position, targetPosition);
        return distance <= attackRange;
    }

    private void Update()
    {
        if(fsm != null)
            fsm.UpdateState();
    }

    public async void SetDeath()
    {   
        // GameManager.instance.characters.Remove(id);
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
            return null;
    }
}