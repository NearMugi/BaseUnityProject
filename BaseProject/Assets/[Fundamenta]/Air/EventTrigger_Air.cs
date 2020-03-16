using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventTrigger_Air : MonoBehaviour
{

    #region Singleton

    private static EventTrigger_Air instance;

    public static EventTrigger_Air Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (EventTrigger_Air)FindObjectOfType(typeof(EventTrigger_Air));

                if (instance == null)
                {
                    //Debug.LogError(typeof(EventTrigger_Air) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton

    [SerializeField]
    Slider sli_OneLoop;

    [SerializeField]
    InputField input_OneLoop;

    [SerializeField]
    Slider sli_OneLoop_On;

    [SerializeField]
    InputField input_OneLoop_On;

    [SerializeField]
    Slider sli_CycleCnt;

    [SerializeField]
    InputField input_CycleCnt;

    [SerializeField]
    Text text_InputValue;
    [SerializeField]
    Text text_OutputValue;

    private void Start()
    {
        //Serial Connect 
        SerialConnect_Arduino_Air.Instance.Connect();
        input_OneLoop.text = sli_OneLoop.value.ToString();
        input_OneLoop_On.text = sli_OneLoop_On.value.ToString();
        input_CycleCnt.text = sli_CycleCnt.value.ToString();
    }

    private void Update()
    {
        //設定するデータをテキストに反映する
        Set_OutputValue();

        //受信したデータをテキストに反映する
        Set_InputValue();
    }

    private void Set_OutputValue()
    {
        if (text_OutputValue == null) return;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("1周期(ms):");
        sb.Append(SerialConnect_Arduino_Air.Instance_Air.Def_OneTime);
        sb.Append("\n");
        sb.Append("On時間(ms):");
        sb.Append(SerialConnect_Arduino_Air.Instance_Air.Def_OneTime_On);
        sb.Append("\n");
        sb.Append("実行回数(回):");
        sb.Append(SerialConnect_Arduino_Air.Instance_Air.Def_CycleCnt);
        sb.Append("\n");
        text_OutputValue.text = sb.ToString();
    }

    private void Set_InputValue()
    {
        if (text_InputValue == null) return;

        SerialConnect_Arduino_Air.ArduinoSendData _arData = SerialConnect_Arduino_Air.Instance_Air.Get__arData();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("1周期(ms):");
        sb.Append(_arData._OneTime);
        sb.Append("\n");
        sb.Append("On時間(ms):");
        sb.Append(_arData._OneTime_On);
        sb.Append("\n");
        sb.Append("実行回数(回):");
        sb.Append(_arData._CycleCnt);
        sb.Append("\n");
        sb.Append("\n");

        sb.Append("[電磁弁]");
        if (_arData.isValvePlay) { sb.Append("実行中..."); } else { sb.Append("停止"); }
        if (_arData.isValveOn) { sb.Append(" Open"); } else { sb.Append(" Close"); }
        sb.Append("\n");

        sb.Append("[ポンプ]");
        if (_arData.isPumpPlay) { sb.Append("可動中..."); } else { sb.Append("停止"); }
        sb.Append("\n");

        sb.Append("[フォトセンサー]");
        if (_arData.isPhotoSensorMode) { sb.Append("入力待ち..."); } else { sb.Append("停止"); }
        sb.Append("\n");

        text_InputValue.text = sb.ToString();
    }

    /// <summary>
    /// 1サイクルの時間をセット
    /// </summary>
    /// <param name="isText">true:テキストボックスからの入力</param>
    public void SetOneLoop(bool isText)
    {
        float _f = 0.0f;
        if (isText)
        {
            float.TryParse(input_OneLoop.text, out _f);
            sli_OneLoop.value = _f;
        }
        else
        {
            _f = sli_OneLoop.value;
            input_OneLoop.text = _f.ToString();
        }

        SerialConnect_Arduino_Air.Instance_Air.Set_Def_OneTime(_f);
    }



    /// <summary>
    /// 1サイクルのOn時間をセット
    /// </summary>
    /// <param name="isText">true:テキストボックスからの入力</param>
    public void SetOneLoop_On(bool isText)
    {
        float _f = 0.0f;
        if (isText)
        {
            float.TryParse(input_OneLoop_On.text, out _f);
            sli_OneLoop_On.value = _f;
        }
        else
        {
            _f = sli_OneLoop_On.value;
            input_OneLoop_On.text = _f.ToString();
        }

        SerialConnect_Arduino_Air.Instance_Air.Set_Def_OneTime_On(_f);
    }


    /// <summary>
    /// 実行回数をセット
    /// </summary>
    /// <param name="isText">true:テキストボックスからの入力</param>
    public void SetCycleCnt(bool isText)
    {
        int _i = 0;
        if (isText)
        {
            int.TryParse(input_CycleCnt.text, out _i);
            sli_CycleCnt.value = _i;
        }
        else
        {
            _i = (int)sli_CycleCnt.value;
            input_CycleCnt.text = _i.ToString();
        }

        SerialConnect_Arduino_Air.Instance_Air.Set_Def_CycleCnt(_i);
    }


    public void Btn_Play()
    {
        SerialConnect_Arduino_Air.Instance_Air.Play_ValvePump();
    }

    public void Btn_Stop()
    {
        SerialConnect_Arduino_Air.Instance_Air.Stop_ValvePump();
    }

    public void Btn_ValveOn()
    {
        SerialConnect_Arduino_Air.Instance_Air.SettingValve();
        SerialConnect_Arduino_Air.Instance_Air.SetSendCmd(SerialConnect_Arduino_Air.CMD_TYPE.VALVE, true);
    }

    public void Btn_PumpOn()
    {
        SerialConnect_Arduino_Air.Instance_Air.SetSendCmd(SerialConnect_Arduino_Air.CMD_TYPE.PUMP, true);
    }

    public void Btn_PhotoSensorOn()
    {
        SerialConnect_Arduino_Air.Instance_Air.SetSendCmd(SerialConnect_Arduino_Air.CMD_TYPE.PHOTO_SENSOR, true);
    }

    public void Btn_PhotoSensorOff()
    {
        SerialConnect_Arduino_Air.Instance_Air.SetSendCmd(SerialConnect_Arduino_Air.CMD_TYPE.PHOTO_SENSOR, false);
    }

}