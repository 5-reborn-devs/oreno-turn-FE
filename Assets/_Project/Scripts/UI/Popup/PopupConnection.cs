using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;

public class PopupConnection : UIBase
{
    [SerializeField] private TMP_InputField ip;
    [SerializeField] private TMP_InputField port;

    public override void Opened(object[] param)
    {
        ip.text = PlayerPrefs.GetString("ip", "15.165.168.120");
        port.text = PlayerPrefs.GetString("port", "9000");
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupConnection>();
    }

    public void OnClickConnection()
    {
        if (string.IsNullOrEmpty(ip.text)) ip.text = "15.165.168.120";
        if (string.IsNullOrEmpty(port.text)) port.text = "9000";
        PlayerPrefs.SetString("ip", ip.text);
        PlayerPrefs.SetString("port", port.text);
        if (SocketManager.instance.isConnected)
        {
            SocketManager.instance.Disconnect();
        }
        SocketManager.instance.Init(ip.text, int.Parse(port.text));
        SocketManager.instance.Connect();
        HideDirect();
    }

    public void OnClickClose()
    {
        var ip = PlayerPrefs.GetString("ip");
        var port = PlayerPrefs.GetString("port");
        if (SocketManager.instance.isConnected)
        {
            SocketManager.instance.Disconnect();
        }
        SocketManager.instance.Init(ip, int.Parse(port));
        SocketManager.instance.Connect();
        HideDirect();
    }
}