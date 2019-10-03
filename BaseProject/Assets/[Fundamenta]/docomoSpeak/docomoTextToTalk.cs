// これを参考にした
// https://qiita.com/kanatano_mirai/items/677fde8589a4d810329a
// http://kan-kikuchi.hatenablog.com/entry/UnityWebRequest
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class docomoTextToTalk : MonoBehaviour
{
    #region Singleton

    private static docomoTextToTalk instance;

    public static docomoTextToTalk Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (docomoTextToTalk)FindObjectOfType(typeof(docomoTextToTalk));

                if (instance == null)
                {
                    UnityEngine.Debug.LogError(typeof(docomoTextToTalk) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton

    /// <summary>
    /// docomoAPIにアクセスする
    /// </summary>
    public class httpRequest
    {
        public bool isGet;
        //テキストを変換したaacバイナリーデータ
        public byte[] aacBinaryData;

        //const string API_KEY = "hoge";
        const string API_KEY = "59666366673443544162684477452f3078323146507170533538364347395a5152534569615437592e7633";

        const string URI = "https://api.apigw.smt.docomo.ne.jp/crayon/v1/textToSpeechSsml?APIKEY=" + API_KEY;

        //TextData
        const string textSSML_Header = "<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?>" +
            "<!DOCTYPE speak SYSTEM \\\"ssml.dtd\\\">" +
            "<speak version=\\\"1.0\\\" xmlns=\\\"http://www.w3.org/2001/10/synthesis\\\" xml:lang=\\\"japanese\\\">";

        const string textSSML_Footer = "</speak>";

        //パラメータ        
        string speakerID;
        string styleID;
        string textData;
        string speechRate;
        string powerRate;
        string voiceType;
        public httpRequest()
        {
            textData = string.Empty;
            speakerID = "14";
            styleID = "14";
            speechRate = "1.00";
            voiceType = "1.00";
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
        public void setSpeechRate(float v)
        {
            speechRate = v.ToString();
        }
        public void setVoiceType(float v)
        {
            voiceType = v.ToString();
        }


        public IEnumerator getAACBinaryData(UnityAction<bool> callback)
        {
            isGet = false;
            //入力値のチェック
            if (textData.Length <= 0)
            {
                callback(false);
                yield break;
            }

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
            sb.Append("\"SpeechRate\":\"");
            sb.Append(speechRate);
            sb.Append("\", ");
            sb.Append("\"VoiceType\":\"");
            sb.Append(voiceType);
            sb.Append("\", ");
            sb.Append("\"TextData\":\"");
            sb.Append(textSSML_Header);
            sb.Append(textData);
            sb.Append(textSSML_Footer);
            sb.Append("\"}");

            string body = sb.ToString();
            UnityEngine.Debug.Log(body);

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
                UnityEngine.Debug.Log(request.error);
                callback(false);
            }
            else
            {
                aacBinaryData = request.downloadHandler.data;
                callback(true);
            }
        }

        public void OnFinishedCoroutine(bool _isGet)
        {
            isGet = _isGet;
            if (!isGet)
            {
                UnityEngine.Debug.Log("...Error getAACBinaryData");
                return;
            }

            UnityEngine.Debug.Log("Success getAACBinaryData");
        }

    }
    /// <summary>
    /// バイナリーデータ(aac)からwavファイルを生成する
    /// </summary>
    public class createWav
    {
        private string folderPath;
        private string pyExePath = @"C:\Users\Kuroda-NotePC\Anaconda3\python.exe";
        //private string pyExePath = @"C:\Users\Teppei\Anaconda3\python.exe";

        private string pyCodePath = @"transcodingToWav.py";

        public createWav(string _fp)
        {
            folderPath = _fp;
        }

        Process process;
        private void callVbs(string batFilePath, string tmpAacFile)
        {
            // 他のプロセスが実行しているなら行わない
            if (process != null) return;

            // 新規プロセスを作成し、batファイルのパスを登録
            process = new Process();
            process.StartInfo.FileName = folderPath + batFilePath;
            process.StartInfo.Arguments = pyExePath + " " +
                pyCodePath + " " +
                folderPath + " " +
                tmpAacFile;

            // 外部プロセスの終了を検知するための設定
            process.EnableRaisingEvents = true;
            process.Exited += process_Exited;

            // 外部プロセスを実行
            process.Start();
        }

        // 外部プロセスの終了を検知してプロセスを終了
        void process_Exited(object sender, System.EventArgs e)
        {
            //UnityEngine.Debug.Log ("process_Exited");
            process.Dispose();
            process = null;
        }

        public IEnumerator create(byte[] aacBinary, string fn)
        {
            string aacFilePath = "/" + fn + ".aac";
            string wavFilePath = "/" + fn + ".wav";
            //binaryからaacファイルを作る
            using (FileStream stream = new FileStream(folderPath + aacFilePath, FileMode.Create))
            {
                stream.Write(aacBinary, 0, aacBinary.Length);
            }

            //aacファイルが出来るまで待つ
            while (!System.IO.File.Exists(folderPath + aacFilePath))
            {
                yield return null;
            }
            UnityEngine.Debug.Log("Created aac :" + aacFilePath);

            //wavファイルの作成、aacファイルの削除
            callVbs(@"/aacToWav.vbs", aacFilePath);
            //wavファイルが出来るまで待つ
            while (!System.IO.File.Exists(folderPath + wavFilePath))
            {
                yield return null;
            }

            yield break;
        }

    }

    [HideInInspector]
    public httpRequest httpReq;
    [HideInInspector]
    public createWav cW;
    private AudioSource audioSource;
    // 作業するフォルダ(バッチファイル・pythonファイルも入っている)
    private string folderPath;
    public IEnumerator playAudioClip(string filePath)
    {
        using (WWW www = new WWW(filePath))
        {
            while (!www.isDone)
                yield return null;

            AudioClip audioClip = www.GetAudioClip(false, true);
            if (audioClip.loadState != AudioDataLoadState.Loaded)
            {
                UnityEngine.Debug.Log("Failed to load AudioClip.");
                yield break;
            }

            audioSource.clip = audioClip;
            audioSource.Play();
            UnityEngine.Debug.Log("Load success : " + filePath);
        }
    }

    public IEnumerator PlayCoroutine()
    {
        //httpRequest
        yield return httpReq.getAACBinaryData(httpReq.OnFinishedCoroutine);
        if (!httpReq.isGet) yield break;

        //wavファイルを作成する
        //aacバイナリーデータ -> aacファイル -> wavファイル
        //※aacファイルは削除する
        DateTime dt = DateTime.Now;
        string fn = dt.ToString("yyMMdd_HHmmss");
        yield return cW.create(httpReq.aacBinaryData, fn);

        //wavファイルをAudioClipにセットして再生する
        string filePath = folderPath + "/" + fn + ".wav";
        yield return playAudioClip(filePath);
    }

    private void Start()
    {
        folderPath = Application.dataPath + @"/StreamingAssets/aacToWav";
        httpReq = new httpRequest();
        cW = new createWav(folderPath);
        audioSource = GetComponent<AudioSource>();
    }
}