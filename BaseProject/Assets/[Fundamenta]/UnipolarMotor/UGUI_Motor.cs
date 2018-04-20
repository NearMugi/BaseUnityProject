using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UGUI_Motor : MonoBehaviour {
    
    [SerializeField]
    SerialConnect_Arduino_Unipolar.CMD_TYPE motor;
    
    enum STATUS { NONE,STOP,START,NOTREADY,TURMINAL}
    STATUS now_status;

    [SerializeField]
    Slider sld_pps;
    int ppsValue;
    int ppsValue_Def;

    [SerializeField]
    Text txt_status;

    [SerializeField]
    Toggle tgl_dir;
    bool dir;
    bool dir_Def;

    // Use this for initialization
    void Start () {
        now_status = STATUS.STOP;
        if (sld_pps != null) {
            ppsValue_Def = (int)sld_pps.value;
            ppsValue = ppsValue_Def;
        }
        if(tgl_dir != null)
        {
            dir_Def = !tgl_dir.isOn;
            dir = dir_Def;
        }
    }

    void Update () {

        ppsValue = (int)sld_pps.value;  //ppsを取得
        dir = !tgl_dir.isOn;    //向きを取得　※チェックボックスとコマンドのtrue/falseが逆になっている

        if (!SerialConnect_Arduino_Base.Instance.GetisConnect()) return;

        try
        {
            //ステータスの更新        
            SerialConnect_Arduino_Base.ReceiveCmd status = SerialConnect_Arduino_Unipolar.Instance_Unipolar.UnipolarStatus;
            bool _motplay = false;
            bool _motready = false;
            if (motor == SerialConnect_Arduino_Unipolar.CMD_TYPE.MOTOR1)
            {
                _motplay = status.HasFlag(SerialConnect_Arduino_Base.ReceiveCmd.flg_1);
                _motready = status.HasFlag(SerialConnect_Arduino_Base.ReceiveCmd.flg_2);
            }
            else
            {
                _motplay = status.HasFlag(SerialConnect_Arduino_Base.ReceiveCmd.flg_4);
                _motready = status.HasFlag(SerialConnect_Arduino_Base.ReceiveCmd.flg_5);
            }

            if (!_motready)
            {
                now_status = STATUS.NOTREADY;
            }
            else
            {
                if (_motplay)
                {
                    now_status = STATUS.START;
                }
                else
                {
                    now_status = STATUS.STOP;
                }
            }

            txt_status.text = now_status.ToString() + " pps:" + ppsValue.ToString();
        }
        catch (System.Exception)
        {
            txt_status.text = "Fail to update Status";
        }


    }

    /// <summary>
    /// モーターの設定を初期化する
    /// </summary>
    public void MotorInit()
    {
        ppsValue = ppsValue_Def;
        sld_pps.value = ppsValue;
        dir = dir_Def;
        tgl_dir.isOn = dir;

        SerialConnect_Arduino_Unipolar.Instance_Unipolar.SetPPS(motor, ppsValue);
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.SetDir(motor, dir);
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.SetSendPattern(motor, SerialConnect_Arduino_Unipolar.SEND_PTN.STOP);
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.ReadyOK(motor);
    }

    public void SetPps()
    {
        ppsValue = (int)sld_pps.value;
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.SetPPS(motor, ppsValue);

        switch (now_status)
        {
            //実行中の場合、即時反映
            case STATUS.START:
                SerialConnect_Arduino_Unipolar.Instance_Unipolar.SetSendPattern(motor, SerialConnect_Arduino_Unipolar.SEND_PTN.ONLY_SETTING);
                SerialConnect_Arduino_Unipolar.Instance_Unipolar.ReadyOK(motor);
                break;

            //準備中または停止中の場合、ppsを送信、設定
            case STATUS.NOTREADY:
            case STATUS.STOP:
                SetStop();
                break;
        }
    }

    public void SetDir()
    {
        //方向を送信、設定
        //モーターは停止する
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.SetDir(motor, dir);
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.SetSendPattern(motor, SerialConnect_Arduino_Unipolar.SEND_PTN.STOP);
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.ReadyOK(motor);
    }

    public void SetStart()
    {
        if(now_status != STATUS.START)
        {
            SerialConnect_Arduino_Unipolar.Instance_Unipolar.SetSendPattern(motor, SerialConnect_Arduino_Unipolar.SEND_PTN.START);
            SerialConnect_Arduino_Unipolar.Instance_Unipolar.ReadyOK(motor);
        }
    }
    public void SetStop()
    {
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.SetSendPattern(motor, SerialConnect_Arduino_Unipolar.SEND_PTN.STOP);
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.ReadyOK(motor);
    }

}
