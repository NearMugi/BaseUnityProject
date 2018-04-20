using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventTrigger_Air : MonoBehaviour {

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
    Text text_InputValue;
    [SerializeField]
    Text text_OutputValue;

    private void Start()
    {
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

        sb.Append("1周期(ms):"); sb.Append(SerialConnect_Arduino_Air.Instance_Air.Def_OneTime); sb.Append("\n");
        sb.Append("On時間(ms):"); sb.Append(SerialConnect_Arduino_Air.Instance_Air.Def_OneTime_On); sb.Append("\n");
        sb.Append("実行回数(回):"); sb.Append(SerialConnect_Arduino_Air.Instance_Air.Def_CycleCnt); sb.Append("\n");
        text_OutputValue.text = sb.ToString();
    }

    private void Set_InputValue()
    {
        if (text_InputValue == null) return;

        SerialConnect_Arduino_Air.ArduinoSendData _arData = SerialConnect_Arduino_Air.Instance_Air.Get__arData();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("1周期(ms):"); sb.Append(_arData._OneTime); sb.Append("\n");
        sb.Append("On時間(ms):"); sb.Append(_arData._OneTime_On); sb.Append("\n");
        sb.Append("実行回数(回):"); sb.Append(_arData._CycleCnt); sb.Append("\n");
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
    /// 1サイクルの時間をセット　テキストボックス用
    /// </summary>
    /// <param name="t"></param>
    public void SetOneTime(string t)
    {
        float i;
        if (float.TryParse(t, out i))
        {
            SetOneTime(i);
        }
    }

    /// <summary>
    /// スライダー用
    /// </summary>
    /// <param name="_f"></param>
    public void SetOneTime(float _f)
    {
        SerialConnect_Arduino_Air.Instance_Air.Set_Def_OneTime(_f);
    }

    /// <summary>
    /// 1サイクルのOn時間をセット　テキストボックス用
    /// </summary>
    /// <param name="t"></param>
    public void SetOneTime_On(string t)
    {
        float i;
        if (float.TryParse(t, out i))
        {
            SetOneTime_On(i);
        }
    }
    /// <summary>
    /// スライダー用
    /// </summary>
    /// <param name="_f"></param>
    public void SetOneTime_On(float _f)
    {
        SerialConnect_Arduino_Air.Instance_Air.Set_Def_OneTime_On(_f);
    }

    /// <summary>
    /// 実行回数をセット　テキストボックス用
    /// </summary>
    /// <param name="t"></param>
    public void SetCycleCnt(string t)
    {
        int i;
        if (int.TryParse(t, out i))
        {
            SetCycleCnt(i);
        }
    }
    /// <summary>
    /// スライダー用
    /// </summary>
    /// <param name="_f"></param>
    public void SetCycleCnt(float _f)
    {
        SerialConnect_Arduino_Air.Instance_Air.Set_Def_CycleCnt((int)_f);
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
