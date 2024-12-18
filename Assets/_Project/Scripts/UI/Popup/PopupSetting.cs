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

    public async void OnExitButton()
    {
        Debug.Log("게임 종료!");
        Application.Quit(); // 게임 종료
    }
}