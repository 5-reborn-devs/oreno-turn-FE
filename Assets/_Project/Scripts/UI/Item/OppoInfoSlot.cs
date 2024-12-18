using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Threading.Tasks;

public class OppoInfoSlot : UIListItem
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TMP_Text nickname2;
    [SerializeField] private Slider hpGauge;
    [SerializeField] private GameObject targetMark;
    [SerializeField] private TMP_Text index;
    [SerializeField] private Image weapon;
    [SerializeField] private List<Image> equips;
    [SerializeField] private List<Image> debuffs;
    [SerializeField] private GameObject select;
    [SerializeField] private GameObject death;

    public int idx;
    public UnityAction<int> callback;
    public UserInfo currentUserInfo;
    public int previousHp = 0;
    public bool isDeath { get => death.activeInHierarchy; }

    public async Task Init(UserInfo userinfo, int index, UnityAction<int> callback)
    {   
        Debug.Log("오퍼넌트 초기화까지는 들어왔나요? 체력은 잘들고 왔고?"+ userinfo.hp + userinfo.maxHp);
        Debug.Log("Init 호출됨 - index: " + index); // 디버그 로그 추가
        this.idx = index;
        this.callback = callback;
        this.currentUserInfo = userinfo; // 현재 유저 정보 저장
        Debug.Log("유저인포의 닉네임"+ userinfo.nickname);
        Debug.Log("유저인포" + userinfo);
        nickname2.text = userinfo.nickname;
        var data = DataManager.instance.GetData<CharacterDataSO>(userinfo.selectedCharacterRcode);
        thumbnail.sprite = await ResourceManager.instance.LoadAsset<Sprite>(data.rcode, eAddressableType.Thumbnail);
        targetMark.GetComponent<Image>().sprite = await ResourceManager.instance.LoadAsset<Sprite>("role_" + userinfo.roleType.ToString(), eAddressableType.Thumbnail);

   

        // for (int i = 0; i < 10; i++)
        // {
        //     mpSlots[i].SetActive(userinfo.mp > i);
        //     mpGauges[i].SetActive(userinfo.mp > i);
        // }
        Debug.Log("여기까지?????");
        Debug.Log("Init - UpdateHp 호출 전"); // 디버그 로그 추가
        UpdateHp(userinfo.hp, userinfo.maxHp);
        Debug.Log("Init - UpdateHp 호출 후"); // 디버그 로그 추가

        targetMark.SetActive(userinfo.roleType == eRoleType.target);
        this.index.text = (index + 1).ToString();
        gameObject.SetActive(true);
        if (weapon != null)
        {
            weapon.gameObject.SetActive(false);
        }
        if (equips.Count > 0)
        {
            equips.ForEach(obj => obj.gameObject.SetActive(false));
        }
        if (debuffs.Count > 0)
        { 
            debuffs.ForEach(obj => obj.gameObject.SetActive(false));
        }
    }

    public void UpdateHp(int currentHp, int maxHp)
    {   
        Debug.Log("업데이트 HP 들어왔어요" + hpGauge.value);
        float currentHpClamped = Mathf.Clamp(currentHp, 0, maxHp);
        hpGauge.value = currentHpClamped / maxHp;
    }

    public async void UpdateData(UserInfo userinfo)
    {
        UpdateHp(userinfo.hp, userinfo.maxHp);

        if (weapon != null)
        {
            weapon.gameObject.SetActive(userinfo.weapon != null);
            if (userinfo.weapon != null)
            {
                weapon.sprite = await ResourceManager.instance.LoadAsset<Sprite>("icon_" + userinfo.weapon.rcode, eAddressableType.Thumbnail);
            }
        }
        for (int i = 0; i < equips.Count; i++)
        {
            if (userinfo.equips.Count > i)
            {
                equips[i].gameObject.SetActive(true);
                equips[i].sprite = await ResourceManager.instance.LoadAsset<Sprite>("icon_" + userinfo.equips[i].rcode, eAddressableType.Thumbnail);
            }
            else
            {
                equips[i].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < debuffs.Count; i++)
        {
            if (userinfo.debuffs.Count > i)
            {
                debuffs[i].gameObject.SetActive(true);
                debuffs[i].sprite = await ResourceManager.instance.LoadAsset<Sprite>("icon_" + userinfo.debuffs[i].rcode, eAddressableType.Thumbnail);
            }
            else
            {
                debuffs[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnClickItem()
    {
        callback?.Invoke(idx);
    }

    public void SetSelectVisible(bool visible)
    {
        select.SetActive(visible);
    }

    public void SetVisibleRole(bool isVisible)
    {
        targetMark.SetActive(isVisible);
    }

    public void SetDeath()
    {
        hpGauge.gameObject.SetActive(false);
        death.SetActive(true);
        SetVisibleRole(true);
    }
}
