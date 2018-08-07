using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

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

    private string brokerIPAddress = string.Empty;
    private int brokerPort = 0;
    private string token = string.Empty;
    private MqttClient client;
    private String PublishMsg;
    [DataContract]
    public class PublishMsg_Beebotte
    {
        [DataMember]
        public string data { get; set; }
        [DataMember]
        public bool ispublic { get; set; }
        [DataMember]
        public long ts { get; set; }
    }
    public PublishMsg_Beebotte msg = null;

    public DateTime timestamp;
    public static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    public static DateTime FromUnixTime(long unixTime)
    {
        // UNIXエポックからの経過秒数で得られるローカル日付
        return UNIX_EPOCH.AddSeconds(unixTime).ToLocalTime();
    }

    public void Init()
    {
        client = null;

        getMQTTsetting();

        if (brokerIPAddress.Length <= 0) return;
        if (brokerPort <= 0) return;
        try
        {
            //mosquitto
            client = new MqttClient(IPAddress.Parse(brokerIPAddress), brokerPort, false, null);
        }
        catch
        {
            //beebotte
            client = new MqttClient(brokerIPAddress, brokerPort, false, null);
        }


        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
        string clientId = Guid.NewGuid().ToString();

        if (token.Length <= 0)
        {
            client.Connect(clientId);
        } else
        {
            client.Connect(clientId, token, "");
        }
    }

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        //Debug.Log("topic: " + e.Topic);
        PublishMsg = System.Text.Encoding.UTF8.GetString(e.Message);
        //Debug.Log("Received: " + PublishMsg);
        msg = JsonConvert.DeserializeObject<PublishMsg_Beebotte>(PublishMsg);
        timestamp = FromUnixTime(msg.ts / 1000);
    }

    public void SetSubscribeTopic(string topic)
    {
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
    }

    public void SetPublish(string topic, string msg)
    {
        client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }
    

    /// <summary>
    /// 外部にある設定ファイルを読む
    /// </summary>
    void getMQTTsetting()
    {
        FileInfo fi = new FileInfo(Application.dataPath + "/StreamingAssets/.mqtt_setting");
        try
        {
            using (StreamReader sr = new StreamReader(fi.OpenRead(), System.Text.Encoding.UTF8))
            {
                //1行目：IPアドレス
                //2行目：ポート
                //3行目：トークン
                brokerIPAddress = sr.ReadLine().Split(',')[1];
                brokerPort = int.Parse(sr.ReadLine().Split(',')[1]);
                token = "token:" + sr.ReadLine().Split(',')[1];
            }
        }
        catch (Exception e)
        {
        }
    }
}
