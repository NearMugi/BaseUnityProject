using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mqttUnit : MonoBehaviour
{
    [SerializeField]
    string SubscribeTopic;
    [SerializeField]
    string PublishTopic;
    [SerializeField]
    Text txt_ts;
    [SerializeField]
    Text txt_data;

    public void Init()
    {
        Debug.Log("Connect MQTT Server...");
        mqttBase.Instance.Init();
        mqttBase.Instance.SetSubscribeTopic(SubscribeTopic);
        Debug.Log("Success");
        txt_ts.text = "Wait Message...";
    }
    public void SetPublish(string _s)
    {
        Debug.Log("sending...");
        mqttBase.Instance.SetPublish(PublishTopic, "Sending from Unity3D");
        Debug.Log("sent");
    }
    private void Update()
    {
        if (mqttBase.Instance == null) return;
        if (mqttBase.Instance.msg == null) return;
        txt_ts.text = mqttBase.Instance.timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        txt_data.text = mqttBase.Instance.msg.data;
    }
}
