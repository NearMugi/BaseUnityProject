using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpriteUnitControl : MonoBehaviour {
    [SerializeField]
    bool DispOnOff;     //表示On/Off
    [SerializeField]
    string _AssetBundle; //使用するアセットバンドル　※指定なしの場合はアセットバンドル名＝シーン名
    [SerializeField]
    string _FileName;    //表示するファイル名

    SpriteRenderer _sp;

    // Use this for initialization
    void Start () {
        if (_AssetBundle == "") _AssetBundle = SceneManager.GetActiveScene().name;
        _sp = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update () {

        bool sw = false;

        //表示するかどうか判別する
        if (DispOnOff)
        {
            //何も表示していない場合、表示OK
            if(_sp.sprite == null)
            {
                sw = true;
            } else
            {
                //何か表示していて、ファイル名が異なる場合、表示OK
                if (_sp.sprite.ToString().ToLower() != _FileName.ToLower())
                {
                    sw = true;
                }
            }
        }

        if (sw)
        {
            _sp.sprite = AssetBundleManager.Instance.GetSpriteFromAssetBundle(_FileName, _AssetBundle);
        }
    }

    /// <summary>
    /// 外部から表示ON/Offを指定
    /// </summary>
    /// <param name="sw"></param>
    public void SetDispOnOff(bool sw)
    {
        DispOnOff = sw;
    }
    /// <summary>
    /// 外部から表示するスプライトを指定
    /// </summary>
    /// <param name="_ab">アセットバンドル名</param>
    /// <param name="_f">ファイル名</param>
    public void SetSprite(string _ab, string _f)
    {
        _AssetBundle = _ab;
        _FileName = _f;
    }


}
