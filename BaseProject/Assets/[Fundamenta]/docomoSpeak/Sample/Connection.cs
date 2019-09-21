using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.IO;

/// <summary>
/// 通信を行うクラス。
/// </summary>
public class Connection : MonoBehaviour
{
    void Start()
    {
    }

    /// <summary>
    /// docomo音声合成APIでテキストを音声データに変換する。
    /// <param name="text">会話テキスト</param>
    /// </summary>
    public IEnumerator ConvertTextToVoice(string text)
    {
        string url = "https://api.apigw.smt.docomo.ne.jp/crayon/v1/textToSpeechSsml?APIKEY=" + Parameter.compositionAPIkey;

        Dictionary<string, string> aiTalksParams = new Dictionary<string, string>();

        gameObject.AddComponent<Voice>();
        var postData = gameObject.GetComponent<Voice>().CreateSSML(text, aiTalksParams);
        var data = System.Text.Encoding.UTF8.GetBytes(postData);

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers["Content-Type"] = "application/ssml+xml";
        headers["Accept"] = "audio/L16";
        WWW www = new WWW(url, data, headers);
        yield return www;
        if (www.error != null)
        {
            Debug.LogError(www.error);
            yield break;
        }

        AudioClip audioClip = gameObject.GetComponent<Voice>().CreateAudioClip(www.bytes, "test.wav");

        StartCoroutine(gameObject.GetComponent<Voice>().Play(audioClip, www.bytes.Length / 2));
    }
}