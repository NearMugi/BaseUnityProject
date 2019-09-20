using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各オブジェクト同士の衝突イベントを管理する
/// </summary>
public class HitManage : MonoBehaviour {

    #region Singleton

    private static HitManage instance;

    public static HitManage Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (HitManage)FindObjectOfType(typeof(HitManage));

                if (instance == null)
                {
                    Debug.LogError(typeof(HitManage) + "is nothing");
                }
            }

            return instance;
        }
    }

    #endregion Singleton

    static private Dictionary<string, HitRef> dictHitRefs;
    static HitManage()
    {
        dictHitRefs = new Dictionary<string, HitRef>();
    }
    // Class with the AssetBundle reference, url and version
    private class HitRef
    {
        /// <summary>
        /// ぶつかってきたオブジェクトのCollider
        /// </summary>
        public Collider HitCollider;

        /// <summary>
        /// <summary>
        /// ぶつかってきたオブジェクトの名前
        /// </summary>
        public string HitColliderName;
        /// <summary>

        /// <summary>
        /// ぶつかられたオブジェクト
        /// </summary>
        public GameObject HitObject;

        //コンストラクター
        public HitRef(GameObject ob)
        {
            HitObject = ob;
        }
    };
    	
	// Update is called once per frame
	void FixedUpdate () {
        //リストを初期化する
        //※このスクリプトはどのスクリプトよりも先に実行される
        //　FixedUpdateにしないとダメ。
        dictHitRefs.Clear();
    }

    void _add(string key, HitRef _hit)
    {
        if (!dictHitRefs.ContainsKey(key))
        {
            dictHitRefs.Add(key, _hit);
           // Debug.LogWarning("[Add dictHitRef] key:" + key + " count:" + dictHitRefs.Count);
        }
    }
    /// <summary>
    /// 追加
    /// </summary>
    /// <param name="ob">ぶつかられたオブジェクト</param>
    /// <param name="_col">ぶつかってきたオブジェクトのCollider</param>
    public void dictAdd(GameObject ob, Collider _col)
    {
        string _key = _col.name + ob.name;  //キーはぶつかったオブジェクト名 + ぶつかられたオブジェクト名
        HitRef _h = new HitRef(ob);
        _h.HitCollider = _col;
        _h.HitColliderName = _col.name;
        _add(_key, _h);

    }


    /// <summary>
    /// 追加
    /// </summary>
    /// <param name="ob">ぶつかられたオブジェクト</param>
    /// <param name="_col">ぶつかってきたオブジェクトのCollision</param>
    public void dictAdd(GameObject ob, Collision _col)
    {
        string _key = _col.collider.name + ob.name;  //キーはぶつかったオブジェクト名 + ぶつかられたオブジェクト名
        HitRef _h = new HitRef(ob);
        _h.HitCollider = _col.collider;
        _h.HitColliderName = _col.collider.name;
        _add(_key, _h);
    }

    /// <summary>
    /// 追加　※RayCastに対応したもの
    /// </summary>
    /// <param name="ob">ぶつかられたオブジェクト</param>
    /// <param name="baseOb">ぶつかってきたオブジェクトの名前</param>
    public void dictAdd(GameObject ob, string baseOb)
    {
        string _key = baseOb + ob.name;  //キーはぶつかったオブジェクト名 + ぶつかられたオブジェクト名
        HitRef _h = new HitRef(ob);
        _h.HitCollider = null;
        _h.HitColliderName = baseOb;
        _add(_key, _h);
    }




    /// <summary>
    /// ぶつかってきたオブジェクトのColliderを取得する
    /// </summary>
    /// <param name="_key">ぶつかったオブジェクト名 + ぶつかられたオブジェクト名</param>
    /// <returns></returns>
    public Collider dictFind(string _key)
    {
        Collider _col = null;
        HitRef _h;
        if(dictHitRefs.TryGetValue(_key, out _h))
        {
            _col = _h.HitCollider;
        }

        return _col;
    }

    /// <summary>
    /// 現在何かにヒットしているオブジェクトのリスト(2次元)
    /// </summary>
    /// <param name="_data">[ぶつかったオブジェクト名,ぶつかられたオブジェクト名]</param>
    public void dictFindHitObject(out string[,] _data)
    {
        string[,] s = new string[dictHitRefs.Count,2];
        int i = 0;
        foreach (KeyValuePair<string, HitRef> pair in dictHitRefs)
        {
            s[i, 0] = pair.Value.HitColliderName;
            s[i, 1] = pair.Value.HitObject.name;
            //Debug.LogWarning(s[i, 0] + " --> " + s[i, 1]);
            i++;
        }
        _data = s;

    }
}
