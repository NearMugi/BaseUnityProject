﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SerialConnect_Arduino : MonoBehaviour {

    #region Singleton

    private static SerialConnect_Arduino instance;

    public static SerialConnect_Arduino Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Arduino)FindObjectOfType(typeof(SerialConnect_Arduino));

                if (instance == null)
                {
                    Debug.LogError(typeof(SerialConnect_Arduino) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton


    /// <summary>
    /// Arduinoと紐づくSerialHandler.serial_unit
    /// </summary>
    SerialHandler.serial_unit _serial;
    Coroutine NowCoroutine;
    [SerializeField]
    int SerialListNo;   //SerialHandlerのリストと紐づく

    const byte endPoint = 0x09; //"\t"
    const byte splitPoint = 0x2c; //","

    //Arduinoからの１バイトデータ
    [Flags]
    public enum ReceiveCmd
    {
        flg_7 = 1 << 7,//
        flg_6 = 1 << 6,//
        flg_5 = 1 << 5,//
        flg_4 = 1 << 4,//
        flg_3 = 1 << 3,//
        flg_2 = 1 << 2,//
        flg_1 = 1 << 1,//
        flg_0 = 1,               //常にTrue
    };
    const int MAX_GETDATA_SIZE = 10;
    [HideInInspector]
    public ReceiveCmd[] GetData = new ReceiveCmd[MAX_GETDATA_SIZE];


    public string DebugList()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT ARDUINO INFO ---");
        sb.Append("\n");
        sb.Append("[GetData]");
        sb.Append("\n");
        foreach (ReceiveCmd cmd in GetData)
        {
            sb.Append(cmd);
            sb.Append("\n");
        }

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

    private void Start()
    {
        Connect();
    }

    public void Connect()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(ConnectCoroutine());
    }

    public IEnumerator ConnectCoroutine()
    {
        var wait = new WaitForSeconds(0.1f);
        yield return wait;

        _serial = SerialHandler.Instance.PortList[SerialListNo];   //SerialHandlerのリストと紐づく

        //USBの切断
        _serial.Close();
        _serial.OnDataReceived -= OnDataReceived;
        yield return wait;


        //USBの接続
        _serial.Open();
        _serial.OnDataReceived += OnDataReceived;

        yield break;

    }

    public void DataSend(string _s)
    {
        string _send = _s + endPoint;
        //DebugMsg("[DataSend] SendData ", _send);
        _serial.Write(_send);
    }


    //受信した信号(message)に対する処理
    void OnDataReceived(string[] message)
    {
        string joinMsg = string.Empty;
        foreach(string _t in message)
        {
            joinMsg += _t;
        }

        GetData = new ReceiveCmd[MAX_GETDATA_SIZE];

        byte[] _tmp = System.Text.Encoding.ASCII.GetBytes(joinMsg);
        byte data = 0x00;
        int i = 0;
        //1バイトのデータを取得する前提
        foreach(byte _b in _tmp)
        {
            switch (_b)
            {
                case 0x00:  //要らないデータを除去
                    break;
                case 0xFF:  //要らないデータを除去
                    break;
                case endPoint:  //区切り文字
                    GetData[i++] = (ReceiveCmd)data; 
                    break;
                case splitPoint:  //区切り文字
                    GetData[i++] = (ReceiveCmd)data;
                    break;

                default:
                    data = _b;
                    break;

            }
            if (i >= MAX_GETDATA_SIZE) break;
        }
        
    }


}