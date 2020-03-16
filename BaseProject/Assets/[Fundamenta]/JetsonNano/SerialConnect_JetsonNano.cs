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
                    //Debug.LogError(typeof(SerialConnect_JetsonNano) + " is nothing");
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


    private const int MAXSIZE = 60;
    private byte[] joinMsg = new byte[MAXSIZE];
    private int cnt;
    private string msg;


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
        sb.Append("--- CONNECT JETSON NANO INFO ---");
        sb.Append("\n");
        sb.Append("[GetData]");
        sb.Append("\n");
        sb.Append(msg);
        sb.Append("\n");

        return sb.ToString();
    }

    string getJoinMsg(byte[] list)
    {
        return System.Text.Encoding.ASCII.GetString(list);
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
        yield return wait;


        //USBの接続
        _serial.Open(false); //byte[]型で受信
        _serial.OnDataReceivedByte += OnDataReceivedByte;
        cnt = 0;
        msg = string.Empty;

        isConnect = true;
        yield break;

    }

    public void DataSend(string _s)
    {
        if (_serial == null) return;
        string _send = _s; // + (char)endPoint;
        Debug.Log("[DataSend] SendData " + _send);

        //byte[]型に変換して送信
        _serial.WriteByte(_s);
    }


    //受信した信号(message)に対する処理
    void OnDataReceivedByte(byte[] message)
    {
        //メッセージを保存
        foreach (byte _t in message)
        {
            if (_t != 0x00) joinMsg[cnt++] = _t;
            if (cnt >= MAXSIZE) cnt = 0;
        }
    }
    void analisysGetData()
    {
        msg = getJoinMsg(joinMsg);
    }

    private void Start()
    {
    }

    private void Update()
    {
        //接続できていない場合は何もしない。
        if (!isConnect) { isAnalysis = false; return; }

        isAnalysis = true;

        //入力値を解析する
        analisysGetData();

        isAnalysis = false;
    }


    private void OnDestroy()
    {
        if(isConnect) Disconnect();
    }

}
