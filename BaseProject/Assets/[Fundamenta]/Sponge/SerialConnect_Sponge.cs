using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SerialConnect_Sponge : MonoBehaviour
{

    #region Singleton

    private static SerialConnect_Sponge instance;

    public static SerialConnect_Sponge Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Sponge)FindObjectOfType(typeof(SerialConnect_Sponge));
            }
            return instance;
        }
    }

    #endregion Singleton

    /// <summary>
    /// スポンジセンサーと紐づくSerialHandler.serial_unit
    /// </summary>
    SerialHandler.serial_unit _serial;

    [SerializeField]
    int Def_SensorCnt;         //基準値　使用するセンサーの個数(個)
    [SerializeField]
    int Def_ReceiveInterval;    //基準値　データ要求周期(×10ms)

    [HideInInspector]
    public Mesh[] MeshPlot;

    //処理内で使用する変数
    Sponge_Manage _spongeManage;       //同一オブジェクトにある前提
    int _init_SensorCnt;    //初期設定においてID要求を受けた回数
    bool _ReceiveInterval;  //データ要求周期が正常に設定された場合、True
    Coroutine NowCoroutine;

    bool flg_TimeOver;      //初期設定時のタイムオーバー
    float timeOver;
    const float LIMIT_TIMEOVER = 2.0f;  //待ち時間


    //送信コマンドの定義
    string SENDCMD_RESET = "r";     //初期設定
    string SENDCMD_CONTINUE = "l";  //連続通信
    string SENDCMD_RECEIVE_INTERVAL = "@0201";  //データ要求周期
    //※センサー数,センサーIDは変数から生成する

    //受信コマンドの定義
    enum RECEIVECMD_PTN
    {
        //使用センサー数指定の要求
        REQ_SENSOR_CNT_0 = 0x55,    //'U'
        REQ_SENSOR_CNT_1 = 0x3A,    //':'
        CR = 0x0D,    //CR
        LF = 0x0A,    //LF
        //使用センサーのID要求(センサーの数だけ受信する)
        REQ_SENSOR_ID_0 = 0x49,     //'I'
        REQ_SENSOR_ID_1 = 0x3A,     //':'
    }

    /// <summary>
    /// センサー値の返値の構成
    /// </summary>
    enum DATALAYOUT
    {
        ID = 0,
        CNT = 2,
        CH1 = 4,
        CH4 = 8,
        CH2 = 12,
        CH3 = 16,
        TURMINAL,
    }

    /// <summary>
    /// モード
    /// </summary>
    public enum MODE
    {
        NONE,
        INIT,               //初期設定
        RECEIVE_INTERVAL,   //データ要求周期変更
        COMMUNICATION,      //連続通信
        ANALYSIS,           //解析中

        TURMINAL,
    }
    public MODE Now_Mode { get; private set; }
    MODE Bef_Mode;

    /// <summary>
    /// 初期設定中のステータス
    /// </summary>
    enum STATUS_INIT
    {
        RESET,              //[PC→中継基板]リセット要求　＆　[中継基板→PC]使用センサ数指定の要求待ち
        SENSOR_CNT,         //[PC→中継基板]センサ数の送信
        SENSOR_ID_WAIT,     //[中継基板→PC]センサーID指定の要求待ち
        SENSOR_ID,          //[PC→中継基板]センサーIDの送信

        TURMINAL,
    }

    STATUS_INIT Now_Status_Init;

    public int GetDef_SensorCnt()
    {
        return Def_SensorCnt;
    }


    public string DebugList()
    {
        if (_serial == null) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- SPONGE INFO ---");
        sb.Append("\n");
        sb.Append("[MODE]");
        sb.Append(Now_Mode);
        sb.Append("\n");

        sb.Append("[value]");
        float ave;
        for (int _id = 0; _id < Def_SensorCnt; _id++)
        {
            ave = 0;
            Sponge_Manage.SpongeInfo _info = _spongeManage.GetSpongeInfo(_id.ToString());
            if (_info != null) ave = _info.distValue_ave;
            sb.Append(ave);
            sb.Append(",");

        }
        sb.Append("\n");

        return sb.ToString();
    }

    public Sponge_Manage.SpongeInfo GetSpongeInfo(int _id)
    {
        Sponge_Manage.SpongeInfo _info = _spongeManage.GetSpongeInfo(_id.ToString());
        return _info;
    }


    /// <summary>
    /// 送信するコマンドをセット、送信する関数をCall
    /// </summary>
    /// <param name="_cmd">送信するコマンド(指定があれば使う)</param>
    public void SetSendCmd(string _cmd)
    {
        flg_TimeOver = false;
        timeOver = 0.0f;
        string sendCmd = string.Empty;
        switch (Now_Mode)
        {
            //初期設定
            case MODE.INIT:
                switch (Now_Status_Init)
                {
                    case STATUS_INIT.RESET:
                        sendCmd = SENDCMD_RESET;
                        break;

                    case STATUS_INIT.SENSOR_CNT:
                        sendCmd = _cmd; //引数＝センサー個数
                        break;

                    case STATUS_INIT.SENSOR_ID:
                        sendCmd = _cmd; //引数＝使用するセンサーID
                        break;
                }
                break;

            //データ要求周期変更
            case MODE.RECEIVE_INTERVAL:
                sendCmd = SENDCMD_RECEIVE_INTERVAL;
                sendCmd += IntToHexString(Def_ReceiveInterval);
                sendCmd += "\r\n";  //改行コード
                break;

            case MODE.COMMUNICATION:
                sendCmd = SENDCMD_CONTINUE;
                break;
        }
        if (sendCmd.Length > 0) DataSend(sendCmd);
    }
    /// <summary>
    /// データ送信
    /// </summary>
    /// <param name="_s"></param>
    void DataSend(string _s)
    {
        //       DebugMsg("[DataSend] ", _s);
        _serial.Write(_s);
    }

    /// <summary>
    /// int型→16進数文字列→そのままString型に
    /// <para>15→0x0F→"0F"</para>
    /// </summary>
    /// <param name="_value"></param>
    /// <returns></returns>
    string IntToHexString(int _value)
    {
        string chg = _value.ToString("X2");
        return chg;
    }

    /// <summary>
    /// 16進数文字列→int型
    /// </summary>
    /// <param name="_value"></param>
    /// <returns></returns>
    int HexStringToInt(string _value)
    {
        int chg = int.Parse(_value, System.Globalization.NumberStyles.HexNumber);
        return chg;
    }
    /// <summary>
    /// 10進数文字列→int型
    /// </summary>
    /// <param name="_value"></param>
    /// <returns></returns>
    int DecimalStringToInt(string _value)
    {
        int chg = int.Parse(_value);
        return chg;
    }

    // Use this for initialization
    void Start()
    {

    }

    public void Connect()
    {
        Now_Mode = MODE.TURMINAL;
        Bef_Mode = MODE.TURMINAL;

        _spongeManage = GetComponent<Sponge_Manage>();
        StartCoroutine(ConnectCoroutine());
    }

    private IEnumerator ConnectCoroutine()
    {
        flg_TimeOver = false;
        timeOver = 0.0f;

        Now_Mode = MODE.TURMINAL;
        Bef_Mode = MODE.TURMINAL;
        var wait = new WaitForSeconds(0.1f);
        yield return null;

        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        yield return null;

        SerialPortName _sp = GetComponent<SerialPortName>();
        if (_sp == null) yield break;
        _serial = SerialHandler.Instance.PortList[_sp.SerialListNo];   //SerialHandlerのリストと紐づく

        //USBの切断
        _serial.Close();
        yield return wait;


        _serial.OnDataReceived -= OnDataRead_Init;
        _serial.OnDataReceived -= OnDataRead_Receive_Interval;
        _serial.OnDataReceived -= OnDataRead_Communication;
        yield return wait;


        //USBの接続
        _serial.Open(true);
        yield return wait;


        Now_Mode = MODE.NONE;
        Bef_Mode = MODE.TURMINAL;


        yield break;

    }

    private void Update()
    {
        //Debug.LogWarning("[SerialConnect_Sponge Update] Bef_Mode:" + Bef_Mode + " Now_Mode:" + Now_Mode);


        //モードが変更になった場合のみ実行
        //各モードでやりたい処理はコルーチンで実行
        //コルーチンの実行が終了次第、コールバックがある
        if (Now_Mode != Bef_Mode)
        {
            Bef_Mode = Now_Mode;

            switch (Now_Mode)
            {
                //初期状態
                case MODE.NONE:
                    Now_Mode = MODE.INIT;
                    break;

                //初期設定
                case MODE.INIT:
                    //初期化
                    _serial.OnDataReceived += OnDataRead_Init;

                    //初期設定用コルーチンをCall
                    NowCoroutine = StartCoroutine(InitCoroutine(OnFinishedInitCoroutine));
                    break;

                //データ要求周期変更
                case MODE.RECEIVE_INTERVAL:
                    //初期化
                    _serial.OnDataReceived -= OnDataRead_Init;
                    _serial.OnDataReceived += OnDataRead_Receive_Interval;

                    //データ要求周期変更用コルーチンをCall
                    NowCoroutine = StartCoroutine(Receive_IntervalCoroutine(OnFinishedReceive_IntervalCoroutine));

                    break;

                //連続データ要求
                case MODE.COMMUNICATION:
                    //初期化
                    _serial.OnDataReceived -= OnDataRead_Receive_Interval;

                    //連続データ要求用コルーチンをCall
                    NowCoroutine = StartCoroutine(CommunicationCoroutine(OnFinishedCommunicationCoroutine));

                    break;

                //解析中
                case MODE.ANALYSIS:
                    _serial.OnDataReceived += OnDataRead_Communication;

                    //連続データ要求用コルーチンをCall
                    NowCoroutine = StartCoroutine(AnalysisCoroutine(OnFinishedAnalysisCoroutine));

                    break;

            }
        }


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

    /// <summary>
    /// [初期設定]分割したデータからキーワードを探す
    /// </summary>
    /// <param name="_data"></param>
    /// <param name="b0"></param>
    /// <param name="b1"></param>
    /// <returns></returns>
    bool FindKeyword_Init(string[] _data, byte b0, byte b1)
    {
        //Debug.LogWarning("_data[0]" + _data[0] + "   b0 , b1 " + b0 + " , " + b1);

        bool sw = false;
        try
        {
            foreach (string _d in _data)
            {
                if (_d.Length < 2) continue;

                //Debug.LogWarning("_d[0] , _d[1] " + _d[0] + " , " + _d[1]); 

                if (_d[0] == b0 && _d[1] == b1)
                {
                    sw = true;
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message + "[_data]" + _data[0] + " , " + _data[1] + "  [" + Now_Status_Init + "]");
        }

        return sw;
    }
    /// <summary>
    /// [初期設定]区切り文字で分割したデータを処理する
    /// </summary>
    /// <param name="_data"></param>
    void RecieveData_Check_Init(string[] _data)
    {
        if (_data == null) return;

        switch (Now_Status_Init)
        {
            //リセット要求を送信済み　→　使用センサ数指定のコマンドの受信待ち
            case STATUS_INIT.RESET:
                if (FindKeyword_Init(_data, (byte)RECEIVECMD_PTN.REQ_SENSOR_CNT_0, (byte)RECEIVECMD_PTN.REQ_SENSOR_CNT_1))
                {
                    Now_Status_Init = STATUS_INIT.SENSOR_CNT;   //センサ数の送信指示
                }
                break;

            //センサーID指定の要求待ち
            case STATUS_INIT.SENSOR_ID_WAIT:
                //最後は"I:"を受信しない仕様
                if (_init_SensorCnt < Def_SensorCnt)
                {
                    if (FindKeyword_Init(_data, (byte)RECEIVECMD_PTN.REQ_SENSOR_ID_0, (byte)RECEIVECMD_PTN.REQ_SENSOR_ID_1))
                    {
                        //                        Debug.LogWarning("I: を受信");
                        Now_Status_Init = STATUS_INIT.SENSOR_ID;   //センサーIDの送信指示
                    }
                }
                else
                {
                    char[] c = new char[2];
                    if (Def_SensorCnt >= 10)
                    {
                        c[0] = (char)Def_SensorCnt.ToString()[0];
                        c[1] = (char)Def_SensorCnt.ToString()[1];
                    }
                    else
                    {
                        c[0] = '0';
                        c[1] = (char)Def_SensorCnt.ToString()[0];
                    }

                    if (FindKeyword_Init(_data, (byte)c[0], (byte)c[1]))
                    {
                        //                        Debug.LogWarning("最後のIDを受信");
                        Now_Status_Init = STATUS_INIT.SENSOR_ID;   //センサーIDの送信指示
                    }
                }

                break;

        }
    }

    private void ChkTimeOver()
    {
        timeOver += Time.deltaTime;
        if (timeOver >= LIMIT_TIMEOVER)
        {
            flg_TimeOver = true;
        }
    }

    /// <summary>
    /// 初期設定用の受信イベント
    /// </summary>
    /// <param name="message"></param>
    void OnDataRead_Init(string[] message)
    {
        //        foreach(string _s in message)
        //        {
        //            DebugMsg("[OnDataRead_Init] ", _s);
        //        }

        //初期設定用の処理を行う
        RecieveData_Check_Init(message);
    }

    /// <summary>
    /// 初期設定用コルーチン
    /// </summary>
    /// <param name="callback_init"></param>
    /// <returns></returns>
    private IEnumerator InitCoroutine(UnityAction<bool> callback)
    {
        var wait = new WaitForSeconds(0.01f);

        //[PC→中継基板]リセット要求
        Now_Status_Init = STATUS_INIT.RESET;
        SetSendCmd(string.Empty);

        //[中継基板→PC]使用センサ数指定の要求待ち
        while (Now_Status_Init != STATUS_INIT.SENSOR_CNT)
        {
            ChkTimeOver();
            if (flg_TimeOver)
            {
                callback(true);
                yield break;
            }

            yield return wait;
        }


        //[PC→中継基板]センサ数の送信
        SetSendCmd(Def_SensorCnt.ToString("00"));
        Now_Status_Init = STATUS_INIT.SENSOR_ID_WAIT;
        //Debug.LogWarning("[PC→中継基板]センサ数の送信");

        //センサーIDの指定をPC・中継基板間でやり取りする
        _init_SensorCnt = 0;
        while (_init_SensorCnt <= Def_SensorCnt)
        {
            //[中継基板→PC]使用センサ数のID要求待ち
            while (Now_Status_Init != STATUS_INIT.SENSOR_ID)
            {
                //Debug.Log("[中継基板→PC]使用センサ数のID要求待ち  id:" + _init_SensorCnt.ToString("00"));
                ChkTimeOver();
                if (flg_TimeOver)
                {
                    callback(false);
                    yield break;
                }

                yield return wait;
            }
            //[PC→中継基板]センサーIDの送信
            if (++_init_SensorCnt <= Def_SensorCnt)
            {
                SetSendCmd(_init_SensorCnt.ToString("00"));
                Now_Status_Init = STATUS_INIT.SENSOR_ID_WAIT;
            }
        }

        //全て終わったら次のモードに以降する
        callback(true);
        yield break;

    }

    /// <summary>
    /// 初期設定用コルーチンからのCallBack
    /// </summary>
    /// <param name="flg"></param>
    private void OnFinishedInitCoroutine(bool flg)
    {
        //タイムオーバーの場合は再度接続する
        if (flg_TimeOver)
        {
            StartCoroutine(ConnectCoroutine());
            return;
        }


        if (flg)
        {
            Now_Mode = MODE.RECEIVE_INTERVAL;
            if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        }
        else
        {
            StartCoroutine(ConnectCoroutine());
        }
    }


    /// <summary>
    /// [データ要求周期変更]区切り文字で分割したデータを処理する
    /// </summary>
    /// <param name="_data"></param>
    void RecieveData_Check_Receive_Interval(string[] _data)
    {
        if (_data == null) return;

        foreach (string _t in _data)
        {
            if (_t != string.Empty)
            {
                string tmp = string.Empty;
                for (int i = 0; i < _t.Length; i++)
                {
                    tmp += _t[i];
                }
                if (DecimalStringToInt(tmp) == Def_ReceiveInterval)
                {
                    _ReceiveInterval = true;
                    //Debug.LogWarning("[RecieveData_Check_Receive_Interval]" + _t);
                }
            }
        }


    }


    /// <summary>
    /// データ要求周期変更用の受信イベント
    /// </summary>
    /// <param name="message"></param>
    void OnDataRead_Receive_Interval(string[] message)
    {
        //Debug.LogWarning("[OnDataRead_Receive_Interval]");
        //データ要求周期変更用の処理を行う
        RecieveData_Check_Receive_Interval(message);
    }

    /// <summary>
    /// データ要求周期変更用コルーチン
    /// </summary>
    /// <param name="callback_init"></param>
    /// <returns></returns>
    private IEnumerator Receive_IntervalCoroutine(UnityAction<bool> callback)
    {
        callback(false);

        _ReceiveInterval = false;

        //データ要求周期を送信
        SetSendCmd(string.Empty);

        while (!_ReceiveInterval)
        {
            ChkTimeOver();
            if (flg_TimeOver)
            {
                callback(true);
                yield break;
            }

            yield return null;
        }
        //全て終わったら次のモードに以降する
        callback(true);
        yield break;

    }

    /// <summary>
    /// データ要求周期変更用コルーチンからのCallBack
    /// </summary>
    /// <param name="flg"></param>
    private void OnFinishedReceive_IntervalCoroutine(bool flg)
    {
        //タイムオーバーの場合は再度接続する
        if (flg_TimeOver)
        {
            StartCoroutine(ConnectCoroutine());
            return;
        }

        if (flg)
        {
            Now_Mode = MODE.COMMUNICATION;
            if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        }
    }

    /// <summary>
    /// [連続データ要求]区切り文字で分割したデータを処理する
    /// </summary>
    /// <param name="_data"></param>
    void RecieveData_Check_Communication(string[] _data)
    {
        if (_data == null) return;

        foreach (string _t in _data)
        {
            //DebugMsg("[RecieveData_Check_Communication] _t ", _t);
            //チャンネルの値を実測データ解析用テーブルへ反映する
            SetInput(_t);
        }
    }

    /// <summary>
    /// 受信データから必要なデータを抽出する
    /// </summary>
    /// <param name="_t"></param>
    /// <param name="_start"></param>
    /// <param name="CharCnt"></param>
    /// <returns></returns>
    int ReceiveDataToInt(string _t, int _start, int CharCnt)
    {
        int rt = 0;

        byte[] tmp = new byte[CharCnt];
        int j = 0;
        for (int i = _start; i < _start + CharCnt; i++)
        {

            tmp[j++] = (byte)_t[i];
        }

        string _s = System.Text.Encoding.ASCII.GetString(tmp);
        rt = HexStringToInt(_s);
        return rt;
    }

    /// <summary>
    /// 各IDに入力値をセットする
    /// </summary>
    /// <param name="_t"></param>
    /// <returns>取得したID　エラーの場合はゼロ</returns>
    void SetInput(string _t)
    {
        //想定するデータの長さと一致しない場合はエラー
        if (_t.Length != 20) return;

        //IDを取得
        int _id = ReceiveDataToInt(_t, (int)DATALAYOUT.ID, 2);
        //DebugMsg("[SetInput] ID:", _id.ToString());
        //存在しないIDの場合はエラー
        if (_id <= 0 || _id > Def_SensorCnt) return;

        //各チャンネルの値を取得
        int[] input = new int[4] {
            ReceiveDataToInt(_t, (int)DATALAYOUT.CH1, 4),
            ReceiveDataToInt(_t, (int)DATALAYOUT.CH2, 4),
            ReceiveDataToInt(_t, (int)DATALAYOUT.CH3, 4),
            ReceiveDataToInt(_t, (int)DATALAYOUT.CH4, 4)
        };

        //Debug.LogWarning("[SetInput] ID:" + _id + " CH:(" + Input[_id-1].CH[0] + "," + Input[_id - 1].CH[1] + "," + Input[_id - 1].CH[2] + "," + Input[_id - 1].CH[3] + ")");
        _spongeManage.SetSpongeInfo((_id - 1).ToString(), input);

    }

    /// <summary>
    /// 連続データ要求用の受信イベント
    /// </summary>
    /// <param name="message"></param>
    void OnDataRead_Communication(string[] message)
    {
        //データ要求周期変更用の処理を行う
        RecieveData_Check_Communication(message);
    }

    /// <summary>
    /// 連続データ要求用コルーチン
    /// </summary>
    /// <param name="callback_init"></param>
    /// <returns></returns>
    private IEnumerator CommunicationCoroutine(UnityAction<bool> callback)
    {
        callback(false);

        //連続データ要求を送信、1回だけで終了
        SetSendCmd(string.Empty);
        yield return null;

        callback(true);
        yield break;
    }

    /// <summary>
    /// 連続データ要求用コルーチンからのCallBack
    /// </summary>
    /// <param name="flg"></param>
    private void OnFinishedCommunicationCoroutine(bool flg)
    {
        if (flg)
        {
            Now_Mode = MODE.ANALYSIS;
            if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        }
    }


    /// <summary>
    /// 解析用コルーチン
    /// </summary>
    /// <param name="callback_init"></param>
    /// <returns></returns>
    private IEnumerator AnalysisCoroutine(UnityAction<bool> callback)
    {
        if (_spongeManage != null) _spongeManage.SetDefine();
        Sponge_PlotMesh _mesh = GetComponent<Sponge_PlotMesh>();
        MeshPlot = new Mesh[Def_SensorCnt];
        if (_mesh != null)
        {
            for (int i = 0; i < Def_SensorCnt; i++)
            {
                MeshPlot[i] = new Mesh();
                _mesh.SetDefine(this, i);
            }
        }

        callback(false);
        while (!_spongeManage._isEndSetting) yield return null;

        var wait = new WaitForSeconds(0.01f);
        yield return null;

        while (Now_Mode == MODE.ANALYSIS)
        {
            _spongeManage.Analysis();  //値を解析する
            for (int i = 0; i < Def_SensorCnt; i++)
            {
                MeshPlot[i] = _mesh.SetZ(_spongeManage, i); //メッシュ用データを更新する
            }

            yield return wait;
        }

        callback(true);
        yield break;
    }

    /// <summary>
    /// 解析用コルーチンからのCallBack
    /// </summary>
    /// <param name="flg"></param>
    private void OnFinishedAnalysisCoroutine(bool flg)
    {
        if (flg)
        {
            if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        }
    }
}
