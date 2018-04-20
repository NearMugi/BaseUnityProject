using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SerialConnect_Arduino_DCMotor : SerialConnect_Arduino_Base
{

    #region Singleton

    private static SerialConnect_Arduino_DCMotor instance;

    public static SerialConnect_Arduino_DCMotor Instance_DCMotor
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Arduino_DCMotor)FindObjectOfType(typeof(SerialConnect_Arduino_DCMotor));

                if (instance == null)
                {
                   // Debug.LogError(typeof(SerialConnect_Arduino_DCMotor) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton
    

    //RSモーターの正転・逆転を指示
    const int MOTOR_STOP = 0;
    const int MOTOR_MAX = 100;
    const int MOTOR_MIN = -100;

    const int CmdLength = 6;    //コマンドの長さ(終点を除く文字列)

    const string cmd_A0 = "A0"; //0x0000　モーター可動の準備
    const string cmd_B0 = "B0"; //0x0000～0xFFFF　目標スピード(＋)
    const string cmd_B1 = "B1"; //0x0000～0xFFFF　目標スピード(－)

    const string subCmd_Motor = "0000";

    string DebugMsg_MotorInit;   //初期化中に表示するメッセージ
    
    public class ArduinoSendData
    {
        public bool isPulseOn;
        public bool isNormalRotation;
        public bool isReverseRotation;
        public int targetSpeed;
    }
    ArduinoSendData _arData = new ArduinoSendData();

    //Arduinoからの１バイトデータ
    [Flags]
    public new enum ReceiveCmd
    {
        flg_7 = 1 << 7,//
        flg_PulseOn = 1 << 6,//
        flg_NormalRotation = 1 << 5,//
        flg_ReverseRotation = 1 << 4,     //
        flg_3 = 1 << 3,      //
        flg_2 = 1 << 2,     //
        flg_1 = 1 << 1,   //
        flg_0 = 1, 
    };

    Coroutine NowCoroutine_RSMotor;
    bool isInit;

    public new string DebugList()
    {
        if (_serial == null) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT DCMOTOR INFO ---  ");
        sb.Append("\n");
        sb.Append("[Status]");
        sb.Append("\n");
        sb.Append("PulseOn:");
        sb.Append(_arData.isPulseOn);
        sb.Append("\n");
        sb.Append("Rotation:");
        if (_arData.isNormalRotation && !_arData.isReverseRotation) sb.Append("Normal");
        if (!_arData.isNormalRotation && _arData.isReverseRotation) sb.Append("Reverse");
        sb.Append("\n");
        sb.Append("TargetSpeed:");
        sb.Append(_arData.targetSpeed);
        sb.Append("\n");

        sb.Append("[GetData] ");
        if (GetData != null) sb.Append(GetData[0]);
        sb.Append("\n");
        sb.Append(DebugMsg_MotorInit);

        return sb.ToString();
    }
    

    private void Start()
    {
        isInit = false;
        DebugMsg_MotorInit = string.Empty;
    }
    private void Update()
    {
        isAnalysis = true;

        //接続できていない場合は何もしない。
        if (!isConnect) { isAnalysis = false; return; }
        //初期化できていない場合は何もしない。
        if (!isInit) { isAnalysis = false; return; }

        foreach (string _d in GetData) analisysGetData(_d);

        isAnalysis = false;

    }

    public void SerialInit()
    {
        if (NowCoroutine_RSMotor != null) StopCoroutine(NowCoroutine_RSMotor);
        NowCoroutine_RSMotor = StartCoroutine(SerialInitCoroutine(OnCallback));
    }

    /// <summary>
    /// シリアル通信が開始したときにモーターを止めるコルーチン
    /// <para>シリアル通信接続の処理と合わせて実行する</para>
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator SerialInitCoroutine(UnityAction<string> callback)
    {
        callback("--- Motor Initialize ... ---");
        isInit = false;
        DebugMsg_MotorInit = string.Empty;

        //シリアル通信が確立するまで待つ
        while (!isConnect)
        {
            yield return null;
        }
        
        //モーターをリセット
        SetReady();
        yield return null;

        //モーターの初期動作　なぜか初回のコマンドに反応しないので一度送っておく。
        int _v = 0;
        while (_v <= 100)
        {
            SetSendCmd(_v);
            _v += 2;
            yield return null;

        }

        while (_v >= 0)
        {
            SetSendCmd(_v);
            _v -= 2;
            yield return null;

        }
       
        callback("");

        isInit = true;
        yield break;
    }

    public void OnCallback(string _s)
    {
        DebugMsg_MotorInit = _s;
    }
    
    /// <summary>
    /// モーター可動の準備
    /// </summary>
    public void SetReady()
    {
        //接続できていない場合は何もしない。
        if (!isConnect) return;

        string sendCmd = string.Empty;
        sendCmd = cmd_A0;
        sendCmd += subCmd_Motor;

        if (sendCmd.Length == CmdLength) DataSend(sendCmd);
     
    }

    /// <summary>
    /// スピードの設定
    /// </summary>
    /// <param name="_speedValue">目標スピード</param>
    public void SetSendCmd(int _speedValue)
    {
        //接続できていない場合は何もしない。
        if (!isConnect) return;

        string sendCmd = string.Empty;

        if(_speedValue >= 0)
        {
            sendCmd = cmd_B0;
        } else
        {
            sendCmd = cmd_B1;
        }

        sendCmd += Math.Abs(_speedValue).ToString("X4");
        
        if (sendCmd.Length == CmdLength) DataSend(sendCmd);
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
        //0～7ビットを見て1バイト目のデータだと判断する
        //全てTrue

        //---2バイト目---
        //ステータス

        //---3バイト目---
        //目標スピード(絶対値)


        try
        {
            //ヘッダーの判定
            if (int.Parse(Onebyte[0]) != 0xFF) return;

            //ステータスの取得
            ReceiveCmd cmd = (ReceiveCmd)(int.Parse(Onebyte[1]));

            //必要な情報を取得する
            _arData.isPulseOn = ReceiveCmd.flg_PulseOn == (cmd & ReceiveCmd.flg_PulseOn);
            _arData.isNormalRotation = ReceiveCmd.flg_NormalRotation == (cmd & ReceiveCmd.flg_NormalRotation);
            _arData.isReverseRotation = ReceiveCmd.flg_ReverseRotation == (cmd & ReceiveCmd.flg_ReverseRotation);

            //目標スピードの取得
            _arData.targetSpeed = int.Parse(Onebyte[2]);

        }
        catch (Exception)
        {
            return;
        }
    }
}
