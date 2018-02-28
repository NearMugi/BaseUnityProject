using UnityEngine;
using System.Collections;

/// <summary>
/// AssetBundleを使用しない時の連番再生用クラス
/// </summary>
public class NoAssetBundle_SpriteControl : MonoBehaviour {
    
    [HideInInspector]
    public bool FlgSpriteEnd;   //最終フレームまでたどり着いたときTrue(ループの場合は除外)

    public enum LAST_FRM{
        END = 0,
        KEEP,
        LOOP
    }

    [HideInInspector]
    public bool flgPlay;

    [SerializeField]
    [Header("※データはResourcesフォルダに入れる")]
    [Header("※sp_nameにはスプライト名(ヘッダー部分)を指定")]
    string sp_name;
    [SerializeField]
    int sp_keta;
    [SerializeField]
    int sp_cnt;  
    [SerializeField]
    float int_time;
    [SerializeField]
    LAST_FRM flgEnd;

    SpriteRenderer _sp;
    int now_sp;

    float timeWait;
    float timeElapsed;
    
    
    public void InitSprite()
    {
        //Debug.LogWarning(sp_name + " SpriteControl Init");

        _sp = gameObject.GetComponent<SpriteRenderer>();
        if (_sp == null)
        {
            gameObject.AddComponent<SpriteRenderer>();
            _sp = gameObject.GetComponent<SpriteRenderer>();
        }

        now_sp = 0;
        FlgSpriteEnd = false;
        timeWait = int_time / 1000;
        timeElapsed = timeWait;
    }

    public LAST_FRM GetLastFrm()
    {
        return flgEnd;
    }
    
	
	// Update is called once per frame
	void FixedUpdate () {

        if (!flgPlay)
        {
            return;
        }

        if (FlgSpriteEnd)
        {
            return;
        }

        //指定時間まで処理をしない。
        timeElapsed += Time.fixedDeltaTime;
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
            s = sp_name + "_" + string.Format("{0:D5}", now_sp);
            //        Debug.Log(" SpriteControl Update : " + s);
            if (_sp != null) _sp.sprite = GetSprite(sp_name, s);
        }
        
    }

    // スプライトの取得
    // @param fileName ファイル名
    // @param spriteName スプライト名
    public Sprite GetSprite(string fileName, string spriteName)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(fileName);

        Sprite _GetSp = System.Array.Find<Sprite>(sprites, (sprite) => sprite.name.Equals(spriteName));

        if(_GetSp == null)
        {
            Debug.LogWarning("SpriteControl.cs : 指定したスプライト画像が見つかりません。 [" + spriteName + "]");
        }

        return _GetSp;
    }

}
