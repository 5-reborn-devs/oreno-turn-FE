using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;

public class UserInfoSlot : UIListItem
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
    [SerializeField] public Image hitImage;
    public AudioSource audioSoucre;
    public AudioClip hitSound;
    public float fadeDuration = 0.5f; // 페이드 인/아웃 시간

    public int previousHp = 50; // 여기에다가 이전 HP 저장 // 처음에는 hp 초기값 50
    public int idx;
    public UnityAction<int> callback;
    public bool isDeath { get => death.activeInHierarchy; }

    // 디버그 로그 추가 
    //Debug.Log($"Init Slot: {nickname.text}, Index: {index}");
    void Start()
    {
        // 초기 alpha 값을 0으로 설정 (투명) 
        SetAlpha(0f);
    }
    // Alpha 값을 설정하는 함수 
    void SetAlpha(float alpha)
    {
        Color color = hitImage.color;
        color.a = alpha;
        hitImage.color = color;
    }

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
        
        UpdateHp(userinfo.hp,userinfo.maxHp);

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
        if(debuffs.Count > 0)
        { 
            debuffs.ForEach(obj => obj.gameObject.SetActive(false));
        }
    }
    public void UpdateHp(int currentHp, int maxHp)
    {
        float currentHpClamped = Mathf.Clamp(currentHp, 0, maxHp); 
        hpGauge.value = currentHpClamped / maxHp; //맞았을때 검증
        if(currentHp < previousHp)
        {
            Debug.Log("업데이트 HP 까지 드러옴");
            TriggerOpacityChange();
        }
        previousHp = currentHp;

    }

    // 불투명도를 255로 올렸다가 다시 0으로 만드는 함수 호출 
    public void TriggerOpacityChange() { 
        Debug.Log("트리거 온");
        StartCoroutine(ChangeAlpha()); 
        hpGauge.value = currentHpClamped / maxHp; 
        if(currentHp < previousHp) // 맞았을때 검증
        {
            audioSoucre.PlayOneShot(hitSound);
            TriggerOpacityChange();
        }
        previousHp = currentHp;
    }
    
    IEnumerator ChangeAlpha() { 
    // Alpha 값을 1로 올리기 
    float elapsedTime = 0f; 
    while (elapsedTime < fadeDuration) { SetAlpha(Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration)); 
    elapsedTime += Time.deltaTime; 
    yield return null; } 
    SetAlpha(1f); 
    // 잠시 대기 
    yield return new WaitForSeconds(0.1f); 
    // Alpha 값을 다시 0으로 만들기 
    elapsedTime = 0f; 
    while (elapsedTime < fadeDuration) { SetAlpha(Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration)); 
    elapsedTime += Time.deltaTime; yield return null; 
        } 
    SetAlpha(0f); 
    Debug.Log("셋알파까지 끝남");
    }
    
    public async void UpdateData(UserInfo userinfo)
    {
        for (int i = 0; i < 10; i++)
        {
            mpGauges[i].SetActive(userinfo.mp > i);
        }
        
        UpdateHp(userinfo.hp,userinfo.maxHp);

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