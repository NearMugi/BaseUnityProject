using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using System;

public class mqttBase : MonoBehaviour {
    #region Singleton

    private static mqttBase instance;

    public static mqttBase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (mqttBase)FindObjectOfType(typeof(mqttBase));
            }
            return instance;
        }
    }

    #endregion Singleton

    [SerializeField]
    private string brokerIPAddress;
    [SerializeField]
    private int brokerPort;
	private MqttClient client;

    public void Init()
    {
        client = null;
        if (brokerIPAddress.Length <= 0) return;
        client = new MqttClient(IPAddress.Parse(brokerIPAddress), brokerPort, false, null);
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId);
    }

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        Debug.Log("topic: " + e.Topic);
        Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message));
    }

    public void SetSubscribeTopic(string topic)
    {
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    public void SetPublish(string topic, string msg)
    {
        client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }
}
