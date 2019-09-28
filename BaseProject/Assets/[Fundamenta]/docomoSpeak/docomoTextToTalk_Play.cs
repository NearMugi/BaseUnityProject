// これを参考にした
// https://qiita.com/kanatano_mirai/items/677fde8589a4d810329a
// http://kan-kikuchi.hatenablog.com/entry/UnityWebRequest
// 
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

    public IEnumerator PlayAudioClip(AudioClip clip, int samples)
    {
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        float[] rawData = new float[samples * clip.channels];
        clip.GetData(rawData, 0);

        audio.clip = clip;
        audio.loop = true;
        audio.Play();
        yield return new WaitForSeconds(audio.clip.length - 0.5f);

    }

    public void playAudio(string word)
    {
        if (docomoTextToTalk_Create.Instance == null) return;
        byte[] wavBinary = docomoTextToTalk_Create.Instance.wavBinaryData;
        writeWav(wavBinary, word + ".aac");

        mediaTranscoding m = gameObject.GetComponent<mediaTranscoding>();
        m.createWav(wavBinary);
        //AudioClip audioClip = CreateAudioClip(tmpBinary, word + ".wav");
        //StartCoroutine(PlayAudioClip(audioClip, tmpBinary.Length / 2));
    }

    public void writeWav(byte[] wavBinary, string fn)
    {
        FileStream stream = new FileStream(fn, FileMode.Create);
        stream.Write(wavBinary, 0, wavBinary.Length);
    }





}