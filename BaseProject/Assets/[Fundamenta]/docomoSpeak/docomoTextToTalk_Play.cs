// これを参考にした
// http://fantom1x.blog130.fc2.com/blog-entry-299.html?sp

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
public class docomoTextToTalk_Play : MonoBehaviour
{

    #region Singleton

    private static docomoTextToTalk_Play instance;

    public static docomoTextToTalk_Play Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (docomoTextToTalk_Play)FindObjectOfType(typeof(docomoTextToTalk_Play));

                if (instance == null)
                {
                    Debug.LogError(typeof(docomoTextToTalk_Play) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton
    public AudioSource audioSource;
    private string filePath;

    public IEnumerator playAudioClip(byte[] aacBinary, string fn)
    {
        if (docomoTextToTalk_Create.Instance == null) yield break;

        filePath = Application.dataPath + @"/StreamingAssets/aacToWav/" + fn + ".wav";
        //wavファイルを作成する
        //aacバイナリーデータ -> aacファイル -> wavファイル
        mediaTranscoding m = gameObject.GetComponent<mediaTranscoding>();
        yield return m.createWav(aacBinary, fn);

        using (WWW www = new WWW(filePath))
        {
            while (!www.isDone)
                yield return null;

            AudioClip audioClip = www.GetAudioClip(false, true);
            if (audioClip.loadState != AudioDataLoadState.Loaded)
            {
                Debug.Log("Failed to load AudioClip.");
                yield break;
            }

            audioSource.clip = audioClip;
            audioSource.Play();
            Debug.Log("Load success : " + filePath);
        }

    }
}