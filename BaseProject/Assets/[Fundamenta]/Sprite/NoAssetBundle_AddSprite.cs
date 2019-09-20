using UnityEngine;
using System.Collections;

/// <summary>
/// AssetBundleを使用しない時の連番再生用クラス
/// </summary>
public class NoAssetBundle_AddSprite : MonoBehaviour {
    [SerializeField]
    [Header("---再生するスプライトオブジェクト---")]
    GameObject _ob;

    [SerializeField]
    bool flg_Start;

    NoAssetBundle_SpriteControl _sp;
    
    public bool GetSpriteEnd()
    {
        return _ob.GetComponent<NoAssetBundle_SpriteControl>().FlgSpriteEnd;
    }

    public void InitSpriteEnd()
    {
        _ob.GetComponent<NoAssetBundle_SpriteControl>().FlgSpriteEnd = false;
    }

    public void SpriteEnd()
    {
//        Debug.Log(_ob.name + " AddSprite SpriteEnd");
        _ob.GetComponent<SpriteRenderer>().sprite = null;
        _ob.SetActive(false);
        flg_Start = false;
    }

    public void SpriteStart()
    {
//        Debug.Log(_ob.name + " AddSprite SpriteStart");
        _ob.SetActive(true);
        _sp = _ob.GetComponent<NoAssetBundle_SpriteControl>();
        _sp.FlgSpriteEnd = false;
        _sp.flgPlay = true;
        _sp.InitSprite();
    }

    public void SpritePlayOnOff(bool _sw)
    {
        _sp.flgPlay = _sw;
    }
    
	// Update is called once per frame
	void Update () {
        if (flg_Start)
        {
            SpriteStart();
            flg_Start = false;
        }


        if (_sp != null && _sp.FlgSpriteEnd )
        {
            if(_sp.GetLastFrm() == NoAssetBundle_SpriteControl.LAST_FRM.END)
            {
                _ob.SetActive(false);
            }
        }
	}
}
