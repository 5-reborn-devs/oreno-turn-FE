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
    [SerializeField] private TMP_Text nickname;
    [SerializeField] private Slider hpGauge;
    [SerializeField] private GameObject targetMark;
    [SerializeField] private TMP_Text index;
    [SerializeField] private Image weapon;
    [SerializeField] private List<Image> equips;
    [SerializeField] private List<Image> debuffs;
    [SerializeField] private GameObject select;
    [SerializeField] private GameObject death;
    [SerializeField] private List<GameObject> mpSlots;
    [SerializeField] private List<GameObject> mpGauges;

    public int idx;
    public UnityAction<int> callback;
    public bool isDeath { get => death.activeInHierarchy; }

    public async Task Init(UserInfo userinfo, int index, UnityAction<int> callback)
    {
        this.idx = index;
        this.callback = callback;
        nickname.text = userinfo.nickname;
        var data = DataManager.instance.GetData<CharacterDataSO>(userinfo.selectedCharacterRcode);
        thumbnail.sprite = await ResourceManager.instance.LoadAsset<Sprite>(data.rcode, eAddressableType.Thumbnail);
        targetMark.GetComponent<Image>().sprite = await ResourceManager.instance.LoadAsset<Sprite>("role_" + userinfo.roleType.ToString(), eAddressableType.Thumbnail);

        for (int i = 0; i < 10; i++)
        {
            mpSlots[i].SetActive(userinfo.mp > i);
            mpGauges[i].SetActive(userinfo.mp > i);
        }
        
        UpdateHp(userinfo.hp, userinfo.maxHp);
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
        float currentHpClamped = Mathf.Clamp(currentHp, 0, maxHp);
        hpGauge.value = currentHpClamped / maxHp;
    }

    public async void UpdateData(UserInfo userinfo)
    {
        for (int i = 0; i < 10; i++)
        {
            mpGauges[i].SetActive(userinfo.mp > i);
        }

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
        for (int i = 0; i < 10; i++)
        {
            mpGauges[i].SetActive(false);
        }
        death.SetActive(true);
        SetVisibleRole(true);
    }
}
