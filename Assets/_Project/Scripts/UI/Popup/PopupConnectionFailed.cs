using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PopupConnectionFailed : UIBase
{
    public override void Opened(object[] param)
    {
        
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupSetting>();
    }

    public async void OnClickButton()
    {
        Debug.Log("게임 종료!");
        Application.Quit(); // 게임 종료
    }

    public void onClickConnectionFailed()
    {
        Debug.Log("로비화면으로");
        UIManager.Show<PopupLogin>();
        UIManager.Hide<PopupConnectionFailed>();
        SocketManager.instance.HearBeatOut(true);
        
    }
}