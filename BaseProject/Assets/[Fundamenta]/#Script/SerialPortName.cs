using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialPortName : MonoBehaviour {
    [System.NonSerialized]
    public int SerialListNo;   //SerialHandlerのリストと紐づく
    public SerialHandler.Def_PortName portName_def;
    public string UserName; //分かりやすい名前
    public string portName; //ポート名
    public int baudRate;   //ボードレート
}
