using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class sceneManage : MonoBehaviour
{

    #region Singleton

    private static sceneManage instance;

    public static sceneManage Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (sceneManage)FindObjectOfType(typeof(sceneManage));

                if (instance == null)
                {
                    Debug.LogError(typeof(sceneManage) + "is nothing");
                }
            }

            return instance;
        }
    }

    #endregion Singleton
    Coroutine NowCoroutine;
    /// <summary>
    /// シーンチェンジ指定フラグ
    /// </summary>
    bool flgSceneChg;
    /// <summary>
    /// 現在のシーン
    /// </summary>
    static sceneManage_Name.SCENE_NAME nowScene;
    /// <summary>
    /// 次のシーン
    /// </summary>
    [SerializeField]
    sceneManage_Name.SCENE_NAME nextScene;

    /// <summary>
    /// 次のシーンを指定
    /// </summary>
    /// <param name="_nextScene"></param>
    public void chgScene(sceneManage_Name.SCENE_NAME _nextScene)
    {
        if (_nextScene == sceneManage_Name.SCENE_NAME.NONE)
            return;
        nextScene = _nextScene;
        flgSceneChg = true;
    }

    void Start()
    {
        nowScene = sceneManage_Name.SCENE_NAME.NONE;
        flgSceneChg = true;
        NowCoroutine = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!flgSceneChg) return;
        if (NowCoroutine != null)
            StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(chgSceneCoroutine());
    }

    /// <summary>
    /// 次のシーンへ切り替えるコルーチン
    /// </summary>
    private IEnumerator chgSceneCoroutine()
    {
        //シーンチェンジ指示をfalseにする
        flgSceneChg = false;

        //シーン切り替え
        SceneManager.LoadScene(nextScene.ToString());
        yield return null;

        //現在のシーン名を更新
        nowScene = nextScene;

        yield break;
    }
}
