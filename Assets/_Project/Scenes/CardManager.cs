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
    [SerializeField] private GameObject hand; // Hand GameObject 추가


    public override void Opened(object[] param)
    {
        SetList();
    }

    public override void SetList()
    {
        ClearList();
        var datas = UserInfo.myInfo.handCards; // �� �տ� �ִ� ī���
        // Debug.Log("CardManager���� ī�� ������ ����: " + datas.Count);
        for (int i =0; i< datas.Count;i++)
        {
            var data = datas[i];
            // Debug.Log("CardManager���� ����� ī�� �� �� �� : " + data);
            var item = AddItem();
            item.Init(data, OnClickItem);
            // Debug.Log($"CardManager���� ī�嵥���� idx�� Ȯ��: {item.idx}");
            item.SetCardIndex(i);
            // Debug.Log($"CardManager���� ī�尡�� �� item {i} : {items[i]}");
        }
        selectedCard = -1;
        //select.SetActive(false);    
    }

    public void DisableHand() {
         hand.SetActive(false); 
    // Hand 비활성화 
    } 
    public void EnableHand() { 
        hand.SetActive(true); 
    // Hand 활성화 
    }
    
    // ���⼭ ī�弱�ð����ϰ� �ؾ���
    // public void SelectCard(int index)
    // {
    //     if (GameManager.instance.userCharacter.IsState<CharacterIdleState>() ||
    //         GameManager.instance.userCharacter.IsState<CharacterWalkState>())
    //     {
    //         if (selectedCard != -1 && selectedCard < items.Count)
    //         {
    //             items[selectedCard].OnSelect(false); // ���� ī�� ���� ����
    //         }
    //         selectedCard = index;
    //         if (items.Count >= selectedCard)
    //         {
    //             var selectedItem = items[selectedCard];
    //             GameManager.instance.SelectedCard = selectedItem.cardData;
    //             selectedItem.OnSelect(true);
    //             Debug.Log("여기다!!");

    //             // 파티클 시스템 활성화 및 재생 
    //             // select.SetActive(true); 
    //             // GetComponent<ParticleSystem>().Play();
    //         }
    //     }            
    // }

    public void SelectCard(int index)
{
    if (GameManager.instance.userCharacter.IsState<CharacterIdleState>() ||
        GameManager.instance.userCharacter.IsState<CharacterWalkState>())
    {
        if (selectedCard != -1 && selectedCard < items.Count)
        {
            items[selectedCard].OnSelect(false); // 이전 카드 선택 해제

            // 이전에 선택된 카드의 y값을 원래대로 되돌리기
            var previousSelectedItem = items[selectedCard];
            previousSelectedItem.transform.position = new Vector3(
                previousSelectedItem.transform.position.x,
                previousSelectedItem.originalYPosition, // 원래 y값으로 되돌리기 위해 추가
                previousSelectedItem.transform.position.z);
        }

        selectedCard = index;
        if (items.Count > selectedCard)
        {
            var selectedItem = items[selectedCard];
            GameManager.instance.SelectedCard = selectedItem.cardData;
            selectedItem.OnSelect(true);
            Debug.Log("여기다!!");

            // 선택한 카드의 y값을 20만큼 올리기
            selectedItem.originalYPosition = selectedItem.transform.position.y; // 원래 y값 저장
            selectedItem.transform.position = new Vector3(
                selectedItem.transform.position.x,
                selectedItem.transform.position.y + 20f,
                selectedItem.transform.position.z);

            // 파티클 시스템 활성화 및 재생 
            // select.SetActive(true); 
            // GetComponent<ParticleSystem>().Play();
        }
    }            
}


    public void CardUse() 
    {
        if (selectedCard < 0 || selectedCard >= UserInfo.myInfo.handCards.Count)
        {
            Debug.LogWarning("CardUse: ���õ� ī�尡 ��ȿ���� �ʽ��ϴ�.");
            return; 
        }

        var card = UserInfo.myInfo.handCards[selectedCard]; // ���õ� ī�� ����]
        if (card.rcode == "CAD00005")
        {
            UIManager.ShowAlert("�������� ��� �Ͻðڽ��ϱ�?", "119 ȣ��", "������", "��ο���", () =>
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

    public void UseCard() // ī�带 ó����
    {
        if (selectedCard < 0 || selectedCard >= UserInfo.myInfo.handCards.Count)
        {
            Debug.LogWarning("UseCard: ���õ� ī�尡 ��ȿ���� �ʽ��ϴ�.");
            return; // ���õ� ī�尡 ��ȿ���� ������ ����
        }
        Debug.Log($"ī���� : {UserInfo.myInfo.OnUseCard(selectedCard)}");
        UserInfo.myInfo.OnUseCard(selectedCard);
   
        SetList();
        selectedCard = -1; // ���� �ʱ�ȭ

    }

    public void OnClickItem(CardDataSO data)
    {

    }
    void Update()
    {
        if (GameManager.instance.userCharacter == null)
            return;
        if (GameManager.instance.userCharacter.IsState<CharacterIdleState>() ||
            GameManager.instance.userCharacter.IsState<CharacterWalkState>())
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
}
