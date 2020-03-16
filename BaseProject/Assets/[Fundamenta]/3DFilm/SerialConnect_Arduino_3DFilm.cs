using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SerialConnect_Arduino_3DFilm : MonoBehaviour
{

    #region Singleton

    private static SerialConnect_Arduino_3DFilm instance;

    public static SerialConnect_Arduino_3DFilm Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Arduino_3DFilm)FindObjectOfType(typeof(SerialConnect_Arduino_3DFilm));

                if (instance == null)
                {
                    //Debug.LogError(typeof(SerialConnect_Arduino_3DFilm) + " is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton


    /// <summary>
    /// Arduinoと紐づくSerialHandler.serial_unit
    /// </summary>
    [HideInInspector]
    public SerialHandler.serial_unit _serial;
    Coroutine NowCoroutine;
    [HideInInspector]
    public bool isConnect;
    [HideInInspector]
    public bool isAnalysis;

    private const byte STX = 0x02; //ヘッダー
    private const byte ETX = 0x03; //フッター

    private bool[] isOn = new bool[10];

    private const int MAXSIZE = 60;
    private byte[] joinMsg = new byte[MAXSIZE];
    private int cnt;

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
        sb.Append("--- 3DFilm ---");
        sb.Append("\n");

        sb.Append("[GetData]");
        sb.Append("\n");
        sb.Append(arrayMsg(joinMsg));
        sb.Append("\n");

        sb.Append("[ON/OFF]");
        sb.Append("\n");
        for (int i = 0; i < 10; i++)
        {
            sb.Append(i);
            sb.Append(":");
            sb.Append(isOn[i]);
            sb.Append(", ");
        }
        sb.Append("\n");

        return sb.ToString();
    }

    string arrayMsg(byte[] list)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (byte l in list)
        {
            sb.Append(l);
            sb.Append(", ");

        }
        return sb.ToString();
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
        _serial.OnDataReceivedByte -= OnDataReceivedByte;
        yield return wait;


        //USBの接続
        _serial.Open(false);
        _serial.OnDataReceivedByte += OnDataReceivedByte;
        cnt = 0;

        isConnect = true;
        yield break;

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
        //update処理内で解析中の場合は以下の処理を行わない
        if (isAnalysis) return;

        //STX, 10バイトデータ, ETX, CR, LFを受信する
        int pos = 0;
        bool isFind = false;
        for (int i = 0; i < MAXSIZE; i++)
        {
            if (joinMsg[i] == STX) pos = i; //pos = STX
            if (joinMsg[i] == ETX)
            {
                if (i - pos == 11)
                {
                    isFind = true;
                    for (int k = 0; k < 10; k++)
                    {
                        isOn[k] = false;
                        Debug.Log(joinMsg[pos + 1 + k]);
                        if (joinMsg[pos + 1 + k] == 49) isOn[k] = true;
                    }
                }
            }
        }

        //見つかったらリセット
        if (isFind)
        {
            cnt = 0;
            for (int i = 0; i < MAXSIZE; i++)
            {
                joinMsg[i] = 0x00;
            }
        }
    }


}
