using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SerialConnect_Arduino_PotentioMeter : SerialConnect_Arduino_Base
{

    #region Singleton

    private static SerialConnect_Arduino_PotentioMeter instance;

    public static SerialConnect_Arduino_PotentioMeter Instance_PotentioMeter
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Arduino_PotentioMeter)FindObjectOfType(typeof(SerialConnect_Arduino_PotentioMeter));
            }
            return instance;
        }
    }

    #endregion Singleton


    //Arduinoからのデータ構成
    //「ヘッダー,整数部分,小数部分」の3バイトになっている。
    //2つのポテンショメータから値を取得する。

    //OnDataReceivedではタブ区切りでデータを取得する。
    //Update内で取得したデータをカンマ区切りで分ける。
    //正常値である場合、入力値として扱う。


    //入力値の調整用　最大値のチェックと最小値を丸める
    const float MAX_VALUE = 10.0f;
    const float MIN_VALUE = 1.0f;
    [HideInInspector]
    public float[] Pos = new float[2];     //各ポテンショメータの値


    public new string DebugList()
    {
        if (_serial == null) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT POTENTIOMETER INFO ---");
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

    private void Start()
    {
        isConnect = false;
    }

    private void Update()
    {
        isAnalysis = true;
        foreach(string _d in GetData) analisysGetData(_d);
        isAnalysis = false;
    }


    bool chkHeader(ReceiveCmd cmd) 
    {
        bool sw = false;
        bool isflg0 = ReceiveCmd.flg_0 == (cmd & ReceiveCmd.flg_0);
        bool isflg1 = ReceiveCmd.flg_1 == (cmd & ReceiveCmd.flg_1);
        bool isflg2 = ReceiveCmd.flg_2 == (cmd & ReceiveCmd.flg_2);
        bool isflg3 = ReceiveCmd.flg_3 == (cmd & ReceiveCmd.flg_3);
        bool isflg4 = ReceiveCmd.flg_4 == (cmd & ReceiveCmd.flg_4);
        bool isflg7 = ReceiveCmd.flg_7 == (cmd & ReceiveCmd.flg_7);

        //Debug.LogWarning("[chkHeader]" + cmd);

        if (isflg0 && isflg1 && isflg2 && isflg3 && isflg4 && isflg7)
        {
            sw = true;
        }

        return sw;
    }

    void analisysGetData(string data)
    {
        if (data == null) return;
        if (data.Length <= 0) return;


        //カンマ区切りでデータを分ける。
        //3バイトでない場合は何もしない。
        string[] Onebyte = data.Split(splitPoint);

        if (Onebyte.Length != 3) return;

        //Debug.LogWarning("[Onebyte]" + Onebyte[0] + " : " + Onebyte[1] + " : " + Onebyte[2]);

        //---1バイト目---
        //0～4,7ビットを見て1バイト目のデータだと判断する
        //flg_7 : true
        //flg_6 : P1(false) or P2(true)
        //flg_5 : プラス(false) or マイナス(true)
        //flg_4～_0 : true

        //---2バイト目---
        //flg_7 : false
        //flg_6～_0 : 整数(0～99)

        //---3バイト目---
        //flg_7 : false
        //flg_6～_0 : 小数(0～99)


        //ヘッダーの判定
        try
        {
            ReceiveCmd cmd = (ReceiveCmd)(int.Parse(Onebyte[0]));
            //Debug.LogWarning("cmd : " + int.Parse(Onebyte[0]) + "  ->  " + cmd);


            if (!chkHeader(cmd)) return;

            //ヘッダーから必要な情報を取得する
            bool isflg5 = ReceiveCmd.flg_5 == (cmd & ReceiveCmd.flg_5);
            bool isflg6 = ReceiveCmd.flg_6 == (cmd & ReceiveCmd.flg_6);

            int _no = 0;    //上下or左右
            if (isflg6) _no = 1;
            int isMinus = 1;
            if (isflg5) isMinus = -1;

            //整数部分の取得
            float _v = int.Parse(Onebyte[1]);
            if (_v > MAX_VALUE) return;    //最大値を超えている場合は抜ける


            //小数部分の取得
            float _v2 = int.Parse(Onebyte[2]);

            _v2 /= 100.0f;
            _v += _v2;

            //最大値を超えないようにする
            if (_v > MAX_VALUE) _v = MAX_VALUE;

            //最小値を丸める
            if (_v < MIN_VALUE) _v = 0.0f;

            _v *= isMinus;

            Pos[_no] = _v;
            //if(_no == 0) Debug.LogWarning(EventTrigger.Instance.Pos[_no]);

        }
        catch (Exception)
        {
            return;
        }
    }

}
