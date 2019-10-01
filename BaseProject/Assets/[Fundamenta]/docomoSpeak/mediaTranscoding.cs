using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class mediaTranscoding : MonoBehaviour {
    private string folderPath;
    //    private string pyExePath = @"C:\Users\Kuroda-NotePC\Anaconda3\python.exe";
    private string pyExePath = @"C:\Users\Teppei\Anaconda3\python.exe";

    private string pyCodePath = @"/transcodingToWav.py";
    private string pyCodePath_Binary = @"/transcodingBinaryToWav.py";

    IEnumerator transcodingToWav (string fn) {
        //外部プロセスの設定
        ProcessStartInfo processStartInfo = new ProcessStartInfo () {
            FileName = pyExePath, //実行するファイル(python)
            UseShellExecute = false, //シェルを使うかどうか
            CreateNoWindow = true, //ウィンドウを開くかどうか
            RedirectStandardOutput = true, //テキスト出力をStandardOutputストリームに書き込むかどうか
            Arguments = folderPath + pyCodePath + " " + fn + " " + folderPath, //実行するスクリプト 引数(複数可)
        };

        //外部プロセスの開始
        Process process = Process.Start (processStartInfo);
        yield return null;

        //ストリームから出力を得る
        StreamReader streamReader = process.StandardOutput;
        //string str = streamReader.ReadLine ();
        string str = streamReader.ReadToEnd ();

        //外部プロセスの終了
        process.WaitForExit ();
        process.Close ();

        //実行
        print (str);
        yield break;
    }

    IEnumerator transcodingToWav (byte[] binary) {
        int l = binary.Length;
        System.Text.StringBuilder sb = new System.Text.StringBuilder ();
        for (int i = 0; i < l; i++) {
            sb.Append (binary[i]);
            sb.Append (",");
        }
        UnityEngine.Debug.Log (sb.ToString ());

        //外部プロセスの設定
        ProcessStartInfo processStartInfo = new ProcessStartInfo () {
            FileName = pyExePath, //実行するファイル(python)
            UseShellExecute = false, //シェルを使うかどうか
            CreateNoWindow = true, //ウィンドウを開くかどうか
            RedirectStandardOutput = true, //テキスト出力をStandardOutputストリームに書き込むかどうか
            Arguments = folderPath + pyCodePath_Binary + " " + folderPath + " " + sb.ToString (), //実行するスクリプト 引数(複数可)
        };

        //外部プロセスの開始
        Process process = Process.Start (processStartInfo);
        yield return null;

        //ストリームから出力を得る
        StreamReader streamReader = process.StandardOutput;
        string str = streamReader.ReadToEnd ();

        //外部プロセスの終了
        process.WaitForExit ();
        process.Close ();

        //実行
        print (str);
        yield break;
    }

    Process process;
    private void callBatFile (string batFilePath) {
        // 他のプロセスが実行しているなら行わない
        if (process != null) return;

        // 新規プロセスを作成し、batファイルのパスを登録
        process = new Process ();
        process.StartInfo.FileName = folderPath + batFilePath;

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

    public void createWav (byte[] binary) {
        //string filePath = folderPath + @"/hoge.aac";
        //StartCoroutine (transcodingToWav (binary));
        callBatFile (@"/aacToWav.bat");
    }

    // Start is called before the first frame update
    void Start () {
        folderPath = Application.dataPath + @"/StreamingAssets";

        //string filePath = folderPath + @"/hoge.aac";
        //StartCoroutine(transcodingToWav(filePath));
    }

    // Update is called once per frame
    void Update () {

    }
}