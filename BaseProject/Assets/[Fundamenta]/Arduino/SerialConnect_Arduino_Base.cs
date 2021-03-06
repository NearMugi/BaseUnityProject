﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SerialConnect_Arduino_Base : MonoBehaviour {

    #region Singleton

    private static SerialConnect_Arduino_Base instance;

    public static SerialConnect_Arduino_Base Instance {
        get {
            if (instance == null) {
                instance = (SerialConnect_Arduino_Base) FindObjectOfType (typeof (SerialConnect_Arduino_Base));

                if (instance == null) {
                    Debug.LogError (typeof (SerialConnect_Arduino_Base) + "is nothing");
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

    public const byte endPoint = 0x09; //"\t"
    public const char splitPoint = ',';

    //Arduinoからの１バイトデータ
    [Flags]
    public enum ReceiveCmd {
        flg_7 = 1 << 7, //
        flg_6 = 1 << 6, //
        flg_5 = 1 << 5, //
        flg_4 = 1 << 4, //
        flg_3 = 1 << 3, //
        flg_2 = 1 << 2, //
        flg_1 = 1 << 1, //
        flg_0 = 1,
    }
    public const int MAX_GETDATA_SIZE = 10;
    [HideInInspector]
    public string[] GetData = new string[MAX_GETDATA_SIZE];
    [HideInInspector]
    public int GetDataSize;

    const int MAX_WAITCNT = 2;
    int WaitCnt;
    string joinMsg;

    /// <summary>
    /// シリアル通信が出来ているかチェック
    /// </summary>
    /// <returns></returns>
    public bool GetisConnect () {
        return isConnect;
    }

    public string DebugList () {
        if (_serial == null) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder ();
        sb.Append ("--- CONNECT ARDUINO INFO ---");
        sb.Append ("\n");
        sb.Append ("[GetData]");
        sb.Append ("\n");
        foreach (string cmd in GetData) {
            sb.Append (cmd);
            sb.Append ("\n");
        }

        return sb.ToString ();
    }

    void DebugMsg (string head, string _s) {
        string msg = head + _s;

        byte[] _tmp = System.Text.Encoding.ASCII.GetBytes (_s);
        msg += " Length:" + _tmp.Length;

        msg += " Hex:";
        foreach (byte b in _tmp) {
            msg += Convert.ToString (b, 16) + " - ";
        }

        Debug.LogWarning (msg);
    }

    public void StartInit () {
        _serial = null;
        WaitCnt = 0;
        isConnect = false;
    }

    public void Connect () {
        if (NowCoroutine != null) StopCoroutine (NowCoroutine);
        NowCoroutine = StartCoroutine (ConnectCoroutine ());
    }

    private IEnumerator ConnectCoroutine () {
        var wait = new WaitForSeconds (0.1f);
        isConnect = false;
        yield return wait;

        SerialPortName _sp = GetComponent<SerialPortName> ();
        if (_sp == null) yield break;
        _serial = SerialHandler.Instance.PortList[_sp.SerialListNo]; //SerialHandlerのリストと紐づく

        //USBの切断
        _serial.Close ();
        _serial.OnDataReceived -= OnDataReceived;
        yield return wait;

        //USBの接続
        joinMsg = string.Empty;
        if (_serial.Open (true)) {
            _serial.OnDataReceived += OnDataReceived;
            isConnect = true;
        }
        yield break;

    }

    public void DataSend (string _s) {
        if (_serial == null) return;
        string _send = _s + (char) endPoint;
        //DebugMsg("[DataSend] SendData ", _send);
        _serial.Write (_send);
    }

    //受信した信号(message)に対する処理
    void OnDataReceived (string[] message) {
        //メッセージを保存
        foreach (string _t in message) {
            joinMsg += _t;
        }

        //大きなメッセージを受信することを想定して、何回か連結させる
        if (++WaitCnt <= MAX_WAITCNT) return;
        WaitCnt = 0;

        //Debug.LogWarning(joinMsg);

        //update処理内で解析中の場合は以下の処理を行わない
        if (isAnalysis) return;

        GetData = new String[MAX_GETDATA_SIZE];

        byte[] _tmp = System.Text.Encoding.ASCII.GetBytes (joinMsg);

        int _maxSaveSize = 50; //例えば「255」のデータはbyteで3桁になる。大きめに取っておく。
        byte[] saveData = new byte[_maxSaveSize];
        int j = 0;

        int i = 0;
        //1バイトのデータを取得する前提
        foreach (byte _b in _tmp) {
            //Debug.LogWarning("_b " + _b);
            switch (_b) {
                case endPoint: //区切り文字
                    GetData[i++] = System.Text.Encoding.ASCII.GetString (saveData);
                    //Debug.LogWarning("[Hit] " + GetData[i - 1]);
                    saveData = new byte[_maxSaveSize];
                    j = 0;
                    break;
                default:
                    saveData[j++] = _b;
                    break;

            }
            if (j > _maxSaveSize - 1) {
                saveData = new byte[_maxSaveSize];
                j = 0;
            }
            if (i >= MAX_GETDATA_SIZE) break;
        }

        //一つでも条件に合うデータを取得出来たら、連結データを削除する
        if (GetData.Length > 0) {
            joinMsg = string.Empty;
        }
    }

}