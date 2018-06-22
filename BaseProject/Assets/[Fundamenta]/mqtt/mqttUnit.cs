using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mqttUnit : MonoBehaviour
{
    [SerializeField]
    string SubscribeTopic;
    [SerializeField]
    string PublishTopic;

    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 40, 80, 20), "Connect"))
        {
            Debug.Log("Connect MQTT Server...");
            mqttBase.Instance.Init();
            mqttBase.Instance.SetSubscribeTopic(SubscribeTopic);
            Debug.Log("Success");
        }


        if (GUI.Button(new Rect(20, 70, 80, 20), "Publish"))
        {
            Debug.Log("sending...");
            mqttBase.Instance.SetPublish(PublishTopic, "Sending from Unity3D");
            Debug.Log("sent");
        }
    }
}
