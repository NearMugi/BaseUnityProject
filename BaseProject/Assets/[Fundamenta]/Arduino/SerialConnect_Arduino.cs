using System;
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

    const string endPoint = "\t";
    const string splitPoint = ",";

    string stockdata;   //受信したデータを保存する変数


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

    public void Connect()
    {
        SerialHandler_Arduino.Instance.OnDataReceived -= OnDataReceived;
        //USBの切断・接続
        SerialHandler_Arduino.Instance.ReConnect();
        //信号を受信したときに、そのメッセージの処理を行う
        SerialHandler_Arduino.Instance.OnDataReceived += OnDataReceived;
    }


    public void DataSend(string _s)
    {
        string _send = _s + endPoint;
        //DebugMsg("[DataSend] SendData ", _send);
        SerialHandler_Arduino.Instance.Write(_send);
    }


    //受信した信号(message)に対する処理
    void OnDataReceived(string message)
    {
        //今回はArduinoの状態だけを取得するので、
        //一番最後のendPointを見つけてその前の１バイトだけを使う。
        //あとは全て捨てる。

        //DebugMsg("[OnDataReceived] message       ", message);
        stockdata += message;
        //DebugMsg("[OnDataReceived] stockdata_bef ", stockdata);

        int _pos = stockdata.LastIndexOf(endPoint); //末尾から探す
        //Debug.LogWarning("[OnDataReceived] _pos{" + _pos +"}");
        if (_pos > 0)
        {
            try
            {
                byte[] _tmp = System.Text.Encoding.ASCII.GetBytes(stockdata);
                SerialConnect_Arduino_Air.Instance.GetData = (SerialConnect_Arduino_Air.ReceiveCmd)_tmp[_pos - 1];
                //Debug.LogWarning("[SerialConnect_Valve] GetData " + GetData + "  <-  Hex:" + Convert.ToString(_tmp[_pos - 1], 16));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }

            stockdata = string.Empty;
        }

        //DebugMsg("[OnDataReceived] stockdata_aft ", stockdata);


    }


}
