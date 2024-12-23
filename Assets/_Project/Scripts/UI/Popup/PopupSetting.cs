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
        Debug.Log("���� ����!");
        Application.Quit(); // ���� ����
    }

    public async void onClickConnectionFailed()
    {
        Debug.Log("�κ�ȭ������");
        await UIManager.Show<PopupLogin>();
    }
}