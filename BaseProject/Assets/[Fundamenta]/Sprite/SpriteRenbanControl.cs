using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SpriteRenbanControl : MonoBehaviour {
    
    [HideInInspector]
    public bool FlgSpriteEnd;   //最終フレームまでたどり着いたときTrue(ループの場合は除外)

    public enum LAST_FRM{
        END = 0,
        KEEP,
        LOOP
    }
    [Header("※データはAssetbundleに入っている")]
    [Header("※sp_nameにはスプライト名(ヘッダー部分)を指定")]
    [Header("※SetObjectにはスプライトを表示させるオブジェトを指定")]
    [SerializeField]
    string sp_label;    //インスペクターで表示するラベル
    [SerializeField]
    string sp_name;
    [SerializeField]
    int sp_keta;
    [SerializeField]
    int sp_cnt;  
    [SerializeField]
    float int_time = 33f;
    [SerializeField]
    LAST_FRM flgEnd;

    [SerializeField]
    GameObject SetObject;
    SpriteRenderer _sp;
    int now_sp;

    float timeWait;
    float timeElapsed;
    
    public string GetLabel()
    {
        return sp_label;
    }

    public void SetSetObject(GameObject ob)
    {
        SetObject = ob;
    }

    public void InitSprite()
    {
        //Debug.LogWarning(sp_name + " SpriteRenbanControl Init");
        if (SetObject == null) return;

        _sp = SetObject.GetComponent<SpriteRenderer>();
        if (_sp == null)
        {
            SetObject.AddComponent<SpriteRenderer>();
            _sp = SetObject.GetComponent<SpriteRenderer>();
        }

        now_sp = 0;
        FlgSpriteEnd = false;
        timeWait = int_time / 1000;
        timeElapsed = timeWait;

        //Debug.LogWarning(sp_name + " timeWait : " + timeWait);

        //ヘッダー名から桁数と枚数を取得
        SearchSprite(sp_name, ref sp_keta, ref sp_cnt);

    }

    public LAST_FRM GetLastFrm()
    {
        return flgEnd;
    }
    
    public bool GetEnd()
    {
        return FlgSpriteEnd;
    }


    void Start()
    {
        //初期化はOnEnableではなくStartで。
        //外部からの_spの設定タイミングに影響。
        InitSprite();
    }


	// Update is called once per frame
	void Update () {

        if (FlgSpriteEnd)
        {
            return;
        }

        //指定時間まで処理をしない。
        timeElapsed += Time.deltaTime;
        //Debug.LogWarning("timeElapsed :" + timeElapsed + " timeWait :" + timeWait);
        if (timeElapsed < timeWait) return;
        timeElapsed = 0;

        //Debug.LogWarning(sp_name + " Frm : " + now_sp + " FlgSpriteEnd : " + FlgSpriteEnd);

        //次のスプライトを選択
        if (++now_sp >= sp_cnt)
        {
            //Debug.LogWarning(sp_name + " Sprite ArriveLastFrm : " + now_sp);
            //最終フレームに到達したとき、処理を分ける。
            switch (flgEnd)
            {
                case LAST_FRM.KEEP:
                    now_sp = sp_cnt - 1;
                    FlgSpriteEnd = true;
                    break;
                case LAST_FRM.LOOP:
                    now_sp = 0;
                    break;
                default:
                    if (_sp != null) _sp.sprite = null;
                    now_sp = 0;
                    FlgSpriteEnd = true;
                    break;
            }
            
        }

        if (!FlgSpriteEnd)
        {
            string s;
            s = sp_name + "_" + string.Format("{0:D" + sp_keta +"}", now_sp);
            //        Debug.Log(" SpriteRenbanControl Update : " + s);
            if (_sp != null)
            {
                //アセットバンドル関係を削除したのでここで何かしなければならない。(覚えていない・・・)
//                _sp.sprite = AssetBundleManager.Instance.GetSpriteFromAssetBundle(s, SceneManager.GetActiveScene().name);
            }
        }
        
    }
    

    public void SearchSprite(string fileName, ref int keta, ref int cnt)
    {
        if (fileName == "") return;

//アセットバンドル関係を削除したので何かしなければならない.(今後使う？？？)
#if false
        //桁数を調べる。
        //"ヘッダー_nnnnn"となっている前提
        string s = "";
        for (int i=5; i>0; i--)
        {
            s = fileName + "_" + string.Format("{0:D" + i + "}", 0);
            if (AssetBundleManager.Instance.GetSpriteFromAssetBundle(s, SceneManager.GetActiveScene().name) != null){
                keta = i;
                break;
            }
        }
        //枚数を調べる。
        cnt = AssetBundleManager.Instance.CntSpriteFromAssetBundle(fileName, SceneManager.GetActiveScene().name);

        Debug.LogWarning("[" + fileName + "] keta:" + keta + " cnt:" + cnt);
#endif 
    }
}
