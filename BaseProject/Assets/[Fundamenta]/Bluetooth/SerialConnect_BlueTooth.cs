using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Bluetooth接続
/// </summary>
public class SerialConnect_BlueTooth : SerialConnect_Arduino_Base
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


    public new string DebugList()
    {
        if (_serial == null) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT BLUETOOTH INFO ---  ");
        sb.Append("\n");

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
        foreach (string _d in GetData) analisysGetData(_d);

        isAnalysis = false;

    }

    void analisysGetData(string data)
    {
        if (data == null) return;
        if (data.Length <= 0) return;
        
        Debug.LogWarning(data);        
    }



}
