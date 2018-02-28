using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AssetBundleManager : MonoBehaviour
{

    #region Singleton

    private static AssetBundleManager instance;

    public static AssetBundleManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (AssetBundleManager)FindObjectOfType(typeof(AssetBundleManager));

                if (instance == null)
                {
                    Debug.LogError(typeof(AssetBundleManager) + "is nothing");
                }
            }

            return instance;
        }
    }

    #endregion Singleton


    private bool isLoadEnd;     //アセットバンドルのロード完了(true)か未完了(false)か
    private bool isDispNowLoading;  //NowLoadingを表示する(true)・しない(false)


    [SerializeField]
    GameObject NowLoadingOb;     //NowLoading用オブジェクト
    Material NowLoadingMat;

    private int version = 0;    //バージョンはゼロで固定
    private AssetBundle _bundle;
    

    public void ChgScene_AssetBundleLoad(string _assetbundleName, GameObject camera)
    {
        //NowLoadingを表示、明滅させるコルーチンを実行
        StartCoroutine(DispNowLoadingCoroutine(camera));

        //アセットバンドルのロード
        StartCoroutine(LoadAssetBundleCoroutine(OnFinishedCoroutine, _assetbundleName));
    }

    public void ChgScene_AssetBundleLoadEnd(GameObject camera)
    {
        //NowLoadingを非表示
        StartCoroutine(DispNowLoadingEnd(camera));
    }

    public IEnumerator DispNowLoadingCoroutine(GameObject camera)
    {
        //「NowLoading」を表示するオブジェクトの位置をカメラの位置に移動させる
        Transform t = this.GetComponent<Transform>();
        t.position = camera.GetComponent<Transform>().position;
        //回転も
        Transform t_now = NowLoadingOb.GetComponent<Transform>();
        t_now.rotation = camera.GetComponent<Transform>().rotation;

        GameObject _child = this.NowLoadingOb.transform.Find("Nowloading").gameObject;
        this.NowLoadingMat = _child.GetComponent<Renderer>().material;

        yield return null;

        NowLoadingOb.SetActive(true);
        isDispNowLoading = true;


        float time = 0;
        float interval = 1.0f;
        Color col = this.NowLoadingMat.color;
        while (isDispNowLoading)
        {
            col.a = 1.0f;
            this.NowLoadingMat.color = col;
            time = 0;
            while (time <= interval / 3)
            {
                time += Time.deltaTime;
                yield return null;
            }

            time = 0;
            while (time <= interval)
            {
                //α値を1.0→0.0へ
                col.a = Mathf.Lerp(1f, 0f, time / interval);
                this.NowLoadingMat.color = col;
                time += Time.deltaTime;
                yield return null;
            }
            

            time = 0;
            while (time <= interval)
            {
                //α値を0.0→1.0へ
                col.a = Mathf.Lerp(0f, 1f, time / interval);
                this.NowLoadingMat.color = col;

                time += Time.deltaTime;
                yield return null;
            }


            yield return null;
        }
        
        yield break;

    }

    public IEnumerator DispNowLoadingEnd(GameObject camera)
    {
        //「NowLoading」を表示するオブジェクトの位置をカメラの位置に移動させる
        Transform t = this.GetComponent<Transform>();
        t.position = camera.GetComponent<Transform>().position;
        //回転も
        Transform t_now = NowLoadingOb.GetComponent<Transform>();
        t_now.rotation = camera.GetComponent<Transform>().rotation;

        yield return null;

        //NowLoadingを非表示
        isDispNowLoading = false;   //コルーチンを停止
        NowLoadingOb.SetActive(false);
        yield break;
    }

    string GetStreamingAssetsPath(string assetbundleName)
    {
        var platform = "Standalone";
#if UNITY_ANDROID
            platform="Android";
#endif
#if UNITY_IOS
            platform="iOS";
#endif
        return "file://" + Application.streamingAssetsPath + "/AssetBundles/" + platform + "/" + assetbundleName;
    }





    /// <summary>
    /// アセットバンドルのロード
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="_assetbundleName">取得するアセットバンドル名</param>
    /// <returns></returns>
    public IEnumerator LoadAssetBundleCoroutine(UnityAction<bool> callback, string _assetbundleName)
    {
        callback(false);

        string bundleUrl = GetStreamingAssetsPath(_assetbundleName.ToLower());
        _bundle = AssetBundleManager_Base.Instance.getAssetBundle(bundleUrl, version);
        yield return null;

        //アセットバンドルをロードする
        //※既に取得しようとするアセットバンドルがロード済みの場合、何もしない
        if (!_bundle)
        {
            yield return StartCoroutine(AssetBundleManager_Base.Instance.downloadAssetBundle(bundleUrl, version));
            _bundle = AssetBundleManager_Base.Instance.getAssetBundle(bundleUrl, version);
            //Debug.LogWarning("[アセットバンドルを初めてロード] " + _bundle.name);
            yield return null;
        }

        callback(true);
        yield break;

    }

    // コルーチンからのコールバック
    public void OnFinishedCoroutine(bool flg)
    {
        isLoadEnd = flg;
    }

    public bool GetisLoadEnd()
    {
        return isLoadEnd;
    }


    private Sprite _getSpriteData;
    /// <summary>
    /// Asset BundleからSpriteを取得
    /// </summary>
    /// <param name="fileName">取得するファイル名</param>
    /// <param name="_assetbundleName">アセットバンドル名(基本的にシーン名と同じ)</param>
    /// <returns></returns>
    public Sprite GetSpriteFromAssetBundle(string fileName, string _assetbundleName)
    {
        //        isGetSpriteEnd = false;
        //        StartCoroutine(GetSpriteCoroutine(OnFinished_SpriteCoroutine, OnFinishedCoroutine_GetSprite, fileName, _assetbundleName));
        //        return _getSpriteData;
        Sprite _getdata = null;
        string bundleUrl = GetStreamingAssetsPath(_assetbundleName.ToLower());
        _bundle = AssetBundleManager_Base.Instance.getAssetBundle(bundleUrl, version);

#if false
        Debug.LogWarning(_bundle.name +":" + bundleUrl + "+++++++++++++++");
        string[] s = _bundle.GetAllAssetNames();
        foreach(string t in s)
        {
            Debug.LogWarning(t);
        }
#endif
        //※SpriteではなくTexture2Dになる？
        //※拡張子を指定しないやり方があるといいな。

        //.jpgで取得を試みる
        _getdata = _bundle.LoadAsset<Sprite>(string.Format("{0}.jpg", fileName));
        if (_getdata != null) return _getdata;

        //.pngで取得を試みる
        _getdata = _bundle.LoadAsset<Sprite>(string.Format("{0}.png", fileName));
        if (_getdata != null) return _getdata;


        Debug.LogWarning("GetSpriteFromAssetBundle : 指定したスプライト画像が見つかりません。 [" + fileName + "]");
        return _getdata;

    }

    /// <summary>
    /// スプライト(連番)専用　連番の枚数を取得する
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="_assetbundleName"></param>
    /// <returns></returns>
    public int CntSpriteFromAssetBundle(string fileName, string _assetbundleName)
    {
        int _cnt = 0;

        string bundleUrl = GetStreamingAssetsPath(_assetbundleName.ToLower());
        _bundle = AssetBundleManager_Base.Instance.getAssetBundle(bundleUrl, version);

        string[] s = _bundle.GetAllAssetNames();

        foreach (string t in s)
        {
            if (t.IndexOf(fileName) > 0) _cnt++;
        }
        
        return _cnt;
    }


    /// <summary>
    /// Asset BundleからGameObjectを取得　※シーン名と一致するアセットバンドルはロード済み
    /// </summary>
    /// <param name="fileName">取得するファイル名</param>
    /// <param name="_assetbundleName">アセットバンドル名(基本的にシーン名と同じ)</param>
    /// <returns></returns>
    public GameObject GetGameObjectFromAssetBundle(string fileName, string _assetbundleName)
    {

        GameObject _getdata = null;
        _getdata = _bundle.LoadAsset<GameObject>(fileName);
        if (_getdata != null) return _getdata;
        return _getdata;

    }

    /// <summary>
    /// Asset BundleからMaterialを取得　※シーン名と一致するアセットバンドルはロード済み
    /// </summary>
    /// <param name="fileName">取得するファイル名</param>
    /// <param name="_assetbundleName">アセットバンドル名(基本的にシーン名と同じ)</param>
    /// <returns></returns>
    public Material GetMaterialFromAssetBundle(string fileName, string _assetbundleName)
    {
        Material _getdata = null;
        _getdata = _bundle.LoadAsset<Material>(fileName);
        if (_getdata != null) return _getdata;
        return _getdata;
    }


}
