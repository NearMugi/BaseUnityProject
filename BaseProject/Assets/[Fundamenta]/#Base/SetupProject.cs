using UnityEngine;

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
    /// </summary>
    public void SetDisplayOrthographic()
    {
        if (MainCamera == null) return;
        
        // 動画再生用の板のサイズ　※高さだけしか使わない。
        float height = 1080f;
        // 画像のPixel Per Unit
        float pixelPerUnit = 100f;


        sceneManage_Name.SCENE_NAME nowScene = sceneManage.Instance.GetNowSceneName();
        switch (nowScene)
        {
            case sceneManage_Name.SCENE_NAME.MAINEVENT:
                //height = ;
                break;
        }


        MainCamera.orthographic = true;
        MainCamera.orthographicSize = height / 2f / pixelPerUnit;

    }
}