using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Unity.VisualScripting;
using System;

public class UIGame : UIBase
{
    public static UIGame instance { get => UIManager.Get<UIGame>(); }
    [SerializeField] private TMP_Text shotCount;
    [SerializeField] private UserInfoSlot userInfoSlot;
    [SerializeField] private UserInfoSlot anotherSlotPrefab;
    [SerializeField] private RectTransform userInfoParent;
    [SerializeField] private TMP_Text dayInfo;
    [SerializeField] private TMP_Text time;
    [SerializeField] private GameObject selectCard;
    [SerializeField] private TMP_Text selectCardText;
    [SerializeField] private TMP_Text deckCount;
    [SerializeField] private Button buttonShot;
    [SerializeField] private TMP_Text noticeText;
    [SerializeField] private TMP_Text noticeLogItem;
    [SerializeField] private GameObject noticeLog;
    [SerializeField] private Transform noticeLogParent;
    [SerializeField] public VirtualStick stick;
    [SerializeField] private Image bombButton;
    [SerializeField] private CardManager cardManager;
    [SerializeField] public OppoInfoSlot oppoInfoSlot;
    [SerializeField] public Image dayImage; 
    [SerializeField] public Image eveningImage; 
    [SerializeField] public Image nightImage;
    [SerializeField] private GameObject nightOrb;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip daybgm;
    [SerializeField] public AudioClip eveningbgm;
    [SerializeField] public AudioClip nightbgm;


    private Coroutine oppoInfoSlotCoroutine;

    //private MoveUpAndDown moveUpAndDown;
    private float timer = 180;
    Dictionary<long, UserInfoSlot> userslots = new Dictionary<long, UserInfoSlot>();
    private bool isBombTargetSelect = false;
    bool isBombAlert = false;

    public bool isOn = false;

    public override void Opened(object[] param)
    {
        StartCoroutine(Init());
    }

    public void Start(){
        nightOrb = GameObject.FindWithTag("NightOrbTag"); // 태그로 찾기

        if (nightOrb != null) { nightOrb.SetActive(false); 
        Debug.Log("nightOrb 오브젝트가 설정되었습니다: " + nightOrb.name); } 
        else { Debug.LogError("nightOrb 오브젝트를 찾을 수 없습니다."); }

    }


    public IEnumerator Init()
    {
        yield return new WaitUntil(() => GameManager.instance.isInit);
        for (int i = 0; i < DataManager.instance.users.Count; i++)
        {
            if (DataManager.instance.users[i].id != UserInfo.myInfo.id)
            {
                var item = Instantiate(anotherSlotPrefab, userInfoParent);
                yield return item.Init(DataManager.instance.users[i], i, OnClickCharacterSlot);
                userslots.Add(DataManager.instance.users[i].id, item);
                cardManager.SetList();
            }
            else
            {
                yield return userInfoSlot.Init(UserInfo.myInfo, i, OnClickCharacterSlot);
                userInfoSlot.SetVisibleRole(true);
            }
        }
        SetShotButton(false);
    }

    // 선택된 상대의 정보를 oppoInfoSlot에 업데이트하는 메서드
    public async void OnClickOpponents(Character targetCharacter){

        Debug.Log("OnClickOpponents 호출됨"); // 디버그 로그 추가

        if (targetCharacter != null) { UserInfo targetUserInfo = targetCharacter.userInfo; 

        Debug.Log($"Target User: {targetUserInfo?.nickname}, ID: {targetUserInfo?.id}"); 
        Debug.Log($"OppoInfoSlot: {oppoInfoSlot != null}");

        // 이전 코루틴이 실행 중이라면 중지 
        if (oppoInfoSlotCoroutine != null) {
            StopCoroutine(oppoInfoSlotCoroutine); 
        }

        // oppoInfoSlot에 선택된 상대의 정보를 업데이트 
        await oppoInfoSlot.Init(targetUserInfo, (int)targetUserInfo.id, null);
        // oppoInfoSlot을 활성화하여 화면에 표시 
        oppoInfoSlot.gameObject.SetActive(true); 
        
        // //5초 후 비활성화하는 코루틴 시작 
         oppoInfoSlotCoroutine = StartCoroutine(DisableOppoInfoSlotAfterDelay(targetUserInfo,5f));

        // 5초 동안 활성화 상태 유지하면서 체력 정보 갱신 
        // oppoInfoSlotCoroutine = StartCoroutine(DisableOppoInfoSlotAfterDelay(targetUserInfo,5f));
        }
        else{
            Debug.Log("targetCharacter가 null입니다.");
        }
        
    }

    public void OnClickCharacterSlot(int idx)
    {
        if (GameManager.instance.SelectedCard == null && !GameManager.instance.isSelectBombTarget) return;
        var card = GameManager.instance.SelectedCard;
        var target = DataManager.instance.users[idx];
        if (card != null && (card.isTargetSelect || (card.rcode == "CAD00001" && UserInfo.myInfo.weapon != null && UserInfo.myInfo.weapon.rcode == "CAD00013")))
        {
            GameManager.instance.SendSocketUseCard(target, UserInfo.myInfo, card.rcode);
            SetSelectCard(null);
        }
        if (GameManager.instance.isSelectBombTarget)
        {
            GameManager.instance.isSelectBombTarget = false;
            if (SocketManager.instance.isConnected)
            {
                GamePacket packet = new GamePacket();
                packet.PassDebuffRequest = new C2SPassDebuffRequest() { DebuffCardType = CardType.Bomb, TargetUserId = target.id };
                SocketManager.instance.Send(packet);
            }
            else
            {
                var targetCard = UserInfo.myInfo.debuffs.Find(obj => obj.rcode == "CAD00023");
                target.debuffs.Add(targetCard);
                UserInfo.myInfo.debuffs.Remove(targetCard);
            }
        }
        UIGame.instance.OnSelectDirectTarget(false);
    }
    public void UpdateUserSlot(List<UserInfo> users)
    {
        for (int i = 0; i < users.Count; i++)
        {
            if (userslots.ContainsKey(users[i].id))
            {
                userslots[users[i].id].UpdateData(users[i]);
            }
            else
            {
                userInfoSlot.UpdateData(users[i]);
            }
        }
        SetDeckCount();
    }
    public void SetShotButton(bool isActive)
    {
        buttonShot.interactable = isActive;
        selectCard.SetActive(isActive);
        shotCount.transform.parent.gameObject.SetActive(isActive);
    }
    public void SetDeckCount()
    {
        deckCount.text = UserInfo.myInfo.handCards.Count.ToString();
        cardManager.SetList();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && buttonShot.interactable) // ????? ??????????? ?????????? ??????
        {
            OnClickBang(); // ??? ??? ????
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnClickReroll();
        }

        if(isOn == true){

        }

        if (!GameManager.instance.isPlaying) return;
        timer -= Time.deltaTime;
        time.text = string.Format("{0:00}:{1:00}", Mathf.Floor(timer / 60), timer % 60);
        if (timer <= 0 && !SocketManager.instance.isConnected) GameManager.instance.OnTimeEnd();
    }
    public override void HideDirect()
    {
        UIManager.Hide<UIGame>();
    }
    public void OnDaySetting(int day, PhaseType phase, long nextAt)
    {
        dayInfo.text = string.Format("Day {0} {1}", day, phase == PhaseType.Day ? "Afternoon" : phase == PhaseType.Evening ? "Evening" : "Night");
        var dt = DateTimeOffset.FromUnixTimeMilliseconds(nextAt) - DateTime.UtcNow;
        timer = (float)dt.TotalSeconds;
        //timer = phase == 1 ? 180 : 60;

        // 이미지 변경 로직 
        switch (phase) { 
        case PhaseType.Day: 
            SetPhaseImage(dayImage);
            SetPhasebgm(daybgm);
            break; 
        case PhaseType.Evening: 
            SetPhaseImage(eveningImage);
            SetPhasebgm(eveningbgm);
            cardManager.DisableHand(); // PhaseType.Evening일 때 hand 비활성화
            break; 
        case PhaseType.End: 
            SetPhaseImage(nightImage);
            SetPhasebgm(nightbgm);
            cardManager.EnableHand();
            break; 
        }
        // if (moveUpAndDown != null) { 
        //     Debug.Log("UIGame OnDaySetting: PhaseType 설정 - " + phase); 
        //     moveUpAndDown.SetPhase(phase); } else { Debug.LogError("MoveUpAndDown 컴포넌트가 설정되지 않았습니다."); }

    }
    public void SetPhaseImage(Image activeImage) { 
        
        // 모든 이미지를 비활성화 
        dayImage.gameObject.SetActive(false); 
        eveningImage.gameObject.SetActive(false); 
        nightImage.gameObject.SetActive(false); 
        
        // 현재 페이즈에 해당하는 이미지를 활성화 
        activeImage.gameObject.SetActive(true); 
        
        }
    public void SetPhasebgm(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
    public void OnClickDeck()
    {
        if (!GameManager.instance.userCharacter.IsState<CharacterStopState>() &&
            !GameManager.instance.userCharacter.IsState<CharacterDeathState>())
        {
            UIManager.Show<PopupDeck>();
        }
    }
    public void OnClickBang()
    {
        if (UserInfo.myInfo.isShotPossible || GameManager.instance.SelectedCard.cardType != CardType.Bbang)
            GameManager.instance.OnUseCard();
    }

    public void OnClickReroll()
    {
        if (UserInfo.myInfo.isRerollPossible || SocketManager.instance.isConnected)
        {
            GamePacket packet = new GamePacket();
            packet.RerollRequest = new C2SRerollRequest() {};
            SocketManager.instance.Send(packet);
        }
    }

    public void SetSelectCard(CardDataSO card = null)
    {
        if (card == null)
        {
            if (GameManager.instance.SelectedCard != null)
            {
                if (UserInfo.myInfo.handCards.Find(obj => obj.rcode == GameManager.instance.SelectedCard.rcode) == null ||
                    (GameManager.instance.SelectedCard.cardType == CardType.Bbang && !UserInfo.myInfo.isShotPossible))
                {
                    GameManager.instance.UnselectCard();
                    SetShotButton(false);
                    return;
                }
            }
            return;
        }
        card = UserInfo.myInfo.handCards.Find(obj => obj.rcode == card.rcode);
        if (card == null)
        {
            GameManager.instance.UnselectCard();
            SetShotButton(false);
            return;
        }
        var shotCount = SetShotCount();
        if (shotCount > 0)
        {
            selectCardText.text = card.displayName;
            SetShotButton(true);
        }
        else
        {
            SetShotButton(false);
        }
    }
    public int SetShotCount()
    {
        var card = GameManager.instance.SelectedCard;
        var count = UserInfo.myInfo.handCards.FindAll(obj => obj.rcode == card.rcode).Count;
        shotCount.text = count.ToString();
        if (card.cardType == CardType.Bbang)
        {
            count = Mathf.Min(UserInfo.myInfo.handCards.FindAll(obj => obj.rcode == card.rcode).Count, UserInfo.myInfo.bbangCount - UserInfo.myInfo.shotCount);
            shotCount.text = count.ToString();
        }
        return count;
    }
    public void OnClickNotice()
    {
        noticeLog.SetActive(true);
    }
    public void OnClickCloseNotice()
    {
        noticeLog.SetActive(false);
    }
    public void SetNotice(string notice)
    {
        noticeText.text = notice;
        var item = Instantiate(noticeLogItem, noticeLogParent);
        item.text = notice;
        noticeLogItem.rectTransform.sizeDelta = new Vector2(item.preferredWidth, item.preferredHeight);
    }
    public void SetBombButton(bool isActive)
    {
        bombButton.gameObject.SetActive(isActive);
    }
    public void OnClickBomb()
    {
        if (GameManager.instance.targetCharacter != null && GameManager.instance.targetCharacter.tag == "Bomb")
        {
            GameManager.instance.isSelectBombTarget = true;
            OnSelectDirectTarget(true);
        }
    }
    public void SetBombAlert(bool isAlert)
    {
        if (isAlert)
        {
            StartCoroutine(BombAlert());
        }
        else
        {
            StopCoroutine(BombAlert());
            bombButton.color = Color.white;
        }
    }
    public IEnumerator BombAlert()
    {
        var col = 1f;
        bool isDown = true;
        while (true)
        {
            bombButton.color = new Color(1, col, col);
            yield return null;
            col += 0.1f * (isDown ? -1 : 1);
            if (col <= 0) isDown = true;
            if (col >= 1) isDown = false;
        }
    }
    public void OnSelectDirectTarget(bool isActive)
    {
        foreach (var key in userslots.Keys)
        {
            if (!userslots[key].isDeath)
                userslots[key].SetSelectVisible(isActive);
        }
    }
    public void SetDeath(long id)
    {
        if (userslots.ContainsKey(id))
        {
            userslots[id].SetDeath();
        }
        else
        {
            userInfoSlot.SetDeath();
        }
    }

     private IEnumerator DisableOppoInfoSlotAfterDelay(UserInfo targetUserInfo, float delay) { 

        float elapsed = 0f;
        int initialHp = targetUserInfo.hp; // 초기 체력 저장

            while (elapsed < delay){

                oppoInfoSlot.UpdateData(targetUserInfo);

                if (targetUserInfo.hp != initialHp){
                     StopCoroutine(oppoInfoSlotCoroutine); // 현재 코루틴 종료
                     
                     Debug.Log("체력 변경 감지, OnClickOpponents 호출");
                     OnClickOpponents(GameManager.instance.targetCharacter); //이부분 게임매니저에서 부를 필요 없이 targetUserInfo로 끝낼수 있는거 아니야?
                     yield break;
                }
                elapsed += 0.01f; // 0.1초마다 갱신
                yield return new WaitForSeconds(0.01f); 
            }
        oppoInfoSlot.gameObject.SetActive(false); 
    }

// private IEnumerator DisableOppoInfoSlotAfterDelay(UserInfo targetUserInfo, float delay)
// {
//     float elapsed = 0f;
//     int initialHp = targetUserInfo.hp; // 초기 체력 저장

//     while (elapsed < delay)
//     {
//         // 현재 체력 정보를 갱신
//         if (targetUserInfo != null)
//         {
//             oppoInfoSlot.UpdateData(targetUserInfo);

//             // 체력이 변경되었는지 확인
//             if (targetUserInfo.hp != initialHp)
//             {
//                 Debug.Log("체력 변경 감지, OnClickOpponents 호출");
//                 // 체력 변경 시 OnClickOpponents 다시 호출
//                 StopCoroutine(oppoInfoSlotCoroutine); // 현재 코루틴 종료
//                 OnClickOpponents(GameManager.instance.targetCharacter); // GameManager의 targetCharacter 사용
//                 yield break;
//             }
//         }
//         elapsed += 0.1f; // 0.1초마다 갱신
//     }
//     oppoInfoSlot.gameObject.SetActive(false); // 5초 후 비활성화
// }



}