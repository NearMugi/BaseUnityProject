using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialPortName : MonoBehaviour {
    // SerialHandlerのリストと紐づく
    [System.NonSerialized]
    public int SerialListNo;
    public SerialHandler.Def_PortName portName_def;
    // 分かりやすい名前
    public string UserName;
    // ポート名を自動取得
    public bool isAutoSetPortName;
    // ポート名
    public string portName;
    // ボーレート 
    public int baudRate;
}