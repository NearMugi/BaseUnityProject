// これを参考にした
// https://qiita.com/kanatano_mirai/items/677fde8589a4d810329a
// http://kan-kikuchi.hatenablog.com/entry/UnityWebRequest
// 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class docomoTextToTalk_Create : MonoBehaviour
{
    #region Singleton

    private static docomoTextToTalk_Create instance;

    public static docomoTextToTalk_Create Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (docomoTextToTalk_Create)FindObjectOfType(typeof(docomoTextToTalk_Create));

                if (instance == null)
                {
                    Debug.LogError(typeof(docomoTextToTalk_Create) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton


    //テキストを変換したwavバイナリーデータ
    public byte[] wavBinaryData;

    const string API_KEY = "hoge";
    const string URI = "https://api.apigw.smt.docomo.ne.jp/crayon/v1/textToSpeechSsml?APIKEY=" + API_KEY;

    //TextData
    const string textSSML_Header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                    "<!DOCTYPE speak SYSTEM \"ssml.dtd\">" +
                                    "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"japanese\">";
    const string textSSML_Footer = "</speak>";

    //パラメータ        
    string speakerID;
    string styleID;
    string textData;

    //必要あれば使う
    string speechRate;
    string powerRate;
    string voiceType;
    string audioFileFormat;
    public docomoTextToTalk_Create()
    {
        speakerID = "1";
        styleID = "1";
        speechRate = "1.00";
        voiceType = "1.00";
        audioFileFormat = "0";
    }

    public void setText(string t)
    {
        textData = t;
    }
    public void setSpeakerID(string t)
    {
        speakerID = t;
    }
    public void setStyleID(string t)
    {
        styleID = t;
    }

    public IEnumerator getBinaryWavData()
    {

        UnityWebRequest request = new UnityWebRequest(URI, "POST");

        //リクエストボディ(json)をセットする
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{");
        sb.Append("\"Command\":\"AP_Synth\", ");
        sb.Append("\"SpeakerID\":\"");
        sb.Append(speakerID);
        sb.Append("\", ");
        sb.Append("\"StyleID\":\"");
        sb.Append(styleID);
        sb.Append("\", ");
        sb.Append("\"TextData\":\"");
        sb.Append(textSSML_Header);
        sb.Append(textData);
        sb.Append(textSSML_Footer);
        sb.Append("\"}");

        string body = sb.ToString();
        Debug.Log(body);

        //json(string)をbyte[]に変換
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);

        //jsonを設定
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        //ヘッダーをセットする
        request.SetRequestHeader("Content-Type", "application/json");

        //送受信開始
        yield return request.SendWebRequest();

        //エラー判定
        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log(request.error);
        }
        else
        {
            wavBinaryData = request.downloadHandler.data;
        }
    }

    private void Start()
    {
    }
}