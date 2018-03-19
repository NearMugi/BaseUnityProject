﻿using UnityEngine;

public class SetupProject : MonoBehaviour
{

    #region Singleton

    private static SetupProject instance;

    public static SetupProject Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SetupProject)FindObjectOfType(typeof(SetupProject));

                if (instance == null)
                {
                    Debug.LogError(typeof(SetupProject) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton


    [SerializeField]
    int DisplayCount;   //ディスプレイ数
    [SerializeField]
    Camera MainCamera;

    //動画再生用の板のサイズ
    float MovieScreenWidth;
    float MovieScreenHeight;

    public string DebugList()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- SETUP PROJECT INFO ---");
        sb.Append("\n");

        if(MainCamera != null)
        {
            sb.Append("[MainMovieScreenSize]");
            sb.Append("\n");
            sb.Append(MovieScreenWidth);
            sb.Append(" : ");
            sb.Append(MovieScreenHeight);
            sb.Append("\n");
            sb.Append("[DiplayInfo]");
            sb.Append("\n");

            for (int i = 0; i < DisplayCount && i < Display.displays.Length; i++)
            {
                sb.Append(i);
                sb.Append(" : ");
                sb.Append(Display.displays[i].renderingWidth);
                sb.Append(" , ");
                sb.Append(Display.displays[i].renderingHeight);
                sb.Append("\n");
            }

            sb.Append("[MainCameraInfo]");
            sb.Append("\n");
            sb.Append("orthographicSize : ");
            sb.Append(MainCamera.orthographicSize);
            sb.Append("\n");
            sb.Append("ViewRect :");
            sb.Append(MainCamera.rect);
            sb.Append("\n");
        } else
        {
            sb.Append("... No Information \n");
        }

        return sb.ToString();
    }




    // 初期化
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SetDisplay();
    }

    //使用するディスプレイ数を指定
    void SetDisplay()
    {
        if (DisplayCount <= 0)
        {
            Debug.LogWarning("[SetDisplay]ディスプレイ数の指定に失敗");
            return;
        }
        for (int i = 0; i < DisplayCount && i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }

    /// <summary>
    /// 表示に使用するディスプレイごとにOrthographicのSizeを変更する
    /// <para>「ディスプレイの解像度に合わせて動画を全画面表示する」ことを基準にしている。</para>
    /// <para>カメラの設定が書き換えられるので、インスペクター上で設定しても変わってしまう点に注意。</para>
    /// </summary>
    public void SetDisplayOrthographic()
    {
        if (MainCamera == null) return;

        // 画像のPixel Per Unit
        float pixelPerUnit = 100f;
        
        int displayNo = MainCamera.targetDisplay;
        float Screen_width;
        float Screen_height;
        try
        {
            Screen_width = Display.displays[displayNo].renderingWidth;
            Screen_height = Display.displays[displayNo].renderingHeight;

        }
        catch (System.Exception)
        {   
            //デバッグで起動させた場合はDisplayを正しく認識できないのでエラーになる。
            //回避策として起動しているディスプレイのサイズを取得する
            Screen_width = Screen.width;
            Screen_height = Screen.height;

        }

        // 動画再生用の板のサイズ(初期設定)
        MovieScreenWidth = Screen_width;
        MovieScreenHeight = Screen_height;
        //※もし板のサイズを変えるのであれば、ここに記載



        float aspect = Screen_height / Screen_width;
        float bgAspect = MovieScreenHeight / MovieScreenWidth;

        //orthographicの設定
        MainCamera.orthographic = true;
        MainCamera.orthographicSize = MovieScreenHeight / 2f / pixelPerUnit;



        //viewportRectの設定
        MainCamera.rect = new Rect(0f, 0f, 1f, 1f);
        if (bgAspect > aspect)
        {
            // 倍率
            float bgScale = MovieScreenHeight / Screen.height;
            // viewport rectの幅
            float camWidth = MovieScreenWidth / (Screen.width * bgScale);
            // viewportRectを設定
            MainCamera.rect = new Rect((1f - camWidth) / 2f, 0f, camWidth, 1f);
        }
        else if(bgAspect < aspect)
        {
            // 倍率
            float bgScale = MovieScreenWidth / Screen.width;
            // viewport rectの幅
            float camHeight = MovieScreenHeight / (Screen.height * bgScale);
            // viewportRectを設定
            MainCamera.rect = new Rect(0f, (1f - camHeight) / 2f, 1f, camHeight);
        }
    }
}