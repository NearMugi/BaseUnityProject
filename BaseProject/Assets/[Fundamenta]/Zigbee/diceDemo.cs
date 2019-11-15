using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class diceDemo : MonoBehaviour {
    [SerializeField]
    Text txtVal;

    [SerializeField]
    Text txtInfo;

    bool isPlay;
    int loopVal;
    int lastVal;
    float waitTime;

    public void btn_start () {
        if (SerialConnect_Zigbee.Instance_Zigbee == null) return;

        isPlay = true;
        waitTime = 0.0f;
        if (SerialConnect_Zigbee.Instance_Zigbee.GetisConnect ()) {
            SerialConnect_Zigbee.Instance_Zigbee.resetDiceValue ();
            lastVal = SerialConnect_Zigbee.Instance_Zigbee.getDiceValue ();
        }
    }

    public void btn_reconnect () {
        if (SerialConnect_Zigbee.Instance_Zigbee == null) return;
        SerialConnect_Zigbee.Instance_Zigbee.Connect ();
    }
    public void btn_Exit () {
        Application.Quit ();
    }

    string getInfo () {
        StringBuilder sb = new StringBuilder ();
        sb.Append (SerialConnect_Zigbee.Instance_Zigbee.DebugList ());
        return sb.ToString ();
    }

    void updateValue () {
        int v = 0;
        if (isPlay) {
            loopVal = (++loopVal) % 6 + 1;
            v = loopVal;

            // 一定時間はダイスの目を受け付けない。
            waitTime += Time.deltaTime;
            if (waitTime > 2.0f) {
                if (SerialConnect_Zigbee.Instance_Zigbee.getDiceValue () != lastVal) {
                    isPlay = false;
                }
            }
        } else {
            v = SerialConnect_Zigbee.Instance_Zigbee.getDiceValue ();
        }
        txtVal.text = v.ToString ();
    }

    // Start is called before the first frame update
    void Start () {
        isPlay = false;
        loopVal = 1;
        lastVal = 1;
        btn_reconnect ();
    }

    // Update is called once per frame
    void Update () {
        if (SerialConnect_Zigbee.Instance_Zigbee == null) return;

        // Info
        txtInfo.text = getInfo ();

        if (SerialConnect_Zigbee.Instance_Zigbee.GetisConnect ()) {
            updateValue ();
        }

    }
}