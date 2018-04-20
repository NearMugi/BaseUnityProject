using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SerialConnect_Arduino_Air : SerialConnect_Arduino_Base
{

    #region Singleton

    private static SerialConnect_Arduino_Air instance;

    public static SerialConnect_Arduino_Air Instance_Air
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Arduino_Air)FindObjectOfType(typeof(SerialConnect_Arduino_Air));                
            }
            return instance;
        }
    }

    #endregion Singleton

    [HideInInspector]
    public float Def_OneTime { get; private set; } //基準値　1サイクルの時間(ms)
    [HideInInspector]
    public float Def_OneTime_On { get; private set; } //基準値　1サイクルのOn時間(ms)
    [HideInInspector]
    public int Def_CycleCnt { get; private set; }//基準値　1サイクルを実行する回数(回)

    //設定値の制限
    const float MIN_UP = 3.5f;   //パルス立ち上がりに要する時間(ms)
    const float MIN_DOWN = 1.5f; //パルス立ち下がりに要する時間(ms)

    const float ONETIME_MIN = 5.0f;    //1サイクルの時間(ms)
    const float ONETIME_MAX = 5000.0f;

    const int CYCLECNT_MIN = 1;   //1サイクルを実行する回数
    const int CYCLECNT_MAX = 100;

    Coroutine NowCoroutine;

    //コマンドの定義
    public enum CMD_TYPE
    {
        NONE,
        VALVE,        //バルブ実行・停止
        PUMP,         //ポンプ実行・停止
        ONETIME_INT,  //1サイクルの時間(ms)　※整数部分
        ONETIME_DEC,  //1サイクルの時間(ms)　※小数部分 2位まで
        ONETIME_ON,   //1サイクルのOn時間(ms)  ※1桁目は小数点第1位
        CYCLECNT,     //1サイクルを実行する回数(回)
        ONETIME_RESET,//1サイクルの時間を初期化

        PHOTO_SENSOR, //光電センサモードON/OFF

        TERMINAL,   //
    }

    const int CmdLength = 6;    //コマンドの長さ(終点を除く文字列)

    const string cmd_A0 = "A0"; //0x0000　バルブ実行・0x0001　ポンプ実行・0x0002　光電センサモードON
    const string cmd_80 = "80"; //0x0000  バルブ停止・0x0001　ポンプ停止・0x0002　光電センサモードOFF
    const string cmd_B0 = "B0"; //0x0000～0xFFFF　1サイクルの時間(ms) ※整数部分
    const string cmd_B1 = "B1"; //0x0000～0xFFFF　1サイクルの時間(ms) ※小数部分 2位まで
    const string cmd_B2 = "B2"; //0x0000～0xFFFF　1サイクルのOn時間(ms) ※1桁目は小数点第1位
    const string cmd_B3 = "B3"; //0x0000～0xFFFF　1サイクルを実行する回数(回)
    const string cmd_C0 = "C0"; //1サイクルの時間を初期化

    const string subCmd_Valve = "0000";
    const string subCmd_Pump = "0001";
    const string subCmd_PhotoelectricSensor = "0002";

    const string subCmd_OneTime_Reset = "0000";
    

    public class ArduinoSendData
    {
        //ステータス
        public bool isPhotoSensorMode; //光電センサモード
        public bool isValveOneLoopEnd; //1ループ終了
        public bool isValveOn; //電磁弁開閉
        public bool isPumpPlay; //ポンプON
        public bool isValvePlay; //電磁弁ON
        public bool isPhotoSensorTriggerOn; //トリガーON(光電センサ)

        //Arduino側が認識している設定値
        public float _OneTime;    //1サイクルの時間(ms)
        public float _OneTime_On; //1サイクルのOn時間(ms)
        public int   _CycleCnt;   //1サイクルを実行する回数(回)
    }
    ArduinoSendData _arData = new ArduinoSendData();

    //Arduinoからの１バイトデータ
    [Flags]
    public new enum ReceiveCmd
    {
        flg_7 = 1 << 7,//
        flg_6 = 1 << 6,//光電センサモード
        flg_5 = 1 << 5,//1ループ終了
        flg_4 = 1 << 4,//電磁弁の開閉
        flg_3 = 1 << 3,//ポンプON
        flg_2 = 1 << 2,//電磁弁ON
        flg_1 = 1 << 1,//トリガーON(光電センサ)
        flg_0 = 1,//
    };

    Coroutine NowCoroutine_Air;

    public new string DebugList()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT ARDUINO AIR INFO ---  ");
        sb.Append("\n");
        sb.Append("[Status]");
        sb.Append("\n");
        sb.Append("ValvePlay:");
        sb.Append(_arData.isValvePlay);
        if (_arData.isValveOn)
        {
            sb.Append("  ON");
        }
        else
        {
            sb.Append("  OFF");
        }
        sb.Append("\n");
        sb.Append("PumpPlay:");
        sb.Append(_arData.isPumpPlay);
        sb.Append("\n");
        sb.Append("PhotoSensorMode:");
        sb.Append(_arData.isPhotoSensorMode);
        sb.Append("\n");

        sb.Append("[Setting Value]");
        sb.Append("\n");
        sb.Append("One Cycle Time:");
        sb.Append(_arData._OneTime);
        sb.Append("\n");
        sb.Append("One Cycle On  :");
        sb.Append(_arData._OneTime_On);
        sb.Append("\n");
        sb.Append("Cycle Count   :");
        sb.Append(_arData._CycleCnt);
        sb.Append("\n");


        sb.Append("[GetData] ");
        if (GetData != null) sb.Append(GetData[0]);
        sb.Append("\n");

        return sb.ToString();
    }
    

    private void Start()
    {
    }

    private void Update()
    {
        //送信する値の妥当性をチェックする
        chkData();

        isAnalysis = true;

        //接続できていない場合は何もしない。
        if (!isConnect) { isAnalysis = false; return; }

        //入力値を解析する
        foreach (string _d in GetData) analisysGetData(_d);

        //電磁弁に設定するコマンドを出力する
        SendSettingValue();


        isAnalysis = false;

    }

    public ArduinoSendData Get__arData()
    {
        return _arData;
    }


    public void Set_Def_OneTime(float _v)
    {
        //1サイクルを短くする場合はOn時間を初期化する
        //On時間を変えないと、1サイクルはOn時間以下にならず、入力した値にならない。
        if (_v < Def_OneTime_On)
        {
            Def_OneTime_On = _v;
            Def_OneTime = Def_OneTime_On;
        } else
        {
            Def_OneTime = _v;
        }
    }

    public void Set_Def_OneTime_On(float _v)
    {
        if(_v > Def_OneTime)
        {
            Def_OneTime_On = Def_OneTime - MIN_DOWN;
        } else
        {
            Def_OneTime_On = _v;
        }


    }

    public void Set_Def_CycleCnt(int _v)
    {
        Def_CycleCnt = _v;
    }

    private void chkData()
    {
        //チェックの順番は1サイクルOn時間→1サイクル時間にする
        Def_OneTime_On = ChkOneTime_On(Def_OneTime_On);
        Def_OneTime = ChkOneTime(Def_OneTime);
        Def_CycleCnt = ChkCycleCnt(Def_CycleCnt);
    }

    /// <summary>
    /// 「1サイクルのOn時間」の妥当性チェック
    /// </summary>
    /// <param name="d">チェックする値</param>
    /// <returns></returns>
    private float ChkOneTime_On(float d)
    {
        float _tmp = d;

        //範囲内かチェックし、範囲外なら近い値にする
        if (_tmp < MIN_UP) _tmp = MIN_UP;
        if (_tmp > ONETIME_MAX - MIN_DOWN) _tmp = ONETIME_MAX - MIN_DOWN;

        //小数点第2位で切り捨てる
        _tmp = ((int)(_tmp * 10)) / 10.0f;

        return _tmp;
    }


    /// <summary>
    /// 「1サイクルの時間」の妥当性チェック
    /// </summary>
    /// <param name="d">チェックする値</param>
    /// <returns></returns>
    private float ChkOneTime(float d)
    {
        float _tmp_OneTime = d;

        //範囲内かチェックし、範囲外なら近い値にする
        if (_tmp_OneTime < ONETIME_MIN) _tmp_OneTime = ONETIME_MIN;
        if (_tmp_OneTime > ONETIME_MAX) _tmp_OneTime = ONETIME_MAX;

        //OFF時間が短い場合(MIN_DOWN未満)、時間を延ばす
        if (_tmp_OneTime - Def_OneTime_On < MIN_DOWN)
        {
            _tmp_OneTime = Def_OneTime_On + MIN_DOWN;
        }

        //小数点第3位で切り捨てる
        _tmp_OneTime = ((int)(_tmp_OneTime * 100)) / 100.0f;


        return _tmp_OneTime;
    }


    /// <summary>
    /// 「1サイクルを実行する回数」の妥当性チェック
    /// </summary>
    /// <param name="d">チェックする値</param>
    /// <returns></returns>
    private int ChkCycleCnt(int d)
    {
        int _tmp_cycleCnt = d;

        //範囲内かチェックし、範囲外なら近い値にする
        if (_tmp_cycleCnt < CYCLECNT_MIN) _tmp_cycleCnt = CYCLECNT_MIN;
        if (_tmp_cycleCnt > CYCLECNT_MAX) _tmp_cycleCnt = CYCLECNT_MAX;

        return _tmp_cycleCnt;
    }

    private void SendSettingValue()
    {
        //電磁弁が実行中の場合、コマンドを送信しない
        if (_arData.isValvePlay) return;

        SetSendCmd(CMD_TYPE.ONETIME_INT);
        SetSendCmd(CMD_TYPE.ONETIME_DEC);
        SetSendCmd(CMD_TYPE.ONETIME_ON);
        SetSendCmd(CMD_TYPE.CYCLECNT);
    }

    /// <summary>
    /// バルブ・ポンプの実行/停止
    /// </summary>
    /// <param name="type">バルプorポンプ</param>
    /// <param name="sw">true…実行、false…停止</param>
    public void SetSendCmd(CMD_TYPE type, bool sw)
    {
        string sendCmd = string.Empty;

        if (sw)
        {
            sendCmd = cmd_A0;
        }
        else
        {
            sendCmd = cmd_80;
        }

        switch (type)
        {
            case CMD_TYPE.VALVE:
                sendCmd += subCmd_Valve;
                break;
            case CMD_TYPE.PUMP:
                sendCmd += subCmd_Pump;
                break;
            case CMD_TYPE.PHOTO_SENSOR:
                sendCmd += subCmd_PhotoelectricSensor;
                break;
        }
        if (sendCmd.Length == CmdLength)
        {
            //Debug.LogWarning("SetSendCmd:" + sendCmd);
            DataSend(sendCmd);
        }
    }

    /// <summary>
    /// バルブの設定
    /// </summary>
    /// <param name="type">1サイクルの時間/デューティー比/1サイクルを実行する回数</param>
    /// <param name="i"></param>
    public void SetSendCmd(CMD_TYPE type)
    {
        string sendCmd = string.Empty;
        switch (type)
        {
            //1サイクルの時間(ms)　※整数部分
            case CMD_TYPE.ONETIME_INT:
                sendCmd = cmd_B0;
                sendCmd += ((int)Def_OneTime).ToString("X4");
                break;

            //1サイクルの時間(ms)　※小数部分
            case CMD_TYPE.ONETIME_DEC:
                sendCmd = cmd_B1;
                int _tmp = (int)((Def_OneTime % 1) * 1000);   //1で割った余り＝小数点以下の値 → 3桁のusに変換
                sendCmd += _tmp.ToString("X4");
                break;

            //1サイクルのOn時間(ms)  ※1桁目は小数点第1位
            case CMD_TYPE.ONETIME_ON:
                sendCmd = cmd_B2;
                sendCmd += ((int)(Def_OneTime_On * 10)).ToString("X4");
                break;

            //1サイクルを実行する回数(回)
            case CMD_TYPE.CYCLECNT:
                sendCmd = cmd_B3;
                sendCmd += Def_CycleCnt.ToString("X4");
                break;

            //1サイクルの時間を初期化
            case CMD_TYPE.ONETIME_RESET:
                sendCmd = cmd_C0 + subCmd_OneTime_Reset;
                break;
        }
        //Debug.LogWarning("[sendCmd] " + sendCmd);
        if (sendCmd.Length == CmdLength) DataSend(sendCmd);
    }

    void analisysGetData(string data)
    {
        if (data == null) return;
        if (data.Length <= 0) return;


        //カンマ区切りでデータを分ける。
        string[] Onebyte = data.Split(splitPoint);

        if (Onebyte.Length != 5) return;
        //Debug.LogWarning("[Onebyte]" + Onebyte[0] + " : " + Onebyte[1] + " : " + Onebyte[2] + " : " + Onebyte[3] + " : " + Onebyte[4]);

        //---1バイト目---
        //0～7ビットを見て1バイト目のデータだと判断する
        //全てTrue
        
        //---2バイト目---
        //ステータス
        
        //---3バイト目---
        //1サイクルの時間(us)
        
        //---4バイト目---
        //1サイクルのOn時間(us)
        
        //---5バイト目---
        //実行回数

        try
        {
            //ヘッダーの判定
            if (int.Parse(Onebyte[0]) != 0xFF) return;

            //ステータスの取得
            ReceiveCmd cmd = (ReceiveCmd)(int.Parse(Onebyte[1]));

            //必要な情報を取得する
            _arData.isPhotoSensorMode = ReceiveCmd.flg_6 == (cmd & ReceiveCmd.flg_6);
            _arData.isValveOneLoopEnd = ReceiveCmd.flg_5 == (cmd & ReceiveCmd.flg_5);
            _arData.isValveOn = ReceiveCmd.flg_4 == (cmd & ReceiveCmd.flg_4);
            _arData.isPumpPlay = ReceiveCmd.flg_3 == (cmd & ReceiveCmd.flg_3);
            _arData.isValvePlay = ReceiveCmd.flg_2 == (cmd & ReceiveCmd.flg_2);
            _arData.isPhotoSensorTriggerOn = ReceiveCmd.flg_1 == (cmd & ReceiveCmd.flg_1);

            //1サイクルの時間(us)　※ms変換
            _arData._OneTime = float.Parse(Onebyte[2]) / 1000;
            //1サイクルのOn時間(us)　※ms変換
            _arData._OneTime_On = float.Parse(Onebyte[3]) / 1000;
            //1サイクルを実行する回数
            _arData._CycleCnt = int.Parse(Onebyte[4]);

        }
        catch (Exception)
        {
            return;
        }
    }

    /// <summary>
    /// 電磁弁・ポンプを可動させる
    /// </summary>
    public void Play_ValvePump()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(PlayAllCoroutine(OnResponseCoroutine));
    }

    public void Stop_ValvePump()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);

        //電磁弁・ポンプを止める
        SetSendCmd(CMD_TYPE.VALVE, false);
        SetSendCmd(CMD_TYPE.PUMP, false);
    }


    private IEnumerator PlayAllCoroutine(UnityAction<string> callback)
    {
        //ポンプを可動させる
        SetSendCmd(CMD_TYPE.PUMP, true);

        //ポンプが十分可動するまで待つ
        yield return new WaitForSeconds(1.0f);
        
        //電磁弁を可動させる
        chkData();
        SetSendCmd(CMD_TYPE.VALVE, true);
        yield return new WaitForSeconds(1.0f);  //ポンプを一定時間可動させる。

        //電磁弁が止まるまで待つ
        while (_arData.isValvePlay)
        {
            yield return null;
        }
        //ポンプを止める
        SetSendCmd(CMD_TYPE.PUMP, false);


        callback("End all Operations");
    }

    private void OnResponseCoroutine(string s)
    {
        Debug.LogWarning("[SerialConnect_Arduino_Air]"+ s );
    }

}
