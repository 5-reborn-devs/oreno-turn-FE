using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PopupSetting : UIBase
{
    public override void Opened(object[] param)
    {
        
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupSetting>();
    }

    public void OnClickButton()
    {
        Debug.Log("게임 종료!");
        Application.Quit(); // 게임 종료
    }

    public async void onClickConnectionFailed()
    {
        Debug.Log("로비화면으로");
        await UIManager.Show<PopupLogin>();
    }
}