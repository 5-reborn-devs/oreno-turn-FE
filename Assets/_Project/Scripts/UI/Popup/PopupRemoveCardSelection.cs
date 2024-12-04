using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Google.Protobuf.Collections;

public class PopupRemoveCardSelection : UIListBase<Card>
{
    [SerializeField] private UIPagingViewController uiPagingViewController;
    [SerializeField] private Button use;
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private List<Transform> equipSlots;
    [SerializeField] private List<Transform> debuffSlots;
    [SerializeField] private TMP_Text count;

    List<Card> selectCards = new List<Card>();
    UserInfo targetUserInfo;
    string targetRcode;

    public override void Opened(object[] param)
    {
        targetUserInfo = UserInfo.myInfo;
        count.text = string.Format("���� ���� : {0} �ʿ� ���� : {1}", selectCards.Count, Mathf.Max(0, UserInfo.myInfo.handCards.Count - UserInfo.myInfo.hp));
        SetList();
    }

    public override void HideDirect()
    {
        UIGame.instance.SetDeckCount();
        UIManager.Hide<PopupRemoveCardSelection>();
    }

    public void ClearWeapon()
    {
        if (weaponSlot.childCount > 0)
            Destroy(weaponSlot.GetChild(0).gameObject);
    }

    public void AddWeapon(CardDataSO data)
    {
        var item = Instantiate(itemPrefab, weaponSlot);
        item.Init(data);
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.rectTransform.sizeDelta = new Vector2(180, 246);
    }

    public void ClearEquips()
    {
        for (int i = 0; i < equipSlots.Count; i++)
        {
            if (equipSlots[i].childCount > 0)
                Destroy(equipSlots[i].GetChild(0).gameObject);
        }
    }

    public void AddEquip(CardDataSO data, int idx)
    {
        var slot = equipSlots[idx];
        var item = Instantiate(itemPrefab, slot);
        item.Init(data);
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.rectTransform.sizeDelta = new Vector2(180, 246);
    }

    public void ClearDebuffs()
    {
        for (int i = 0; i < debuffSlots.Count; i++)
        {
            if (debuffSlots[i].childCount > 0)
                Destroy(debuffSlots[i].GetChild(0).gameObject);
        }
    }

    public void AddDebuff(CardDataSO data, int idx)
    {
        var slot = debuffSlots[idx];
        var item = Instantiate(itemPrefab, slot);
        item.Init(data);
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.rectTransform.sizeDelta = new Vector2(180, 246);
    }

    public override void SetList()
    {
        ClearList();
        ClearEquips();
        ClearWeapon();
        ClearDebuffs();
        for (int i = 0; i < targetUserInfo.handcardCount; i++)
        {
            var item = AddItem();
            item.Init(targetUserInfo.handCards[i], OnClickItem);
        }
        if(targetUserInfo.weapon != null)
        {
            AddWeapon(targetUserInfo.weapon);
        }
        for (int i = 0; i < targetUserInfo.equips.Count; i++)
        {
            AddEquip(targetUserInfo.equips[i], i);
        }
        for (int i = 0; i < targetUserInfo.debuffs.Count; i++)
        {
            AddDebuff(targetUserInfo.debuffs[i], i);
        }
    }

    // public void OnClickItem(Card card)
    // {
    //     if (selectCards.Contains(card))
    //     {
    //         selectCards.Remove(card);
    //         card.OnSelect(false);
    //     }
    //     else
    //     {
    //         selectCards.Add(card);
    //         card.OnSelect(true);
    //     }
    //     count.text = string.Format("���� ���� : {0} �ʿ� ���� : {1}", selectCards.Count, Mathf.Max(0, UserInfo.myInfo.handCards.Count - UserInfo.myInfo.hp));
    //     use.gameObject.SetActive(UserInfo.myInfo.handCards.Count - selectCards.Count == UserInfo.myInfo.hp);
    //     count.gameObject.SetActive(UserInfo.myInfo.handCards.Count - selectCards.Count != UserInfo.myInfo.hp);
    // }

    public void OnClickItem(Card card)
{
    if (SocketManager.instance.isConnected)
    {
        // 서버로 데이터 전송
        SendCardDataToServer(card);
        
        // 카드 데이터 클라이언트 패에서 제거
        RemoveCardFromClientHand(card);

        // 팝업 종료
        UIManager.Hide<PopupRemoveCardSelection>();
    }
    else
    {
        // 서버와 연결되지 않은 경우의 처리 (옵션)
    }
}

private void SendCardDataToServer(Card card)
{
    GamePacket packet = new GamePacket();
    packet.DestroyCardRequest = new C2SDestroyCardRequest();
    packet.DestroyCardRequest.DestroyCards.Add(new CardData() { Type = card.cardData.cardType, Count = 1 });
    SocketManager.instance.Send(packet);
}

private void RemoveCardFromClientHand(Card card)
{
    if (selectCards.Contains(card))
    {
        selectCards.Remove(card);
        UserInfo.myInfo.handCards.Remove(card.cardData);
    }
}


    public List<CardData> CreateField()
    {
        var list = new List<CardData>();
        for (int i = 0; i < selectCards.Count; i++)
        {
            var cardData = list.Find(obj => obj.Type == selectCards[i].cardData.cardType);
            if(cardData != null)
            {
                cardData.Count++;
            }
            else
            {
                list.Add(new CardData() { Type = selectCards[i].cardData.cardType, Count = 1 });
            }
        }
        return list;
    }

    public void OnClickUse()
    {
        if (SocketManager.instance.isConnected)
        {
            GamePacket packet = new GamePacket();
            packet.DestroyCardRequest = new C2SDestroyCardRequest();
            packet.DestroyCardRequest.DestroyCards.AddRange(CreateField());
            SocketManager.instance.Send(packet);
            //  UIManager.Hide<PopupRemoveCardSelection>();
            //  ClearList();
        }
        
        else{
        }
}
}