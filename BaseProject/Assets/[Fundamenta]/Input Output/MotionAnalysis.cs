using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// MotionList.csで記録したデータを解析する
/// <para>解析パターンを増やしていく。</para>
/// </summary>
public class MotionAnalysis : MonoBehaviour {
    MotionList list;
    Dictionary<int, MotionList.IDCls> dictIDList;

    ChangeValueManage _chgValue;
    [SerializeField]
    bool isAnalysisChangeValue;
    [SerializeField]
    bool isDispChangeValue;
    [HideInInspector]
    public string ChangeValueLog;

    // Use this for initialization
    void Start () {
        //同オブジェクト内にスクリプトがある前提
        list = gameObject.GetComponent<MotionList>();
        _chgValue = new ChangeValueManage();
        ChangeValueLog = string.Empty;

    }
	
	// Update is called once per frame
	void Update () {
        dictIDList = list.getdictIDList();

        if (isAnalysisChangeValue)
        {
            _chgValue.Analysis(dictIDList);
            if (isDispChangeValue)
            {
                ChangeValueLog = _chgValue.DispDebugLog();
            }

        }
    }   

    public Dictionary<int, ChangeValueManage.DataCls> getDataList()
    {
        return _chgValue.getDataList();
    }

    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    /// <summary>
    /// 先頭のデータから最新データまでの変化量を解析
    /// </summary>
    public class ChangeValueManage
    {
        Dictionary<int,DataCls> DataList;

        /// <summary>
        /// 解析する時間の範囲(s)
        /// </summary>
        const float RANGE_TIME = 1.0f;

        public Dictionary<int,DataCls> getDataList()
        {
            return DataList;
        }

        public class DataCls
        {
            //2点間を結ぶデータ
            public Vector3 firstPos;
            public Vector3 lastPos;
            public float scalar;
            public float degree;

            //先頭データから最新データまでにかかった時間
            public float rangeTime;
            
            //単位時間当たりの変化量
            public Vector3 deltaParTime;   
        }

        public ChangeValueManage()
        {
            DataList = new Dictionary<int, DataCls>();
            DataList.Clear();
        }

        /// <summary>
        /// 先頭のデータから最新データまでの変化量を解析
        /// </summary>
        public void Analysis(Dictionary<int, MotionList.IDCls> _dictIDList)
        {
            //保存データの初期化
            DataList.Clear();
            
            float t = 0.0f;
            //現在入力されているIDごとに解析する
            var IDListsorted = _dictIDList.OrderBy((x) => x.Key);    //IDで昇順
            foreach (var _id in IDListsorted)
            {
                DataCls tmp = new DataCls();
                
                var FrmInfosorted = _id.Value.FrmInfoData.OrderByDescending((x) => x.Key);    //経過時間で降順

                //最新データ
                KeyValuePair<float, MotionList.FrmInfoCls> pair = FrmInfosorted.First();
                tmp.lastPos = pair.Value.getnowPos();
                t = pair.Value.getaddTime();

                foreach (var _info in FrmInfosorted)
                {
                    //指定した範囲内のデータのみ抽出
                    //最後のデータを先頭データとみなす
                    if( t - _info.Value.getaddTime() <= RANGE_TIME)
                    {
                        tmp.firstPos = _info.Value.getnowPos();
                        tmp.rangeTime = t - _info.Value.getaddTime();
                    }
                    else
                    {
                        break;
                    }

                }

                tmp.scalar = (tmp.firstPos - tmp.lastPos).magnitude;
                Vector3 delta = new Vector3(tmp.lastPos.x - tmp.firstPos.x, tmp.lastPos.y - tmp.firstPos.y, tmp.lastPos.z - tmp.firstPos.z);
                tmp.degree = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;   //※z方向は考えていない。

                if (tmp.rangeTime != 0)
                {
                    //変化量を経過時間で割る
                    tmp.deltaParTime = delta / tmp.rangeTime;
                }
                else
                {
                    tmp.deltaParTime = new Vector3(0.0f, 0.0f, 0.0f);
                }
                
                DataList.Add(_id.Key, tmp);
            }
        }

        public string DispDebugLog()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Length = 0;

            var IDListsorted = DataList.OrderByDescending((x) => x.Key);    //IDで降順
            foreach (var _id in IDListsorted)
            {
                sb.Append("[ID]");
                sb.Append(_id.Key);

                sb.Append("\n");
                sb.Append(_id.Value.firstPos);
                sb.Append(" -> ");
                sb.Append(_id.Value.lastPos);
                sb.Append("\n");
                sb.Append(_id.Value.deltaParTime);
                sb.Append("\n");
                sb.Append(_id.Value.scalar);
                sb.Append("\n");
                sb.Append("--------------------");
                sb.Append("\n");
            }

            return sb.ToString();
        }
    }


}
