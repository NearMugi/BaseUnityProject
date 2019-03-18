using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
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

    
    static sceneManage_Name.SCENE_NAME now_sceneName;   //現在のシーン名

    [SerializeField]
    bool isUseAssetBundle;  //アセットバンドルの使用有無

    [SerializeField]
    List<sceneData> ScenePool;
    
    //処理用
    Coroutine NowCoroutine;

    //+++++++++++++++++++++++
    //シーン切り替え関係
    //
    bool flgSceneChg;           //シーンチェンジ指定フラグ　trueになると次のシーンへ切り替わる ※外部から指定
    sceneManage_Name.SCENE_NAME next_sceneName;   //次のシーン名
    //シーン切り替え時の制御用
    enum SCENE_CHANGE
    {
        STEP_0 = 0, //0 : シーン切り替え処理を開始
        STEP_1,     //1 : 切り替え後の初期化
    }
    SCENE_CHANGE StepChgScene;

    //+++++++++++++++++++++++

    //リストの定義
    [System.Serializable]
    public class sceneData
    {
        public sceneManage_Name.SCENE_NAME name; //シーン名
        public sceneManage_Name.SCENE_NAME nextNum; //次のシーン
        [HideInInspector]
        public sceneManage_Name.SCENE_NAME nextNum_Edit;    //処理中に指定したときの次のシーン　※外部から指定

        //処理用
        [HideInInspector]
        public bool ActiveFlg;  //アクティブならTrue
    }

    /// <summary>
    /// 現在のシーン名を取得
    /// </summary>
    /// <returns></returns>
    public sceneManage_Name.SCENE_NAME GetNowSceneName()
    {
        return now_sceneName;
    }
    /// <summary>
    /// 現在のシーン名とアクティブになっているシーン名が一致しているか判断する
    /// <para>true…一致している　false…一致していない</para>
    /// </summary>
    /// <returns></returns>
    public bool jdgNowSceneName(string _activeScene)
    {
        bool sw = false;
        //小文字に揃えて比較
        if (_activeScene.ToLower() == now_sceneName.ToString().ToLower()) sw = true;

        //Debug.LogWarning("NowScene:" + now_sceneName + " , ActiveScene:" + _activeScene + " , SW:" + sw);

        return sw;
    }

    /// <summary>
    /// シーンチェンジ指定フラグをTrueにしてシーンを切り替える
    /// </summary>
    public void SetflgSceneChgTrue()
    {
        flgSceneChg = true;
        StepChgScene = SCENE_CHANGE.STEP_0;
    }

    /// <summary>
    /// 次に移行するシーンを指定する
    /// </summary>
    /// <param name="now">現在のシーン名(string)</param>
    /// <param name="next">移行するシーン名</param>
    public void SetnextNum_Edit(string now, sceneManage_Name.SCENE_NAME next)
    {
        int num = 0;
        bool sw = false;

        //現在のシーン名と一致するsceneManage_Name.SCENE_NAMEを検索する
        for (int i = 0; i < (int)sceneManage_Name.SCENE_NAME.TERMINAL; i++)
        {
            if (((sceneManage_Name.SCENE_NAME)i).ToString().ToUpper() == now.ToUpper())
            {
                sw = true;
                num = i;
                break;
            }
        }
        
        if (!sw) return;
        ScenePool[num].nextNum_Edit = next;

        //Debug.LogWarning("SetnextNum_Edit" + ScenePool[num].nextNum_Edit);
    }

    //次のシーンを指定して切り替え
    public void chgScene(sceneManage_Name.SCENE_NAME _nextScene)
    {
        if (_nextScene != sceneManage_Name.SCENE_NAME.NONE)
        {

            Debug.LogWarning("Forced Termination : " + _nextScene);

            SetnextNum_Edit(SceneManager.GetActiveScene().name, _nextScene);
            SetflgSceneChgTrue();
        }
    }


    // Use this for initialization
    void Start()
    {
        //外部から指定する移行先シーン名を初期化する
        foreach(sceneData s in ScenePool)
        {
            s.nextNum_Edit = s.nextNum;
        }


        //先頭のステータスを有効にする
        //ゼロ番目はエラー用
        now_sceneName = (sceneManage_Name.SCENE_NAME)((int)sceneManage_Name.SCENE_NAME.NONE);
        flgSceneChg = true;
        StepChgScene = SCENE_CHANGE.STEP_0;

        NowCoroutine = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!flgSceneChg) return;

        
        //シーンチェンジを指示されている場合
        //Debug.LogWarning("[シーンチェンジ中]" + StepChgScene + "  now_sceneName:" + now_sceneName);
        switch (StepChgScene)
        {
            //AssetBundleの読み込み＆次のシーンへ切り替え
            case SCENE_CHANGE.STEP_0:
                if (NowCoroutine == null) NowCoroutine = StartCoroutine(ChgScene(OnFinichedCoroutine_NextStep));
                break;

            //切り替え後の初期化
            case SCENE_CHANGE.STEP_1:
                if (NowCoroutine == null) NowCoroutine = StartCoroutine(EndChgScene());
                break;

            default:
                break;

        }

    }

    void OnFinichedCoroutine_NextStep()
    {
        //Debug.LogWarning("OnFinichedCoroutine_NextStep ");
        if (NowCoroutine != null)
        {
            //Debug.LogWarning("[sceneManage][StopCoroutine] StepChgScene" + StepChgScene);
            StopCoroutine(NowCoroutine);
            NowCoroutine = null;
        } else
        {
            //Debug.LogWarning("NowCoroutine is null");
        }
        StepChgScene++;
    }


    bool isRunning = false;
    /// <summary>
    /// AssetBundleの読み込み＆次のシーンへ切り替え
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChgScene(UnityAction callback)
    {
        if (isRunning) yield break;
        isRunning = true;
        
        //次のシーン名を取得する　処理中に変更されている場合はそっちを採用
        next_sceneName = ScenePool[(int)now_sceneName].nextNum;
        if (next_sceneName != ScenePool[(int)now_sceneName].nextNum_Edit)
        {
            next_sceneName = ScenePool[(int)now_sceneName].nextNum_Edit;
        }
        //Debug.LogWarning("次のシーン：" + next_sceneName);


        //AssetBundleを読み込む
        if (isUseAssetBundle)
        {
            //AssetBundleManager.Instance.ChgScene_AssetBundleLoad(next_sceneName.ToString(), ScenePool[(int)now_sceneName].MainCamera);
            yield return null;
            while (!AssetBundleManager.Instance.GetisLoadEnd())
            {
                yield return null;
            }
        }

        //シーン切り替え
        ScenePool[(int)now_sceneName].ActiveFlg = false;
        SceneManager.LoadScene(next_sceneName.ToString());
        yield return null;
        
        //コールバック
        callback();
        isRunning = false;
        yield break;
    }


    /// <summary>
    /// シーンの切り替えが終了したときの後処理
    /// </summary>
    /// <returns></returns>
    public IEnumerator EndChgScene()
    {

        //Debug.LogWarning("EndChgScene");

        if (isRunning) yield break;
        isRunning = true;

        now_sceneName = next_sceneName; //現在のシーン名を更新
        ScenePool[(int)now_sceneName].ActiveFlg = true;

        //次のシーン名(編集)と次のシーン名を一致させる
        ScenePool[(int)now_sceneName].nextNum_Edit = ScenePool[(int)now_sceneName].nextNum;

        //シーンチェンジのフラグを初期値に戻す
        StepChgScene = SCENE_CHANGE.STEP_0;

        //シーンチェンジ指示をfalseにする
        flgSceneChg = false;
        isRunning = false;
        
        yield break;


    }
    
    public string GetStatusLabel(sceneData d)
    {
        string s = "[[none]]";

        if (d.name.ToString() != "")
        {
            s = d.name.ToString();
        }

        return s;
    }
}
