using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;

public class CardManager : UIListBase<Card> 
{   

    [SerializeField] private GameObject select;
    private int selectedCard = -1;
    public override void Opened(object[] param)
    {
        SetList();
    }

    public override void SetList()
    {
        ClearList();
        var datas = UserInfo.myInfo.handCards; // 내 손에 있는 카드들
        Debug.Log("CardManager에서 카드 데이터 개수: " + datas.Count);
        for (int i =0; i< datas.Count;i++)
        {
            var data = datas[i];
            // Debug.Log("CardManager에서 보험용 카드 한 장 씩 : " + data);
            var item = AddItem();
            item.Init(data, OnClickItem);
            // Debug.Log($"CardManager에서 카드데이터 idx값 확인: {item.idx}");
            item.SetCardIndex(i);
            // Debug.Log($"CardManager에서 카드가공 후 item {i} : {items[i]}");
        }
        selectedCard = -1;
        select.SetActive(false);    
    }

    // 여기서 카드선택가능하게 해야함
    public void SelectCard(int index)
    {
        if (selectedCard != -1 && selectedCard < items.Count) 
        {
            items[selectedCard].OnSelect(false); // 이전 카드 선택 해제
        }
        selectedCard = index;
        if (items.Count >= selectedCard)
        {
            var selectedItem = items[selectedCard];
            GameManager.instance.SelectedCard = selectedItem.cardData;
            selectedItem.OnSelect(true); 
        }
        
    }

    public void CardUse() 
    {
        if (selectedCard < 0 || selectedCard >= UserInfo.myInfo.handCards.Count)
        {
            Debug.LogWarning("CardUse: 선택된 카드가 유효하지 않습니다.");
            return; 
        }

        var card = UserInfo.myInfo.handCards[selectedCard]; // 선택된 카드 정보
        if (card.rcode == "CAD00005")
        {
            UIManager.ShowAlert("누구에게 사용 하시겠습니까?", "119 호출", "나에게", "모두에게", () =>
            {
                UserInfo.myInfo.OnUseCard(selectedCard);
                GameManager.instance.SendSocketUseCard(UserInfo.myInfo, UserInfo.myInfo, card.rcode);
            }, () =>
            {
                UserInfo.myInfo.OnUseCard(selectedCard);
                GameManager.instance.SendSocketUseCard(null, UserInfo.myInfo, card.rcode);
            });
        }
        else
        {
            UseCard();
        }
    }

    public void UseCard() // 카드를 처리함
    {
        if (selectedCard < 0 || selectedCard >= UserInfo.myInfo.handCards.Count)
        {
            Debug.LogWarning("UseCard: 선택된 카드가 유효하지 않습니다.");
            return; // 선택된 카드가 유효하지 않으면 종료
        }

        var card = UserInfo.myInfo.OnUseCard(selectedCard);
        if (selectedCard == -1) return; // 카드 미선택 상황 시 return.

        GameManager.instance.OnUseCard(card.rcode);
        selectedCard = -1; // 선택 초기화
        SetList();
    }

    public void OnClickItem(CardDataSO data)
    {

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCard(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCard(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCard(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCard(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectCard(4);

        if (Input.GetKeyDown(KeyCode.Space) && selectedCard != -1)
        {
            CardUse();  
        }
    }
}
