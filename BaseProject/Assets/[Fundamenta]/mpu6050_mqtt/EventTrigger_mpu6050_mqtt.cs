using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventTrigger_mpu6050_mqtt : MonoBehaviour
{

    #region Singleton

    private static EventTrigger_mpu6050_mqtt instance;

    public static EventTrigger_mpu6050_mqtt Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (EventTrigger_mpu6050_mqtt)FindObjectOfType(typeof(EventTrigger_mpu6050_mqtt));
            }
            return instance;
        }
    }

    #endregion Singleton


    [SerializeField]
    Text text_OutputValue;

    [SerializeField]
    GameObject obj;

    [SerializeField]
    Vector3 ofs_localRotation;

    private Quaternion base_qt;  //mpu6050から取得したデータ
    private Quaternion unity_qt; //unity座標に変換したデータ
    public Quaternion TargetQuaternion; //画面に合わせて角度を調整したデータ



    private void Start()
    {
        TargetQuaternion = new Quaternion();
    }

    private void Update()
    {
        if (mqttBase.Instance != null && mqttBase.Instance.msg != null)
        {
            string[] tmp = mqttBase.Instance.msg.data.Split(',');
            if (tmp.Length == 4)
            {
                base_qt = new Quaternion(float.Parse(tmp[0]), float.Parse(tmp[1]), float.Parse(tmp[2]), float.Parse(tmp[3]));
                unity_qt = new Quaternion(1.0f * base_qt[2], -1.0f * base_qt[3], -1.0f * base_qt[1], base_qt[0]);
                //画面に合わせて角度を調整する
                TargetQuaternion = new Quaternion(unity_qt[0] + ofs_localRotation.x, unity_qt[1] + ofs_localRotation.y, unity_qt[2] + ofs_localRotation.z, unity_qt[3]);
            }
        }

        obj.GetComponent<Transform>().localRotation = TargetQuaternion;

        //設定するデータをテキストに反映する
        Set_OutputValue();

    }

    private void Set_OutputValue()
    {
        if (text_OutputValue == null) return;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("MPU6050のQuaternion[w,x,y,z]\n");
        sb.Append(base_qt.ToString("f2"));
        sb.Append("\n");

        sb.Append("調整後のQuaternion[x,y,z,w]\n");
        sb.Append(TargetQuaternion.ToString("f2"));
        sb.Append("\n");

        //        sb.Append("WorldAccel[x,y,z]\n");
        //sb.Append(_arData._worldAccel[0].ToString()); sb.Append("\n");
        //sb.Append(_arData._worldAccel[1].ToString()); sb.Append("\n");
        //sb.Append(_arData._worldAccel[2].ToString()); sb.Append("\n");


        text_OutputValue.text = sb.ToString();
    }

}