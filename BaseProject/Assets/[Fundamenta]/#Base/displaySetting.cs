using UnityEngine;
using System.Text;
public class displaySetting : MonoBehaviour
{

    #region Singleton

    private static displaySetting instance;

    public static displaySetting Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (displaySetting)FindObjectOfType(typeof(displaySetting));

                if (instance == null)
                {
                    Debug.LogError(typeof(displaySetting) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton

    int displayCnt;   //ディスプレイ数
    string debugDisplayList;
    Camera MainCamera;  //そのシーンでのメインカメラ


    public string DebugList()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("--- DISPLAY INFO ---");
        sb.Append("\n");
        sb.Append(debugDisplayList);

        sb.Append("[MainCamera]");
        sb.Append("\n");
        if (MainCamera == null)
        {
            sb.Append("Nothing\n");
        }
        else
        {
            sb.Append("orthographicSize : ");
            sb.Append(MainCamera.orthographicSize);
            sb.Append("\n");
            sb.Append("ViewRect :");
            sb.Append(MainCamera.rect);
            sb.Append("\n");
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
        debugDisplayList = string.Empty;
        displayCnt = Display.displays.Length;
        if (displayCnt <= 0)
        {
            Debug.LogWarning("[SetDisplay]ディスプレイ数の指定に失敗");
            return;
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < displayCnt; i++)
        {
            Display.displays[i].Activate();
            sb.Append(i);
            sb.Append(" : ");
            sb.Append(Display.displays[i].renderingWidth);
            sb.Append(" , ");
            sb.Append(Display.displays[i].renderingHeight);
            sb.Append("\n");
        }
        debugDisplayList = sb.ToString();
    }

    /// <summary>
    /// カメラの設定を解像度(1920*1080)に合わせる
    /// </summary>
    /// <param name="_TargetCamera">表示するカメラ</param>
    public void SetDisplayOrthographic(Camera _TargetCamera)
    {
        SetDisplayOrthographic(_TargetCamera, 1920f, 1080f);
    }

    /// <summary>
    /// カメラの設定をディスプレイの解像度に合わせる
    /// </summary>
    /// <param name="_TargetCamera">表示するカメラ</param>
    /// <param name="_width">解像度(幅)</param>
    /// <param name="_height">解像度(高さ)</param>
    public void SetDisplayOrthographic(Camera _TargetCamera, float _width, float _height)
    {
        MainCamera = _TargetCamera;
        if (MainCamera == null) return;

        // 動画再生用の板のサイズ
        float width = _width;
        float height = _height;
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
        float aspect = Screen_height / Screen_width;
        float bgAspect = height / width;

        //orthographicの設定
        MainCamera.orthographic = true;
        MainCamera.orthographicSize = height / 2f / pixelPerUnit;



        //viewportRectの設定
        MainCamera.rect = new Rect(0f, 0f, 1f, 1f);
        if (bgAspect > aspect)
        {
            // 倍率
            float bgScale = height / Screen.height;
            // viewport rectの幅
            float camWidth = width / (Screen.width * bgScale);
            // viewportRectを設定
            MainCamera.rect = new Rect((1f - camWidth) / 2f, 0f, camWidth, 1f);
        }
        else if (bgAspect < aspect)
        {
            // 倍率
            float bgScale = width / Screen.width;
            // viewport rectの幅
            float camHeight = height / (Screen.height * bgScale);
            // viewportRectを設定
            MainCamera.rect = new Rect(0f, (1f - camHeight) / 2f, 1f, camHeight);
        }
    }
}