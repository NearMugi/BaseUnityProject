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
}