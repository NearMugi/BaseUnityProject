using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// mpu6050と接続する
/// </summary>
public class SerialConnect_Arduino_mpu6050 : SerialConnect_Arduino_Base
{

    #region Singleton

    private static SerialConnect_Arduino_mpu6050 instance;

    public static SerialConnect_Arduino_mpu6050 Instance_mpu6050
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialConnect_Arduino_mpu6050)FindObjectOfType(typeof(SerialConnect_Arduino_mpu6050));                
            }
            return instance;
        }
    }

    #endregion Singleton

    Coroutine NowCoroutine;

    [HideInInspector]
    public Quaternion GetQuaternion { get; private set; }

    [HideInInspector]
    public Vector3 GetWorldAccel { get; private set; }

    /// <summary>
    /// 外部からオフセット設定するときに使用する変数
    /// </summary>
    int[] SettingOfs = new int[4];

    //コマンドの定義
    public enum CMD_TYPE
    {
        NONE,
        OFFSET_GYRO_X,
        OFFSET_GYRO_Y,
        OFFSET_GYRO_Z,
        OFFSET_ACCEL_Z,

        TERMINAL,   //
    }

    const int CmdLength = 6;    //コマンドの長さ(終点を除く文字列)

    const string cmd_A0 = "A0"; //0x0000　オフセット指定なし・0x0001　オフセット指定あり
    const string subCmd_Calibration = "0000";
    const string subCmd_Calibration_WithOffset = "0001";

    //キャリブレーション時に指定するオフセット
    const string cmd_B0 = "B0"; //Gyro X　プラス
    const string cmd_B1 = "B1"; //Gyro X　マイナス
    const string cmd_C0 = "C0"; //Gyro Y　プラス
    const string cmd_C1 = "C1"; //Gyro Y　マイナス
    const string cmd_D0 = "D0"; //Gyro Z　プラス
    const string cmd_D1 = "D1"; //Gyro Z　マイナス
    const string cmd_E0 = "E0"; //Accel Z　プラス
    const string cmd_E1 = "E1"; //Accel Z　マイナス


    
    public class ArduinoSendData
    {
        //ステータス
        public bool isFinishInitialize;

        //Arduino側が認識しているオフセット
        public int[] CalOfs = new int[4];

        public float[] _q = new float[4];
        public Vector3 _worldAccel = new Vector3();

    }
    ArduinoSendData _arData = new ArduinoSendData();

    //Arduinoからの１バイトデータ
    [Flags]
    public new enum ReceiveCmd
    {
        flg_7 = 1 << 7,//true:初期化完了
        flg_6 = 1 << 6,//true:オフセットGyroXがゼロ
        flg_5 = 1 << 5,//true:オフセットGyroYがゼロ
        flg_4 = 1 << 4,//true:オフセットGyroZがゼロ
        flg_3 = 1 << 3,//true:オフセットAccelZがゼロ
        flg_2 = 1 << 2,//
        flg_1 = 1 << 1,//
        flg_0 = 1,//
    };

    Coroutine NowCoroutine_Air;

    public new string DebugList()
    {
        if (_serial == null) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- CONNECT MPU6050 INFO ---  ");
        sb.Append("\n");
        sb.Append("FinishInitialize:");
        sb.Append(_arData.isFinishInitialize);
        sb.Append("\n");

        sb.Append("Calibration Offset:");
        sb.Append(_arData.CalOfs[0]); sb.Append(", ");
        sb.Append(_arData.CalOfs[1]); sb.Append(", ");
        sb.Append(_arData.CalOfs[2]); sb.Append(", ");
        sb.Append(_arData.CalOfs[3]);
        sb.Append("\n");

        sb.Append("Quaternion[w,x,y,z]:");
        sb.Append(_arData._q[0].ToString("f2")); sb.Append(", ");
        sb.Append(_arData._q[1].ToString("f2")); sb.Append(", ");
        sb.Append(_arData._q[2].ToString("f2")); sb.Append(", ");
        sb.Append(_arData._q[3].ToString("f2"));
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
        isAnalysis = true;

        //接続できていない場合は何もしない。
        if (!isConnect) { isAnalysis = false; return; }

        //入力値を解析する
        foreach (string _d in GetData) analisysGetData(_d);

        isAnalysis = false;

    }

    public ArduinoSendData Get__arData()
    {
        return _arData;
    }
    

    /// <summary>
    /// キャリブレーションコマンド送信
    /// </summary>
    /// <param name="sw">true…オフセット指定なし、false…オフセット指定あり</param>
    private void SendCmd_Calibration(bool sw)
    {
        string sendCmd = cmd_A0;

        if (sw) sendCmd += subCmd_Calibration; else sendCmd += subCmd_Calibration_WithOffset;

        if (sendCmd.Length == CmdLength)
        {
            //Debug.LogWarning("SetSendCmd:" + sendCmd);
            DataSend(sendCmd);
        }
    }

    /// <summary>
    /// キャリブレーション時のオフセット設定
    /// </summary>
    /// <param name="type"></param>
    private void SendCmd_Offset(CMD_TYPE type, int _v)
    {
        string sendCmd = string.Empty;
        switch (type)
        {            
            case CMD_TYPE.OFFSET_GYRO_X:
                if (_v >= 0) sendCmd = cmd_B0; else sendCmd = cmd_B1;
                break;
            case CMD_TYPE.OFFSET_GYRO_Y:
                if (_v >= 0) sendCmd = cmd_C0; else sendCmd = cmd_C1;
                break;
            case CMD_TYPE.OFFSET_GYRO_Z:
                if (_v >= 0) sendCmd = cmd_D0; else sendCmd = cmd_D1;
                break;
            case CMD_TYPE.OFFSET_ACCEL_Z:
                if (_v >= 0) sendCmd = cmd_E0; else sendCmd = cmd_E1;
                break;
        }

        sendCmd += Math.Abs(_v).ToString("X4");

        //Debug.LogWarning("[sendCmd] " + sendCmd);
        if (sendCmd.Length == CmdLength) DataSend(sendCmd);
    }

    void analisysGetData(string data)
    {
        if (data == null) return;
        if (data.Length <= 0) return;


        //カンマ区切りでデータを分ける。
        string[] Onebyte = data.Split(splitPoint);
        int GetDataSize = Onebyte.Length;
        if (GetDataSize < 2) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("[GetData]");
        for (int i = 0; i < GetDataSize; i++)
        {
            sb.Append(Onebyte[i]);
            sb.Append(" : ");

        }
        //Debug.LogWarning(sb.ToString());


        //1バイト目のヘッダーを見て処理を切り替える
        //0xFF:ステータス(2バイト)
        //0xFE:オフセット(5バイト)
        //0xFD:Quaternion(5バイト)
        //0xFC:WorldAccel(4バイト)
        try
        {
            switch (int.Parse(Onebyte[0]))
            {
                //ステータスの取得
                case 0xFF:
                    if (GetDataSize != 2) break;
                    ReceiveCmd cmd = (ReceiveCmd)(int.Parse(Onebyte[1]));
                    _arData.isFinishInitialize = ReceiveCmd.flg_7 == (cmd & ReceiveCmd.flg_7);
                    break;

                //オフセットの取得
                case 0xFE:
                    if (GetDataSize != 5) break;
                    _arData.CalOfs[0] = int.Parse(Onebyte[1]);
                    _arData.CalOfs[1] = int.Parse(Onebyte[2]);
                    _arData.CalOfs[2] = int.Parse(Onebyte[3]);
                    _arData.CalOfs[3] = int.Parse(Onebyte[4]);
                    break;

                //Quaternionの取得
                case 0xFD:
                    if (GetDataSize != 5) break;
                    //[w,x,y,z]
                    _arData._q[0] = float.Parse(Onebyte[1]);
                    _arData._q[1] = float.Parse(Onebyte[2]);
                    _arData._q[2] = float.Parse(Onebyte[3]);
                    _arData._q[3] = float.Parse(Onebyte[4]);

                    //変換
                    GetQuaternion = new Quaternion(-1.0f * _arData._q[1], -1.0f * _arData._q[3], -1.0f * _arData._q[2], _arData._q[0]);
                    break;
                
                //WorldAccelの取得
                case 0xFC:
                    if (GetDataSize != 4) break;
                    //[x,y,z]
                    _arData._worldAccel = new Vector3(int.Parse(Onebyte[1]), int.Parse(Onebyte[2]), int.Parse(Onebyte[3]));
                    break;


                default:
                    break;
            }
        }
        catch (System.Exception)
        {

        }
        
    }

    /// <summary>
    /// キャリブレーション開始
    /// </summary>
    /// <param name="sw">true…オフセット指定なし、false…オフセット指定あり</param>
    public void StartCalibration(bool sw)
    {
        if (sw)
        {
            SendCmd_Calibration(sw);
        } else
        {
            if (NowCoroutine != null) StopCoroutine(NowCoroutine);
            NowCoroutine = StartCoroutine(StartCalibration_WithOffsetCoroutine(OnResponseCoroutine));
        }
    }

    private IEnumerator StartCalibration_WithOffsetCoroutine(UnityAction<string> callback)
    {
        //オフセットを送信
        SendCmd_Offset(CMD_TYPE.OFFSET_GYRO_X, _arData.CalOfs[0]);
        yield return null;
        SendCmd_Offset(CMD_TYPE.OFFSET_GYRO_Y, _arData.CalOfs[1]);
        yield return null;
        SendCmd_Offset(CMD_TYPE.OFFSET_GYRO_Z, _arData.CalOfs[2]);
        yield return null;
        SendCmd_Offset(CMD_TYPE.OFFSET_ACCEL_Z, _arData.CalOfs[3]);
        yield return null;

        //キャリブレーション開始
        SendCmd_Calibration(false);

        callback("Calibration with offset Start");
        yield break;

    }

    private void OnResponseCoroutine(string s)
    {
        Debug.LogWarning("[SerialConnect_Arduino_mpu6050]"+ s );
    }

    /// <summary>
    /// 外部からオフセットを指定、キャリブレーションを実施
    /// </summary>
    /// <param name="type"></param>
    /// <param name="_v"></param>
    public void SetOffset_And_Calibration(int _gyroX, int _gyroY, int _gyroZ, int _accelZ)
    {
        SettingOfs[0] = _gyroX;
        SettingOfs[1] = _gyroY;
        SettingOfs[2] = _gyroZ;
        SettingOfs[3] = _accelZ;

        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(SetOffset_And_CalibrationCoroutine(OnResponseCoroutine));
    }

    private IEnumerator SetOffset_And_CalibrationCoroutine(UnityAction<string> callback)
    {
        //オフセットを送信
        SendCmd_Offset(CMD_TYPE.OFFSET_GYRO_X, SettingOfs[0]);
        yield return null;
        SendCmd_Offset(CMD_TYPE.OFFSET_GYRO_Y, SettingOfs[1]);
        yield return null;
        SendCmd_Offset(CMD_TYPE.OFFSET_GYRO_Z, SettingOfs[2]);
        yield return null;
        SendCmd_Offset(CMD_TYPE.OFFSET_ACCEL_Z, SettingOfs[3]);
        yield return null;

        callback("End Send Offset");
        yield return null;

        //キャリブレーション開始
        SendCmd_Calibration(false);

        callback("Calibration with offset Start");

        yield break;

    }



}
