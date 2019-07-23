using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Bluetooth接続
/// </summary>
public class SerialConnect_BlueTooth : SerialConnect_BlueTooth_Base
{

    #region Singleton

    private static SerialConnect_BlueTooth instance;

    public static SerialConnect_BlueTooth Instance_BlueTooth
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_BlueTooth)FindObjectOfType(typeof(SerialConnect_BlueTooth));                
            }
            return instance;
        }
    }

    #endregion Singleton


    const int dataSize = 9; //nnn,nn,nn
    const int cmdSize = 3;
    string[] cmd = new string[cmdSize];

    public new string DebugList()
    {
        if (_serial == null) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT BLUETOOTH INFO ---  ");
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
    

    private void Start()
    {
    }

    private void Update()
    {
        isAnalysis = true;

        //接続できていない場合は何もしない。
        if (!isConnect) { isAnalysis = false; return; }


        //入力値を解析する
        cmd = new string[cmdSize];
        analisysGetData(GetLastData);

        isAnalysis = false;


        DataSend("Hoge HOGE");

    }

    void analisysGetData(string data)
    {
        if (data == null) return;
        if (data.Length != dataSize) return;

        string[] _tmp = data.Split(splitPoint);
        if (_tmp.Length != cmdSize) return;
        int i = 0;
        foreach(string d in _tmp)
        {
            cmd[i++] = d;
        }
    }



}
