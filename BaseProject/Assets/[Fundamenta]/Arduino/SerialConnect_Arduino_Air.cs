using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SerialConnect_Arduino_Air : MonoBehaviour {

    #region Singleton

    private static SerialConnect_Arduino_Air instance;

    public static SerialConnect_Arduino_Air Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Arduino_Air)FindObjectOfType(typeof(SerialConnect_Arduino_Air));

                if (instance == null)
                {
                    Debug.LogError(typeof(SerialConnect_Arduino_Air) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton
    
    [SerializeField]
    float Def_OneTime; //基準値　1サイクルの時間(ms)
    [SerializeField]
    float Def_OneTime_On; //基準値　1サイクルのOn時間(ms)
    [SerializeField]
    int Def_CycleCnt;//基準値　1サイクルを実行する回数(回)

    //設定値の制限
    const float MIN_UP = 1.5f;   //パルス立ち上がりに要する時間(ms)
    const float MIN_DOWN = 1.5f; //パルス立ち下がりに要する時間(ms)

    const float ONETIME_MIN = 3.0f;    //1サイクルの時間(ms)
    const float ONETIME_MAX = 5000.0f;

    const int CYCLECNT_MIN = 1;   //1サイクルを実行する回数
    const int CYCLECNT_MAX = 2000;

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

        TERMINAL,   //
    }
    
    const int CmdLength = 6;    //コマンドの長さ(終点を除く文字列)

    const string cmd_A0 = "A0"; //0x0000　バルブ実行・0x0001　ポンプ実行
    const string cmd_80 = "80"; //0x0000  バルブ停止・0x0001　ポンプ停止
    const string cmd_B0 = "B0"; //0x0000～0xFFFF　1サイクルの時間(ms) ※整数部分
    const string cmd_B1 = "B1"; //0x0000～0xFFFF　1サイクルの時間(ms) ※小数部分 2位まで
    const string cmd_B2 = "B2"; //0x0000～0xFFFF　1サイクルのOn時間(ms) ※1桁目は小数点第1位
    const string cmd_B3 = "B3"; //0x0000～0xFFFF　1サイクルを実行する回数(回)
    const string cmd_C0 = "C0"; //1サイクルの時間を初期化

    const string subCmd_Valve = "0000";
    const string subCmd_Pump = "0001";

    const string subCmd_OneTime_Reset = "0000";
    
    bool isConnect;

    //Arduinoからの１バイトデータ
    [Flags]
    public enum ReceiveCmd
    {
        flg_7 = 1 << 7,//
        flg_6 = 1 << 6,//
        ONE_LOOP_END = 1 << 5,//
        VALVE_OPEN = 1 << 4,     //電磁弁の開閉
        PUMP_PLAY = 1 << 3,      //ポンプON
        VALVE_PLAY = 1 << 2,     //電磁弁ON
        flg_1= 1 << 1,   //
        flg_0 = 1,               //常にTrue
    };

    [HideInInspector]
    public ReceiveCmd GetData;


    public string DebugList()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT ARDUINO AIR INFO ---");
        sb.Append("\n");
        sb.Append("[Connect]");
        sb.Append(isConnect);
        sb.Append("[Data]");
        sb.Append(GetData);
        sb.Append("\n");

        return sb.ToString();
    }

    /// <summary>
    /// 外部から1サイクルの時間をセット　UGUIからセットする都合で引数はString型
    /// </summary>
    /// <param name="t"></param>
    public void SetOneTime(string t)
    {
        float i;
        if (float.TryParse(t, out i))
        {
            //1サイクルを短くする場合はOn時間を初期化する
            //On時間を変えないと、1サイクルはOn時間以下にならず、入力した値にならない。
            if (Def_OneTime > i) Def_OneTime_On = MIN_UP;

            Def_OneTime = i;
        }
    }

    /// <summary>
    /// 外部から1サイクルのOn時間をセット　UGUIからセットする都合で引数はString型
    /// </summary>
    /// <param name="t"></param>
    public void SetOneTime_On(string t)
    {
        float i;
        if (float.TryParse(t, out i))
        {
            Def_OneTime_On = i;
        }
    }

    /// <summary>
    /// 外部からサイクルを実行する回数をセット　UGUIからセットする都合で引数はString型
    /// </summary>
    /// <param name="t"></param>
    public void SetCycleCnt(string t)
    {
        int i;
        if (int.TryParse(t, out i))
        {
            Def_CycleCnt = i;
        }
    }

    public float GetDef_OneTime()
    {
        return Def_OneTime;
    }
    public float GetDef_OneTime_On()
    {
        return Def_OneTime_On;
    }
    public int GetDef_CycleCnt()
    {
        return Def_CycleCnt;
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
        }
        if (sendCmd.Length == CmdLength)
        {
            //Debug.LogWarning("SetSendCmd:" + sendCmd);
            SerialConnect_Arduino.Instance.DataSend(sendCmd);
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
        if (sendCmd.Length == CmdLength) SerialConnect_Arduino.Instance.DataSend(sendCmd);
    }
    
    public IEnumerator ConnectCoroutine()
    {
        var wait = new WaitForSeconds(0.5f);
        isConnect = false;
        yield return null;

        SerialConnect_Arduino.Instance.Connect();
        yield return wait;


        //電磁弁・ポンプを止める
        //電磁弁を停止させる
        bool isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
        while (isPlay)
        {
            SetSendCmd(CMD_TYPE.VALVE, false);
            isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
            yield return wait;
        }

        //ポンプを停止させる
        isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
        while (isPlay)
        {
            SetSendCmd(CMD_TYPE.PUMP, false);
            isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
            yield return wait;
        }

        isConnect = true;
        yield break;

    }


    // Use this for initialization
    void Start() {
        isConnect = false;

        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(ConnectCoroutine());
    }


    private void Update()
    {
        //接続できていない場合は何もしない。
        if (!isConnect) return;

        //電磁弁の値をセットする。
        SetValveValue();

        //EventTriggerのステータスを見て、電磁弁のON,OFFを決める。
#if false
        switch (EventTrigger.Instance.timeline_status)
        {
            case EventTrigger.TIMELINE_STATUS.HOLD:
                SetSendCmd(CMD_TYPE.VALVE, true);
                break;
            case EventTrigger.TIMELINE_STATUS.TRIGGER_ON:
                SetSendCmd(CMD_TYPE.VALVE, true);
                break;
            default:
                InitValve();
                break;
                
        }    
#endif

    }

    //電磁弁の設定値 1サイクル,On時間,回数
#if false
    string[,] airValue = {  { "5000", "50", "2000" },//None
                            { "500", "40", "2000" },//Level1
                            { "400", "30", "2000" },//Level2
                            { "200", "20", "2000" },//Level3
                            { "50", "10", "2000" },//LevelMax
                            { "5000", "4990", "2000" }//LevelFinish
    };
#else
    string[,] airValue = {  { "5000", "50", "2000" },//None
                            { "832.5", "40", "2000" },//Level1
                            { "499.5", "30", "2000" },//Level2
                            { "333", "20", "2000" },//Level3
                            { "166.5", "10", "2000" },//LevelMax
                            { "5000", "4990", "2000" },//LevelFinish 発射時
                            { "25", "5", "2000" }//LevelFinish　相手に当たったイメージ
    };

#endif
    float _finishTime;
    void SetValveValue()
    {
#if false
        int _nowNo = 0;
        switch (EventTrigger.Instance.now_lv)
        {
            case EventTrigger.POWER_LEVEL.LV_1:
                _finishTime = 0.0f;
                _nowNo = 1;
                break;

            case EventTrigger.POWER_LEVEL.LV_2:
                _finishTime = 0.0f;
                _nowNo = 2;
                break;

            case EventTrigger.POWER_LEVEL.LV_3:
                _finishTime = 0.0f;
                _nowNo = 3;
                break;

            case EventTrigger.POWER_LEVEL.LV_MAX:
                _finishTime = 0.0f;
                _nowNo = 4;
                break;

            case EventTrigger.POWER_LEVEL.LV_FINISH:
                _finishTime += Time.deltaTime;
                _nowNo = 5;
                if (_finishTime > 2.0f) _nowNo = 6;    //発射の途中で演出を切り替える
                break;

            default:
                break;

        }
        SetOneTime(airValue[_nowNo, 0]);
        SetOneTime_On(airValue[_nowNo, 1]);
        SetCycleCnt(airValue[_nowNo, 2]);
        chkData();
        SetSendCmd(CMD_TYPE.ONETIME_INT);
        SetSendCmd(CMD_TYPE.ONETIME_DEC);
        SetSendCmd(CMD_TYPE.ONETIME_ON);
        SetSendCmd(CMD_TYPE.CYCLECNT);
#endif
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

    public void PlayValve()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(PlayValveCoroutine(OnFinishCoroutine_PlayValve));
    }
    public void PlayPump()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(PlayPumpCoroutine(OnFinishCoroutine));
    }
    public void PlayAll()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(PlayAllCoroutine(OnFinishCoroutine));
    }
    public void StopPump()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(StopPumpCoroutine(OnFinishCoroutine));
    }
    public void StopAll()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(StopAllCoroutine(OnFinishCoroutine));
    }
    
    public void RestartValve()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(RestartValveCoroutine(OnFinishCoroutine_PlayValve));
    }
    
    public void InitValve()
    {
        //電磁弁を停止させる
        SetSendCmd(CMD_TYPE.VALVE, false);

        //値を初期化
        SetOneTime("5000");
        SetOneTime_On("0");
        SetCycleCnt("0");
        chkData();
        SetSendCmd(CMD_TYPE.ONETIME_INT);
        SetSendCmd(CMD_TYPE.ONETIME_DEC);
        SetSendCmd(CMD_TYPE.ONETIME_ON);
        SetSendCmd(CMD_TYPE.CYCLECNT);
    }


    private void OnFinishCoroutine_PlayValve(string s)
    {
        //Debug.LogWarning("["+ s + "] Coroutine End");
    }

    private void OnFinishCoroutine(string s)
    {
        //Debug.LogWarning("["+ s + "] End");
    }

    private IEnumerator PlayValveCoroutine(UnityAction<string> callback)
    {
        WaitForSeconds t = new WaitForSeconds(0.03f);

        //電磁弁を可動させる
        //※コマンドの送信は1回だけにする。2回以上送ると電磁弁がON状態になってしまう…
        chkData();
        SetSendCmd(CMD_TYPE.VALVE, true);
        bool isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
        while (!isPlay)
        {
            isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
            yield return t;
        }
        callback("PlayValveCoroutine");
        yield break;
    }
    private IEnumerator PlayPumpCoroutine(UnityAction<string> callback)
    {
        WaitForSeconds t = new WaitForSeconds(0.03f);

        //ポンプを可動させる
        SetSendCmd(CMD_TYPE.PUMP, true);
        bool isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
        while (!isPlay)
        {
            isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
            yield return t;
        }
        callback("PlayPumpCoroutine");
        yield break;
    }


    private IEnumerator PlayAllCoroutine(UnityAction<string> callback)
    {
        WaitForSeconds t = new WaitForSeconds(0.001f);

        //ポンプを可動させる
        SetSendCmd(CMD_TYPE.PUMP, true);
        bool isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
        while (!isPlay)
        {
            isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
            yield return t;
        }


        //ポンプが十分可動するまで待つ
        yield return new WaitForSeconds(1.0f);


        //電磁弁を可動させる
        chkData();
        SetSendCmd(CMD_TYPE.VALVE, true);
        isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
        while (!isPlay)
        {
            isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
            yield return t;
        }


        //電磁弁が止まるまで待つ
        isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
        while (isPlay)
        {
            isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
            yield return null;
        }
        //ポンプを止める
        SetSendCmd(CMD_TYPE.PUMP, false);


        callback("PlayAllCoroutine");
        yield break;
    }

    private IEnumerator StopPumpCoroutine(UnityAction<string> callback)
    {
        WaitForSeconds t = new WaitForSeconds(0.001f);

        //ポンプを停止させる
        bool isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
        while (isPlay)
        {
            SetSendCmd(CMD_TYPE.PUMP, false);
            isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
            yield return t;
        }

        callback("StopPumpCoroutine");
        yield break;
    }

    private IEnumerator StopAllCoroutine(UnityAction<string> callback)
    {
        WaitForSeconds t = new WaitForSeconds(0.001f);

        //電磁弁を停止させる
        bool isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
        while (isPlay)
        {
            SetSendCmd(CMD_TYPE.VALVE, false);
            isPlay = ReceiveCmd.VALVE_PLAY == (GetData & ReceiveCmd.VALVE_PLAY);
            yield return t;
        }

        //ポンプを停止させる
        isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
        while (isPlay)
        {
            SetSendCmd(CMD_TYPE.PUMP, false);
            isPlay = ReceiveCmd.PUMP_PLAY == (GetData & ReceiveCmd.PUMP_PLAY);
            yield return t;
        }

        callback("StopAllCoroutine");
        yield break;
    }

    private IEnumerator RestartValveCoroutine(UnityAction<string> callback)
    {
        //電磁弁を停止させる
        SetSendCmd(CMD_TYPE.VALVE, false);
        yield return null;

        //値を再設定
        SetSendCmd(CMD_TYPE.ONETIME_INT);
        SetSendCmd(CMD_TYPE.ONETIME_DEC);
        SetSendCmd(CMD_TYPE.ONETIME_ON);
        SetSendCmd(CMD_TYPE.CYCLECNT);
        yield return null;

        //電磁弁を可動させる
        SetSendCmd(CMD_TYPE.VALVE, true);
        yield return null;


        callback("RestartValveCoroutine");
        yield break;
    }
}
