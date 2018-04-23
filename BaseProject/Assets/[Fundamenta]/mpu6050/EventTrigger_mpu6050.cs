using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventTrigger_mpu6050 : MonoBehaviour {

    #region Singleton

    private static EventTrigger_mpu6050 instance;

    public static EventTrigger_mpu6050 Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (EventTrigger_mpu6050)FindObjectOfType(typeof(EventTrigger_mpu6050));
            }
            return instance;
        }
    }

    #endregion Singleton


    [SerializeField]
    Text text_InputValue;
    [SerializeField]
    Text text_OutputValue;

    [SerializeField]
    GameObject obj;

    [SerializeField]
    int Offset_gyroX;
    [SerializeField]
    int Offset_gyroY;
    [SerializeField]
    int Offset_gyroZ;
    [SerializeField]
    int Offset_accelZ;

    [SerializeField]
    Vector3 ofs_localRotation;

    private void Update()
    {
        if(SerialConnect_Arduino_mpu6050.Instance_mpu6050 != null)
        {
            Quaternion _q = SerialConnect_Arduino_mpu6050.Instance_mpu6050.GetQuaternion;
            //画面に合わせて角度を調整する
            _q[0] += ofs_localRotation.x;
            _q[1] += ofs_localRotation.y;
            _q[2] += ofs_localRotation.z;

            obj.GetComponent<Transform>().localRotation = _q;

            //受信したデータをテキストに反映する
            Set_InputValue();
        }

        //設定するデータをテキストに反映する
        Set_OutputValue();

    }

    private void Set_OutputValue()
    {
        if (text_OutputValue == null) return;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("[設定するオフセット]\n");
        sb.Append(Offset_gyroX); sb.Append(", ");
        sb.Append(Offset_gyroY); sb.Append(", ");
        sb.Append(Offset_gyroZ); sb.Append(", ");
        sb.Append(Offset_accelZ);
        sb.Append("\n");

        sb.Append("[画面に合わせた角度調整]\n");
        sb.Append(ofs_localRotation.ToString("f2"));



        text_OutputValue.text = sb.ToString();
    }

    private void Set_InputValue()
    {
        if (text_InputValue == null) return;

        SerialConnect_Arduino_mpu6050.ArduinoSendData _arData = SerialConnect_Arduino_mpu6050.Instance_mpu6050.Get__arData();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("[受信データ]\n");
        sb.Append("初期化済み:"); sb.Append(_arData.isFinishInitialize); sb.Append("\n");

        sb.Append("[オフセット]\n");
        sb.Append(_arData.CalOfs[0]); sb.Append(", ");
        sb.Append(_arData.CalOfs[1]); sb.Append(", ");
        sb.Append(_arData.CalOfs[2]); sb.Append(", ");
        sb.Append(_arData.CalOfs[3]);
        sb.Append("\n");



        sb.Append("Quaternion[w,x,y,z]\n(");
        sb.Append(_arData._q[0].ToString("f2")); sb.Append(", ");
        sb.Append(_arData._q[1].ToString("f2")); sb.Append(", ");
        sb.Append(_arData._q[2].ToString("f2")); sb.Append(", ");
        sb.Append(_arData._q[3].ToString("f2"));
        sb.Append(")\n");

        sb.Append("UnityでのQuaternion[x,y,z,w]\n");
        sb.Append(SerialConnect_Arduino_mpu6050.Instance_mpu6050.GetQuaternion.ToString("f2"));
        sb.Append("\n");

        text_InputValue.text = sb.ToString();
    }





    public void Btn_Calibration()
    {
        SerialConnect_Arduino_mpu6050.Instance_mpu6050.StartCalibration(true);
    }
    
    public void Btn_Calibration_WithOffset()
    {
        SerialConnect_Arduino_mpu6050.Instance_mpu6050.SetOffset_And_Calibration(Offset_gyroX, Offset_gyroY, Offset_gyroZ, Offset_accelZ);
    }    
}
