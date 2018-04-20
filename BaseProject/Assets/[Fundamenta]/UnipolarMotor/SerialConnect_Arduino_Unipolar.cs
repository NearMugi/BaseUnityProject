using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialConnect_Arduino_Unipolar : SerialConnect_Arduino_Base
{

    #region Singleton

    private static SerialConnect_Arduino_Unipolar instance;

    public static SerialConnect_Arduino_Unipolar Instance_Unipolar
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Arduino_Unipolar)FindObjectOfType(typeof(SerialConnect_Arduino_Unipolar));
            }
            return instance;
        }
    }

    #endregion Singleton

    
    //コマンドの定義
    public enum CMD_TYPE
    {
        NONE,
        MOTOR1,     //モーター１実行・停止、またはppsを即時反映
        MOTOR2,     //モーター２実行・停止、またはppsを即時反映
        TERMINAL,   //
    }

    //コマンド送信時の動作パターン
    public enum SEND_PTN
    {
        NONE,
        START,  //開始
        STOP,   //停止
        ONLY_SETTING,   //モーターの可動状況と関係なく設定値を送信
    }

    /// <summary>
    /// 送信するデータセット
    /// </summary>
    class MotorSetting
    {
        public bool isReady;    //true…コマンドを送信準備完了、false…コマンドを送らない
        public SEND_PTN _ptn;
        public CMD_TYPE type;
        public int pps;
        public bool dir;
    }
    MotorSetting[] MotorInfo = new MotorSetting[2];

    [HideInInspector]
    public ReceiveCmd UnipolarStatus { get; private set; }

    const int CmdLength = 6;    //コマンドの長さ(終点を除く文字列)

    const string cmd_A0 = "A0"; //0x0000　モーター１実行・0x0001　モーター２実行
    const string cmd_80 = "80"; //0x0000  モーター１停止・0x0001　モーター２停止
    const string cmd_B0 = "B0"; //0x0000～0xFFFF　pps
    const string cmd_B1 = "B1"; //0x0000　正方向・0x0001　逆方向
    const string cmd_B2 = "B2"; //0x0000　モーター１に指定したpps、方向を設定　※モーターは停止する
                                //0x0001　モーター２に～
    const string cmd_B3 = "B3"; //0x0000～0xFFFF　モーター１に指定したppsを即時反映
    const string cmd_B4 = "B4"; //0x0000～0xFFFF　モーター２に指定したppsを即時反映

    const bool PLAY = true;
    const bool STOP = false;


    const bool MOTOR1 = true;
    const bool MOTOR2 = false;
    const string subCmd_Motor1 = "0000";
    const string subCmd_Motor2 = "0001";

    const bool FORWARD = true;
    const bool REVERSE = false;
    const string subCmd_DirForward = "0000";
    const string subCmd_DirReverse = "0001";

    //Arduinoから受信するデータ
    //1バイト目：ヘッダー
    //flg_0,7がTrue、それ以外はfalse


    //2バイト目：モーター1，2のステータス
    //flg_7
    //flg_6
    //flg_5 モーター２　true…準備OK、false…NG
    //flg_4 モーター２　true…開始、false…停止
    //flg_3
    //flg_2 モーター１　true…準備OK、false…NG
    //flg_1 モーター１　true…開始、false…停止
    //flg_0
    
    public new string DebugList()
    {
        if (_serial == null) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT UNIPOLAR MOTOR INFO ---");
        sb.Append("\n");
        sb.Append("[GetData]");
        sb.Append("\n");
        sb.Append(GetDataSize);
        sb.Append("\n");
        sb.Append("[UnipolarStatus]");
        sb.Append("\n");
        sb.Append(UnipolarStatus);
        sb.Append("\n");
        return sb.ToString();
    }

    private void Start()
    {
        StartInit();
        MotorInfo[0] = new MotorSetting { isReady = false, _ptn = SEND_PTN.NONE, type = CMD_TYPE.MOTOR1, pps = 0, dir = true };
        MotorInfo[1] = new MotorSetting { isReady = false, _ptn = SEND_PTN.NONE, type = CMD_TYPE.MOTOR2, pps = 0, dir = true };

    }

    private void Update()
    {
        isAnalysis = true;
        
        foreach (string _d in GetData) analisysGetData(_d);

        foreach (MotorSetting _ms in MotorInfo)
        {
            if (_ms.isReady)
            {
                SendCmd_MotorSetting(_ms);
            }
        }
        isAnalysis = false;
    }
    
    MotorSetting getMotorSetting(CMD_TYPE motor)
    {
        MotorSetting rtn = null;
        foreach (MotorSetting _ms in MotorInfo)
        {
            if (_ms.type == motor)
            {
                rtn = _ms;
            }
        }

        return rtn;
    }
    
    //PPSをセット　※コマンドは送信していない
    public void SetPPS(CMD_TYPE motor,int i)
    {
        MotorSetting _ms = getMotorSetting(motor);
        if (_ms == null) return;
        _ms.pps = i;
    }

    //方向をセット　※コマンドは送信していない
    public void SetDir(CMD_TYPE motor, bool sw)
    {
        MotorSetting _ms = getMotorSetting(motor);
        if (_ms == null) return;
        _ms.dir = sw;
    }

    //コマンド送信時の動作パターンをセット　※コマンドは送信していない
    public void SetSendPattern(CMD_TYPE motor, SEND_PTN ptn)
    {
        MotorSetting _ms = getMotorSetting(motor);
        if (_ms == null) return;
        _ms._ptn = ptn;

    }

    //準備OK
    public void ReadyOK(CMD_TYPE motor)
    {
        MotorSetting _ms = getMotorSetting(motor);
        if (_ms == null) return;
        _ms.isReady = true;
    }

    /// <summary>
    /// MotorSettingにセットした値をもとにコマンドを送信
    /// </summary>
    void SendCmd_MotorSetting(MotorSetting _ms)
    {
        //モーターを止めて反映するか、止めずに反映するかによってコマンドが異なる
        switch (_ms._ptn)
        {
            case SEND_PTN.NONE:
                break;
            case SEND_PTN.START:
                SetSendCmd_isStart(_ms);
                break;
            case SEND_PTN.STOP:
                SetSendCmd_isStart(_ms);
                break;
            case SEND_PTN.ONLY_SETTING:
                SetSendCmd_OnlySetting(_ms);
                break;
        }
        
        //送信終わったら初期化する
        _ms._ptn = SEND_PTN.NONE;
        _ms.isReady = false;
    }


    /// <summary>
    /// PPSを送信
    /// </summary>
    /// <param name="i">pps</param>
    void SetSendCmd_pps(int i)
    {
        if (!GetisConnect()) return;
        string sendCmd = cmd_B0;
        sendCmd += i.ToString("X4");
        if (sendCmd.Length == CmdLength) DataSend(sendCmd);
    }

    /// <summary>
    /// 方向を送信
    /// </summary>
    /// <param name="sw"></param>
    void SetSendCmd_Dir(bool sw)
    {
        if (!GetisConnect()) return;

        string sendCmd = cmd_B1;
        if (sw)
        {
            sendCmd += subCmd_DirForward;
        }
        else
        {
            sendCmd += subCmd_DirReverse;
        }

        if (sendCmd.Length == CmdLength) DataSend(sendCmd);
    }

    /// <summary>
    /// PPS,方向を反映するコマンドを送信
    /// </summary>
    /// <param name="_ms"></param>
    void SetSendCmd_Setting(MotorSetting targetMotor)
    {
        if (!GetisConnect()) return;

        string sendCmd = cmd_B2;
        switch (targetMotor.type)
        {
            case CMD_TYPE.MOTOR1:
                sendCmd += subCmd_Motor1;
                break;
            case CMD_TYPE.MOTOR2:
                sendCmd += subCmd_Motor2;
                break;
        }

        if (sendCmd.Length == CmdLength) DataSend(sendCmd);
    }

    /// <summary>
    /// 対象モーターの開始/停止コマンドを送信
    /// </summary>
    /// <param name="targetMotor"></param>
    void SetSendCmd_isStart(MotorSetting targetMotor)
    {

        //ppsを送信
        SetSendCmd_pps(targetMotor.pps);
        //方向を送信
        SetSendCmd_Dir(targetMotor.dir);
        //値を反映
        SetSendCmd_Setting(targetMotor);


        string sendCmd = string.Empty;
        if (targetMotor._ptn == SEND_PTN.START)
        {
            sendCmd = cmd_A0;
        }
        else
        {
            sendCmd = cmd_80;
        }

        switch (targetMotor.type)
        {
            case CMD_TYPE.MOTOR1:
                sendCmd += subCmd_Motor1;
                break;
            case CMD_TYPE.MOTOR2:
                sendCmd += subCmd_Motor2;
                break;
        }

        if (sendCmd.Length == CmdLength) DataSend(sendCmd);
    }

    /// <summary>
    /// 対象モーターを止めずにppsを反映するコマンドを送信
    /// </summary>
    /// <param name="targetMotor"></param>
    void SetSendCmd_OnlySetting(MotorSetting targetMotor)
    {
        string sendCmd = string.Empty;
        
        switch (targetMotor.type)
        {
            case CMD_TYPE.MOTOR1:
                sendCmd = cmd_B3;
                break;
            case CMD_TYPE.MOTOR2:
                sendCmd = cmd_B4;
                break;
        }

        sendCmd += targetMotor.pps.ToString("X4");
        if (sendCmd.Length == CmdLength) DataSend(sendCmd);
    }

    void analisysGetData(string data)
    {
        GetDataSize = 0;
        if (data == null) return;
        if (data.Length <= 0) return;

        //カンマ区切りでデータを分ける。
        //1バイト目：ヘッダー、2バイト目：モーター1，2のステータス
        //2バイトでない場合は何もしない。
        string[] Onebyte = data.Split(splitPoint);

        if (Onebyte.Length != 2) return;

        try
        {
            //ヘッダーの判定
            if (int.Parse(Onebyte[0]) != 129) return;   //129 -> 0b10000001

            UnipolarStatus = (ReceiveCmd)(int.Parse(Onebyte[1]));
            GetDataSize++;
            //Debug.LogWarning("cmd : " + int.Parse(data) + "  ->  " + UnipolarStatus);

        }
        catch (Exception)
        {
            return;
        }
    }

}

