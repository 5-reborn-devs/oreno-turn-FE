using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIMain : UIListBase<ItemRoom>
{
    float time = 0;
    List<RoomData> rooms;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    public override void Opened(object[] param)
    {

    }

    private void Update()
    {
        time += Time.deltaTime;
        if(time > 5)
        {
            time = 0;
            OnRefreshRoomList();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Show<PopupSetting>();
        }
    }

    public void SetRoomList(List<RoomData> rooms)
    {
        this.rooms = rooms;
        SetList();
    }
    public void ClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }
    public void OnRefreshRoomList()
    {
        if (SocketManager.instance.isConnected)
        {
            GamePacket packet = new GamePacket();
            packet.GetRoomListRequest = new C2SGetRoomListRequest();
            SocketManager.instance.Send(packet);
        }
    }

    public override void HideDirect()
    {
        ClickSound();
        UIManager.Hide<UIMain>();
    }

    public override void SetList()
    {
        ClearList();
        for (int i = 0; i < rooms.Count; i++)
        {
            var item = AddItem();
            item.SetItem(rooms[i], OnJoinRoom);
        }
    }

    public void OnClickRandomMatch()
    {
        ClickSound();
        if (SocketManager.instance.isConnected)
        {
            GamePacket packet = new GamePacket();
            packet.JoinRandomRoomRequest = new C2SJoinRandomRoomRequest();
            SocketManager.instance.Send(packet);
        }
    }

    public void OnClickCreateRoom()
    {
        ClickSound();
        UIManager.Show<PopupRoomCreate>();
    }

    public void OnJoinRoom(int idx)
    {
        ClickSound();
        if (SocketManager.instance.isConnected)
        {
            GamePacket packet = new GamePacket();
            packet.JoinRoomRequest = new C2SJoinRoomRequest() { RoomId = idx };
            SocketManager.instance.Send(packet);
        }
    }

}