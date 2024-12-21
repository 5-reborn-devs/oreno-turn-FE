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
        Debug.Log("���� ����!");
        Application.Quit(); // ���� ����
    }

    public void onClickConnectionFailed()
    {
        Debug.Log("�κ�ȭ������");
        UIManager.Show<PopupLogin>();
        UIManager.Hide<PopupConnectionFailed>();
        SocketManager.instance.HearBeatOut(true);
        
    }
}