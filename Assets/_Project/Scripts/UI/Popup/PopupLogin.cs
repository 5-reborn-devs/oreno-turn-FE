using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using Unity.Multiplayer.Playmode;

public class PopupLogin : UIBase
{
    [SerializeField] private GameObject touch;
    [SerializeField] private GameObject buttonSet;
    [SerializeField] private GameObject register;
    [SerializeField] private GameObject login;
    [SerializeField] private TMP_InputField loginId;
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private TMP_InputField regId;
    [SerializeField] private TMP_InputField regNickname;
    [SerializeField] private TMP_InputField regPassword;
    [SerializeField] private TMP_InputField regPasswordRe;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    public override void Opened(object[] param)
    {
        register.SetActive(false);
        login.SetActive(false);

        var tags = CurrentPlayer.ReadOnlyTags();
        if (tags.Length == 0)
        {
            tags = new string[1] { "player1" };
        }
        loginId.text = PlayerPrefs.GetString("id" + tags[0], "");
        loginPassword.text = PlayerPrefs.GetString("password" + tags[0], "");
    }

    public override void HideDirect()
    {
        ClickSound();
        UIManager.Hide<PopupLogin>();
    }

    public void ClickSound()
    {       
       audioSource.PlayOneShot(clickSound);
    }
    public void OnClickLogin()
    {
        ClickSound();
        if (!SocketManager.instance.isConnected)
        {
            //var ip = PlayerPrefs.GetString("ip", "127.0.0.1");
            //var port = PlayerPrefs.GetString("port", "9000");
            var ip = "3.36.19.101";
            var port = "9000";
            SocketManager.instance.Init(ip, int.Parse(port));
            SocketManager.instance.Connect(() =>
            {
                buttonSet.SetActive(false);
                login.SetActive(true);

            });
        }
        else
        {
            buttonSet.SetActive(false);
            login.SetActive(true);
        }
    }

    public void OnClickRegister()
    {
        ClickSound();
        if (!SocketManager.instance.isConnected)
        {
            var ip = "3.36.19.101";
            var port = "9000";
            SocketManager.instance.Init(ip, int.Parse(port));
            SocketManager.instance.Connect(() =>
            {
                buttonSet.SetActive(false);
                register.SetActive(true);
            });
        }
        else
        {
            buttonSet.SetActive(false);
            register.SetActive(true);
        }
    }

    public void OnClickSendLogin()
    {
        ClickSound();
        GamePacket packet = new GamePacket();
        packet.LoginRequest = new C2SLoginRequest() { Email = loginId.text, Password = loginPassword.text };
        var tags = CurrentPlayer.ReadOnlyTags();
        if(tags.Length == 0)
        {
            tags = new string[1] { "player1" };
        }
        PlayerPrefs.SetString("id" + tags[0], loginId.text);
        PlayerPrefs.SetString("password" + tags[0], loginPassword.text);
        SocketManager.instance.Send(packet);
        //OnLoginEnd(true);
    }

    public void OnClickSendRegister()
    {
        ClickSound();
        if (regPassword.text != regPasswordRe.text)
        {
            UIManager.ShowAlert("비밀번호 확인이 일치하지 않습니다.");
            return;
        }
        GamePacket packet = new GamePacket();
        packet.RegisterRequest = new C2SRegisterRequest() { Nickname = regNickname.text, Email = regId.text, Password = regPassword.text };
        var tags = CurrentPlayer.ReadOnlyTags();
        if (tags.Length == 0)
        {
            tags = new string[1] { "player1" };
        }
        PlayerPrefs.SetString("id" + tags[0], regId.text);
        PlayerPrefs.SetString("password" + tags[0], regPassword.text);
        SocketManager.instance.Send(packet);
    }

    public void OnClickCancelRegister()
    {
        ClickSound();
        buttonSet.SetActive(true);
        register.SetActive(false);
    }

    public void OnClickCancelLogin()
    {
        ClickSound();
        buttonSet.SetActive(true);
        login.SetActive(false);
    }

    public void OnTouchScreen()
    {
        touch.SetActive(false);
        buttonSet.SetActive(false);
    }

    public async void OnLoginEnd(bool isSuccess, GlobalFailCode failCode, string message)
    {
        ClickSound();
        if (isSuccess)
        {
            await UIManager.Show<UIMain>();
            UIManager.Get<UIMain>().OnRefreshRoomList();
            HideDirect();
            await UIManager.Show<UITopBar>();
            await UIManager.Show<UIGnb>();
        }
        else
        {
            UIManager.ShowAlert(message);
        }
    }

    public void OnRegisterEnd(bool isSuccess, GlobalFailCode failCode, string message)
    {
        ClickSound();
        if (isSuccess)
        {
            register.SetActive(false);
            login.SetActive(true);
            var tags = CurrentPlayer.ReadOnlyTags();
            if (tags.Length == 0)
            {
                tags = new string[1] { "player1" };
            }
            loginId.text = PlayerPrefs.GetString("id" + tags[0]);
            loginPassword.text = PlayerPrefs.GetString("password" + tags[0]);
        }
     else
        {
            UIManager.ShowAlert(message);
        }
    }

    public void OnClickChangeServer()
    {
        ClickSound();
        UIManager.Show<PopupConnection>();
    }
}