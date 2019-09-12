using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// JetsonNanoとシリアル接続
/// </summary>
public class SerialConnect_JetsonNano : MonoBehaviour
{

    #region Singleton

    private static SerialConnect_JetsonNano instance;

    public static SerialConnect_JetsonNano Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_JetsonNano)FindObjectOfType(typeof(SerialConnect_JetsonNano));

                if (instance == null)
                {
                    Debug.LogError(typeof(SerialConnect_JetsonNano) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton


    /// <summary>
    /// JetsonNanoと紐づくSerialHandler.serial_unit
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


    const int dataSize = 9; //nnn,nn,nn
    const int cmdSize = 3;
    string[] cmd = new string[cmdSize];

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
        sb.Append("--- CONNECT JETSON NANO INFO ---  ");
        sb.Append("\n");
        sb.Append("[GetLastData]");
        sb.Append("\n");
        sb.Append(cmd[0]);
        sb.Append("\n");
        sb.Append(cmd[1]);
        sb.Append("\n");
        sb.Append(cmd[2]);
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
        _serial.OnDataReceivedByte -= OnDataReceivedByte;
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
        _serial.OnDataReceivedByte -= OnDataReceivedByte;
        _serial.OnDataReceived -= OnDataReceived;
        yield return wait;


        //USBの接続
        joinMsg = string.Empty;
        _serial.Open(true);
        _serial.OnDataReceivedByte += OnDataReceivedByte;
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
    void OnDataReceivedByte(byte[] message)
    {
        Debug.Log("OnDataReceivedByte");

        //メッセージを保存
        foreach (byte _t in message)
        {
            Debug.Log(_t);
        }
    }

    //受信した信号(message)に対する処理
    void OnDataReceived(string[] message)
    {
        Debug.Log("OnDataReceived");

        //メッセージを保存
        foreach (string _t in message)
        {
            Debug.Log(_t);
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        isAnalysis = true;

        Debug.Log(isConnect);

        //接続できていない場合は何もしない。
        if (!isConnect) { isAnalysis = false; return; }


        //入力値を解析する
        //cmd = new string[cmdSize];
        //analisysGetData(GetLastData);

        isAnalysis = false;


       // DataSend("Hoge HOGE");

    }

    void analisysGetData(string data)
    {
        if (data == null) return;
        if (data.Length != dataSize) return;

        string[] _tmp = data.Split(splitPoint);
        if (_tmp.Length != cmdSize) return;
        int i = 0;
        foreach (string d in _tmp)
        {
            cmd[i++] = d;
        }
    }
}
