using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class mediaTranscoding : MonoBehaviour {
    private string folderPath;
    private string pyExePath = @"C:\Users\Kuroda-NotePC\Anaconda3\python.exe";
    //private string pyExePath = @"C:\Users\Teppei\Anaconda3\python.exe";

    private string pyCodePath = @"transcodingToWav.py";
    Process process;
    private void callVbs (string batFilePath, string tmpAacFile) {
        // 他のプロセスが実行しているなら行わない
        if (process != null) return;

        // 新規プロセスを作成し、batファイルのパスを登録
        process = new Process ();
        process.StartInfo.FileName = folderPath + batFilePath;
        process.StartInfo.Arguments = pyExePath + " " +
            pyCodePath + " " +
            folderPath + " " +
            tmpAacFile;

        // 外部プロセスの終了を検知するための設定
        process.EnableRaisingEvents = true;
        process.Exited += process_Exited;

        // 外部プロセスを実行
        process.Start ();
    }

    // 外部プロセスの終了を検知してプロセスを終了
    void process_Exited (object sender, System.EventArgs e) {
        //UnityEngine.Debug.Log ("process_Exited");
        process.Dispose ();
        process = null;
    }

    public IEnumerator createWav (byte[] aacBinary, string word) {
        string aacFilePath = "/" + word + ".aac";
        string wavFilePath = "/" + word + ".wav";
        //binaryからaacファイルを作る
        using (FileStream stream = new FileStream (folderPath + aacFilePath, FileMode.Create)) {
            stream.Write (aacBinary, 0, aacBinary.Length);
        }

        //aacファイルが出来るまで待つ
        while (!System.IO.File.Exists (folderPath + aacFilePath)) {
            yield return null;
        }
        UnityEngine.Debug.Log ("Created aac :" + aacFilePath);

        //wavファイルの作成、aacファイルの削除
        callVbs (@"/aacToWav.vbs", aacFilePath);
        //wavファイルが出来るまで待つ
        while (!System.IO.File.Exists (folderPath + wavFilePath)) {
            yield return null;
        }

        yield break;
    }

    // Start is called before the first frame update
    void Start () {
        folderPath = Application.dataPath + @"/StreamingAssets/aacToWav";
    }
}