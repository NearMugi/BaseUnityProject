using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SerialConnect_BlueTooth_Base : MonoBehaviour {

    #region Singleton

    private static SerialConnect_BlueTooth_Base instance;

    public static SerialConnect_BlueTooth_Base Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_BlueTooth_Base)FindObjectOfType(typeof(SerialConnect_BlueTooth_Base));

                if (instance == null)
                {
                    Debug.LogError(typeof(SerialConnect_BlueTooth_Base) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton


    /// <summary>
    /// Bluetoothと紐づくSerialHandler.serial_unit
    /// </summary>
    [HideInInspector]
    public SerialHandler.serial_unit _serial;
    Coroutine NowCoroutine;
    [HideInInspector]
    public bool isConnect;
    [HideInInspector]
    public bool isAnalysis;

    public const char endPoint = '@';
    public const char splitPoint = ',';
    
    [HideInInspector]
    public string[] GetData;
    [HideInInspector]
    public string GetLastData;  //最新受信データ

    string joinMsg;

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
        sb.Append("--- CONNECT BLUETOOTH INFO ---");
        sb.Append("\n");
        sb.Append("[GetData]");
        sb.Append("\n");
        foreach (string cmd in GetData)
        {
            sb.Append(cmd);
            sb.Append("\n");
        }

        return sb.ToString();
    }

    string DebugMsg(string head, string _s)
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
        return msg;
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

    public void Disconnect()
    {
        _serial.Close();
        _serial.OnDataReceived -= OnDataReceived;
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
        joinMsg = string.Empty;
        _serial.Open();
        _serial.OnDataReceived += OnDataReceived;

        isConnect = true;
        yield break;

    }

    public void DataSend(string _s)
    {
        if (_serial == null) return;
        string _send = _s + (char)endPoint;
        //DebugMsg("[DataSend] SendData ", _send);
        _serial.Write(_send);
    }


    //受信した信号(message)に対する処理
    void OnDataReceived(string[] message)
    {
        //メッセージを保存
        foreach (string _t in message)
        {
            joinMsg += _t;
        }
        if (joinMsg.Length <= 0) return;
        //Debug.LogWarning(joinMsg);

        //update処理内で解析中の場合は以下の処理を行わない
        if (isAnalysis) return;

        //endPoint区切りのデータを格納している。
        //a,b,c@1,2,3@ -> GetData[0] = a,b,c GetData[1] = 1,2,3
        //最後に取得したデータをGetLastDataに保存
        GetData = joinMsg.Split(endPoint);
        GetLastData = GetData[GetData.Length - 2];
        //Debug.Log(joinMsg + "  ->  " + GetLastData);
        //DebugMsg("", joinMsg);

        //一つでも条件に合うデータを取得出来たら、連結データを削除する
        if (GetData.Length > 0)
        {
            joinMsg = string.Empty;
        }
    }


}
