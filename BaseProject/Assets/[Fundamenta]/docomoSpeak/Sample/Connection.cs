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
        string uri = "https://api.apigw.smt.docomo.ne.jp/crayon/v1/textToSpeechSsml?APIKEY=" + Parameter.compositionAPIkey;

        Dictionary<string, string> aiTalksParams = new Dictionary<string, string>();

        gameObject.AddComponent<Voice>();
        var postData = gameObject.GetComponent<Voice>().CreateSSML(text, aiTalksParams);

        UnityWebRequest request = new UnityWebRequest(uri, postData);
        request.SetRequestHeader("Content-Type", "application/ssml+xml");
        request.SetRequestHeader("Accept", "audio/L16");
        yield return request.SendWebRequest();

        if (request.isHttpError || request.isNetworkError)
        {
            Debug.LogError(request.error);
            yield break;
        }
        Debug.Log(request.downloadHandler.text);
        byte[] wavData = request.downloadHandler.data;
        AudioClip audioClip = gameObject.GetComponent<Voice>().CreateAudioClip(wavData, "test.wav");

        StartCoroutine(gameObject.GetComponent<Voice>().Play(audioClip, wavData.Length / 2));
    }
}