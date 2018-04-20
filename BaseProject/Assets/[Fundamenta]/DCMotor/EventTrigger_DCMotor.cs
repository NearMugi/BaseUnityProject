using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventTrigger_DCMotor : MonoBehaviour {

    #region Singleton

    private static EventTrigger_DCMotor instance;

    public static EventTrigger_DCMotor Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (EventTrigger_DCMotor)FindObjectOfType(typeof(EventTrigger_DCMotor));

                if (instance == null)
                {
                    Debug.LogError(typeof(EventTrigger_DCMotor) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton

    [SerializeField]
    Toggle tgl_isUseSilderValue;
    [SerializeField]
    Text txt_silderValue;
    [SerializeField]
    Text txt_Mode;

    const int MOTOR_MIN = -100; //モータースピードの閾値
    const int MOTOR_STOP = 0;
    const int MOTOR_MAX = 100;
    const float TRIGGER_ON = 0.15f;  //スポンジ入力の閾値(割合)
    const float SPONGE_TRIGGER_MAX = 6.0f;  //スポンジ入力の閾値

    bool _isUseSilderValue;
    int flg_mode=0;    //0…init・1…Trigger・2…Linear
    float Debug_MotorValue; //スライダーの値(＝スピードの値)
    int _speedValue;  //モーターに送信している値

    public void Set_isUseSilderValue()
    {
        if (tgl_isUseSilderValue == null) return;
        _isUseSilderValue = tgl_isUseSilderValue.isOn;

    }

    public void Set_ModeInit()
    {
        flg_mode = 0;
    }
    public void Set_ModeStartTrigger()
    {
        flg_mode = 1;
    }
    public void Set_ModeStartLinear()
    {
        flg_mode = 2;
    }

    private void Start()
    {
        flg_mode = 0;
        _speedValue = 0;
        Set_isUseSilderValue();
    }

    private void Update()
    {
        txt_silderValue.text = Debug_MotorValue.ToString();
        switch (flg_mode)
        {
            case 0:
                MotorPlay_Init();
                txt_Mode.text = "停止中\n設定スピード:" + _speedValue;
                break;
            case 1:
                MotorPlay_Trigger();
                txt_Mode.text = "閾値を超えるとスタート・ストップ\n設定スピード:" + _speedValue;
                break;
            case 2:
                MotorPlay_Linear();
                txt_Mode.text = "入力値に沿って回転スピードを可変\n設定スピード:" + _speedValue;
                break;
        }
        try
        {
            SerialConnect_Arduino_DCMotor.Instance_DCMotor.SetSendCmd(_speedValue);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[EventTrigger_DCMotor] " + ex.Message);
        }
    }


    public void setDebug_SpongeValue_Motor(float v)
    {
        Debug_MotorValue = v;
    }



    float GetSpeedValue(int _id)
    {
        if (_isUseSilderValue) return Debug_MotorValue;

        float _s = 0.0f;
        try
        {
            if (SerialConnect_Sponge.Instance.Now_Mode == SerialConnect_Sponge.MODE.ANALYSIS)
            {
                //指定したスポンジから情報を取得する
                float _v = 0;
                Sponge_Manage.SpongeInfo _info = SerialConnect_Sponge.Instance.GetSpongeInfo(_id);
                if (_info != null) _v = _info.distValue_ave;
                _s = MOTOR_MAX * (_v / SPONGE_TRIGGER_MAX);
            }
        }
        catch (System.Exception)
        {

        }
        return _s;
    }
    
    public void MotorPlay_Init()
    {
        _speedValue = 0;
    }

    /// <summary>
    /// スポンジ入力からモーターの回転速度を決める(速度一定)
    /// </summary>
    /// <returns></returns>
    public void MotorPlay_Trigger()
    {
        float v = GetSpeedValue(0);  //1番目のスポンジの平均値を使う

        //閾値の判定 最小値に満たない場合は可動させない
        //満たしている場合は一定の値で回転させる。
        //つまりスポンジの値は無視して一定。
        if (System.Math.Abs(v)/ MOTOR_MAX < TRIGGER_ON)
        {
            _speedValue = MOTOR_STOP;
        }
        else
        {
            if (v > MOTOR_STOP) _speedValue = MOTOR_MAX;
            if (v < MOTOR_STOP) _speedValue = MOTOR_MIN;
        }
    }


    /// <summary>
    /// スポンジ入力からモーターの回転速度を決める(スポンジの値に合わせて可変する)
    /// </summary>
    /// <returns></returns>
    public void MotorPlay_Linear()
    {
        float v = GetSpeedValue(0);  //1番目のスポンジの平均値を使う
        if (v > MOTOR_MAX) v = MOTOR_MAX;
        if (v < MOTOR_MIN) v = MOTOR_MIN;
        _speedValue = (int)v;
    }

}
