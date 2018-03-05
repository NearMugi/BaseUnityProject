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
        STEP_1,     //1 : フェードアウト終了
        STEP_2,     //2 : シーンの切り替え終了
        STEP_3,     //3 : フェードイン終了
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

        //シーンを読み込むときフェードインするかどうか
        public bool isStartFadeIn;

        /// <summary>
        /// シーンを閉じるときフェードアウトするかどうか
        /// </summary>
        public bool isEndFadeOut;
        /// <summary>
        /// フェードを表示させるカメラ
        /// </summary>
        public GameObject MainCamera;

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

            Debug.LogWarning("ForcedTermination" + _nextScene);

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

        //シーンチェンジを指示されている場合
        if (flgSceneChg)
        {
            //Debug.LogWarning("[シーンチェンジ中]" + StepChgScene + "  now_sceneName:" + now_sceneName);

            switch (StepChgScene)
            {
                //--シーン切り替え処理を開始
                case SCENE_CHANGE.STEP_0:
                    //フェードアウトするorしないを判断し、必要に応じてフェードアウトする
                    if (NowCoroutine == null) NowCoroutine = StartCoroutine(isFadeOut(OnFinichedCoroutine_NextStep));
                    break;

                //--フェードアウト終了
                case SCENE_CHANGE.STEP_1:
                    //AssetBundleの読み込み＆次のシーンへ切り替え
                    if (NowCoroutine == null) NowCoroutine = StartCoroutine(ChgScene(OnFinichedCoroutine_NextStep));
                    break;

                //--シーンの切り替え終了
                case SCENE_CHANGE.STEP_2:
                    //フェードインするorしないを判断し、必要に応じてフェードインする
                    if (NowCoroutine == null) NowCoroutine = StartCoroutine(isFadeIn(OnFinichedCoroutine_NextStep));
                    break;

                //--フェードイン終了
                case SCENE_CHANGE.STEP_3:
                    //シーンの切り替えが終了しているので、色々初期化する
                    if (NowCoroutine == null) NowCoroutine = StartCoroutine(EndChgScene(OnFinichedCoroutine_NextStep));
                    break;

                default:
                    break;

            }

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
            Debug.LogWarning("NowCoroutine is null");
        }
        StepChgScene++;
    }


    bool isRunning = false;
    /// <summary>
    /// フェードアウト
    /// </summary>
    /// <returns></returns>
    private IEnumerator isFadeOut(UnityAction callback)
    {
        if (isRunning) yield break;
        isRunning = true;

        //シーン切り替えと同時にマウス入力を無効にする ※未使用
        //mouseManage.Instance.Init();
        yield return null;
        
        //フェードアウトの処理
        if(FadeManager.Instance != null)
        {
            //フェードアウトの指定がない場合は何もしない
            if (!ScenePool[(int)now_sceneName].isEndFadeOut)
            {
                //フェードインのように、α値＝1.0fにして不透明にすることはしない。
            }
            else
            {
                //フェードアウト
                FadeManager.Instance.StartFadeOut(0.5f, ScenePool[(int)now_sceneName].MainCamera);
                //Debug.LogWarning("StartFadeOut");
                yield return null;
                while (!FadeManager.Instance.GetisFadeEnd())
                {
                    yield return null;
                }
            }
        }


        //Debug.LogWarning("GetisFadeEnd");

        //コールバック
        callback();
        isRunning = false;
        yield break;

    }

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
            AssetBundleManager.Instance.ChgScene_AssetBundleLoad(next_sceneName.ToString(), ScenePool[(int)now_sceneName].MainCamera);
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

        //NowLoadingを非表示にする
        if(isUseAssetBundle) AssetBundleManager.Instance.ChgScene_AssetBundleLoadEnd(ScenePool[(int)now_sceneName].MainCamera);


        //コールバック
        callback();
        isRunning = false;
        yield break;
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    /// <returns></returns>
    private IEnumerator isFadeIn(UnityAction callback)
    {
        if (isRunning) yield break;
        isRunning = true;

        //フェードインの処理
        if (FadeManager.Instance != null)
        {
            //フェードインの指定がない場合はα値を戻す
            if (!ScenePool[(int)now_sceneName].isStartFadeIn)
            {
                //α値＝0.0fにして透明にする
                FadeManager.Instance.FadeAlpha(0.0f, ScenePool[(int)now_sceneName].MainCamera);
            }
            else
            {
                //フェードイン
                FadeManager.Instance.StartFadeIn(0.5f, ScenePool[(int)now_sceneName].MainCamera);
            }
            while (!FadeManager.Instance.GetisFadeEnd())
            {
                yield return null;
            }
        }
        
        //コールバック
        callback();
        isRunning = false;
        yield break;
    }

    /// <summary>
    /// シーンの切り替えが終了したときの後処理
    /// </summary>
    /// <returns></returns>
    public IEnumerator EndChgScene(UnityAction callback)
    {

        //Debug.LogWarning("EndChgScene");

        if (isRunning) yield break;
        isRunning = true;

        now_sceneName = next_sceneName; //現在のシーン名を更新
        ScenePool[(int)now_sceneName].ActiveFlg = true;
        SetupProject.Instance.SetDisplayOrthographic();

        //次のシーン名(編集)と次のシーン名を一致させる
        ScenePool[(int)now_sceneName].nextNum_Edit = ScenePool[(int)now_sceneName].nextNum;

        //シーンチェンジのフラグを初期値に戻す
        StepChgScene = SCENE_CHANGE.STEP_0;

        //シーンチェンジ指示をfalseにする
        flgSceneChg = false;
        isRunning = false;

        //マウス入力を有効にする ※未使用
        //mouseManage.Instance.ReStart();
        yield return null;

        //コールバック
        callback();
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
