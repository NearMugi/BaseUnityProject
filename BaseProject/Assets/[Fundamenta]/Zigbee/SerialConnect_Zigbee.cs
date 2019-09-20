using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SerialConnect_Zigbee : MonoBehaviour
{

    #region Singleton

    private static SerialConnect_Zigbee instance;

    public static SerialConnect_Zigbee Instance_Zigbee
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Zigbee)FindObjectOfType(typeof(SerialConnect_Zigbee));

                if (instance == null)
                {
                    Debug.LogError(typeof(SerialConnect_Zigbee) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton


    /// <summary>
    /// SerialHandler.serial_unit
    /// </summary>
    [HideInInspector]
    public SerialHandler.serial_unit _serial;
    Coroutine NowCoroutine;
    [HideInInspector]
    public bool isConnect;
    [HideInInspector]
    public bool isAnalysis;
    string GetData;

    int[] dice = new int[2];
    int battery;

    /// <summary>
    /// シリアル通信が出来ているかチェック
    /// </summary>
    /// <returns></returns>
    public bool GetisConnect()
    {
        return isConnect;
    }

    public string DebugList()
    {
        if (_serial == null) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT ZIGBEE INFO ---");
        sb.Append("\n");

        sb.Append("[DICE]");
        sb.Append("\n (ID, Number) : (");
        sb.Append(dice[0]);
        sb.Append(", ");
        sb.Append(dice[1]);
        sb.Append(")");
        sb.Append("\n");

        sb.Append("Battery");
        sb.Append("\n");
        sb.Append(battery);
        sb.Append("\n");

        sb.Append("[GetData]");
        sb.Append("\n");
        sb.Append(GetData);
        sb.Append("\n");

        return sb.ToString();
    }

    void DebugMsg(string head, string _s)
    {
        string msg = head + _s;

        byte[] _tmp = System.Text.Encoding.ASCII.GetBytes(_s);
        msg += " Length:" + _tmp.Length;

        msg += " Hex:";
        foreach (byte b in _tmp)
        {
            msg += Convert.ToString(b, 16) + " - ";
        }

        Debug.LogWarning(msg);
    }


    public void StartInit()
    {
        _serial = null;
        isConnect = false;
    }

    public void Connect()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(ConnectCoroutine());
    }

    private IEnumerator ConnectCoroutine()
    {
        var wait = new WaitForSeconds(0.1f);
        isConnect = false;
        yield return wait;

        SerialPortName _sp = GetComponent<SerialPortName>();
        if (_sp == null) yield break;
        _serial = SerialHandler.Instance.PortList[_sp.SerialListNo];   //SerialHandlerのリストと紐づく

        //USBの切断
        _serial.Close();
        _serial.OnDataReceived -= OnDataReceived;
        yield return wait;


        //USBの接続
        _serial.Open(true);
        _serial.OnDataReceived += OnDataReceived;

        isConnect = true;
        yield break;

    }

    public void DataSend(string _s)
    {
        if (_serial == null) return;
        //DebugMsg("[DataSend] SendData ", _send);
        _serial.Write(_s);
    }


    //受信した信号(message)に対する処理
    void OnDataReceived(string[] message)
    {
        //今回受信するデータの形式
        //::rc=80000000:lq=189:ct=006D:ed=810D33EA:id=3:ba=2490:a1=1237:a2=0576:x=0001:y=0000:z=0000
        //欲しい情報はIDとxの値だけなので、そのペアを見つける。
        //子機電源電圧も取得する。

        //メッセージを保存
        GetData = string.Empty;
        foreach (string _t in message)
        {
            GetData += _t;
        }

        //update処理内で解析中の場合は以下の処理を行わない
        if (isAnalysis) return;

        int i = GetData.LastIndexOf(":id="); //一番後ろ(最新)のIDを取得する
        int j = GetData.LastIndexOf(":x="); //一番後ろ(最新)のxを取得する
        if (i > 0 && i < j)
        {
            dice[0] = (char)GetData[i + 4] - 48; //0のasciiコードが48
            dice[1] = (char)GetData[j + 6] - 48;
        }

        i = GetData.LastIndexOf(":ba=");
        j = GetData.LastIndexOf(":a1=");
        if (i > 0 && i < j)
        {
            string ba = GetData.Substring(i + 4, j - (i + 4));
            battery = Int16.Parse(ba);
        }
    }


}
