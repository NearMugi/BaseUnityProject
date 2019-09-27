using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class mediaTranscoding : MonoBehaviour
{
    private string folderPath;
    private string pyExePath = @"C:\Users\Kuroda-NotePC\Anaconda3\python.exe";

    private string pyCodePath = @"/transcodingToWav.py";

    IEnumerator transcodingToWav(string fn)
    {
        //外部プロセスの設定
        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            FileName = pyExePath, //実行するファイル(python)
            UseShellExecute = false, //シェルを使うかどうか
            CreateNoWindow = true, //ウィンドウを開くかどうか
            RedirectStandardOutput = true, //テキスト出力をStandardOutputストリームに書き込むかどうか
            Arguments = folderPath + pyCodePath + " " + fn + " " + folderPath, //実行するスクリプト 引数(複数可)
        };

        //外部プロセスの開始
        Process process = Process.Start(processStartInfo);
        yield return null;

        //ストリームから出力を得る
        StreamReader streamReader = process.StandardOutput;
        string str = streamReader.ReadLine();

        //外部プロセスの終了
        process.WaitForExit();
        process.Close();

        //実行
        print(str);
        yield break;
    }

    // Start is called before the first frame update
    void Start()
    {
        folderPath = Application.dataPath + @"/StreamingAssets";
        //        string filePath = folderPath + @"/hoge.aac";
        string filePath = folderPath + @"/Super.aac";
        StartCoroutine(transcodingToWav(filePath));
    }

    // Update is called once per frame
    void Update()
    {

    }
}