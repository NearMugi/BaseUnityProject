using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// マウス入力などの値を記録する
/// <para>記録したいデータごとにクラスを作成する</para>
/// </summary>
public class MotionList : MonoBehaviour {

    //dictIDList_addをCallすることで、外部から座標データを取得している。
    //連続データにIDを割り当てて、そのIDに座標データを紐づけている。
    //【イメージ】
    // [ID:1] --- t1の時の座標pos1
    //         +- t2の時の座標pos2
    //         +- t3の時の座標pos3
    //         ・
    //         ・
    //         ・
    //データが一定期間追加されない場合、そのIDは入力終了と判断する


    /// <summary>
    /// 保存するID最大数
    /// </summary>
    public const int MAX_ID_CNT = 10;
    /// <summary>
    /// 入力値を保存しておく最大時間(s)
    /// </summary>
    const float MAX_DATA_KEEP_TIME = 5.0f;
    /// <summary>
    /// 入力情報待ち時間(s)　情報の追加がなかった場合、そのIDは入力が終わったとみなす
    /// </summary>
    const float WATCH_WAIT_TIME = 2.5f;
    /// <summary>
    /// 入力値変化無しと判断するまでの時間(s)　変化しなかった場合、そのIDは入力が終わったとみなす
    /// </summary>
    const float WATCH_NOUPDATE_TIME = 0.5f;

    /// <summary>
    /// 経過時間(s)　IDがゼロ個になると初期化される
    /// </summary>
    float recordTime;


    //最終登録ID
    int lastAddID;
    //最終登録時の時間(s)
    float lastAddID_Time;

    /// <summary>
    /// IDを保存するライブラリー
    /// キーはID
    /// </summary>
    static private Dictionary<int, IDCls> dictIDList;
    public Dictionary<int, IDCls>  getdictIDList()
    {
        return dictIDList;
    }
    public class IDCls
    {
        public bool isEnd;     //入力情報の追加が一定時間ない場合、Trueにする

        int ID;
       
        public float lastTime;
        public FrmInfoCls lastInfo;    //最終追加時の入力情報


        //コンストラクター
        public IDCls(int _id, float _time)
        {
            ID = _id;
            FrmInfoData = new Dictionary<float, FrmInfoCls>();
            FrmInfoData.Clear();
        }

        /// <summary>
        /// 入力情報を保存するライブラリー
        /// キーは新規追加からの経過時間
        /// </summary>
        public Dictionary<float, FrmInfoCls> FrmInfoData;
        
        //入力情報を追加する
        public void addFrmInfoData(float _time, Vector3 _nowPos)
        {
            lastTime = _time;

            //入力情報ディクショナリーに一つも登録されていない場合、前回の座標＝今回の座標として登録する
            //一つでも登録されている場合、最終データを検索して前回の座標を取得する
            if(FrmInfoData.Count <= 0)
            {
                lastInfo = new FrmInfoCls(lastTime, _nowPos, _nowPos);
            } else
            {
                var sorted = FrmInfoData.OrderByDescending((x) => x.Key);    //経過時間で降順
                KeyValuePair<float, FrmInfoCls> pair = sorted.First();
                lastInfo = new FrmInfoCls(lastTime, pair.Value.getnowPos(), _nowPos);


            }

            //既にキーがある場合は更新しない(同じ経過時間に追加している。登録しなくても支障はないと思われる。)
            if (!FrmInfoData.ContainsKey(lastTime))
            {
                FrmInfoData.Add(lastTime, lastInfo);
            }
        }

        public int getID()
        {
            return ID;
        }

    };

    /// <summary>
    /// フレーム単位の入力情報
    /// </summary>
    public class FrmInfoCls
    {
        //追加した時間(ms)    update内の経過時間
        float addTime;
        //前回の座標
        Vector3 befPos;
        //今回の座標
        Vector3 nowPos;
        //変化量
        Vector3 delta;
        //2点間の距離
        float scalar;
        //角度(°)
        float degree;

        //コンストラクタ
        public FrmInfoCls(float _addTime, Vector3 _befPos, Vector3 _nowPos)
        {
            addTime = _addTime;

            setValue(_befPos, _nowPos);
        }

        public float getaddTime()
        {
            return addTime;
        }
        public Vector3 getbefPos()
        {
            return befPos;
        }
        
        public Vector3 getnowPos()
        {
            return nowPos;
        }
        public Vector3 getdelta()
        {
            return delta;
        }

        public float getscalar()
        {
            return scalar;
        }

        public float getdegree()
        {
            return degree;
        }

        void setValue(Vector3 _befPos, Vector3 _nowPos)
        {
            befPos = _befPos;
            nowPos = _nowPos;
            scalar = (_befPos - _nowPos).magnitude;
            delta = new Vector3(_nowPos.x - _befPos.x, _nowPos.y - _befPos.y, _nowPos.z - _befPos.z);
            degree = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;   //※z方向は考えていない。
        }

    }


    // Use this for initialization
    void Start () {
        dictIDList = new Dictionary<int, IDCls>();
        dictIDList.Clear();
        lastAddID = -1;
        lastAddID_Time = 0.0f;
    }
	

	void FixedUpdate () {
        //リストの更新処理
        dictIDList_Update();
    }

    /// <summary>
    /// リストの更新処理
    /// </summary>
    void dictIDList_Update()
    {
        int _dictIDListCnt = dictIDList.Count;

        //リストに一つもない場合、経過時間をリセットして処理を抜ける
        if (_dictIDListCnt <= 0)
        {
            //Debug.LogWarning("リストに一つもない");
            dictIDList.Clear();
            recordTime = 0.0f;
            return;
        }

        //経過時間を加算
        recordTime += Time.deltaTime;

        //Debug.LogWarning("リストにデータあり " + dictIDList.Count + "  経過時間 " + recordTime);

        int[] delID = new int[MAX_ID_CNT];
        int i = 0;

        //入力情報を更新する
        foreach (KeyValuePair<int, IDCls> targetIDCls in dictIDList)
        {
            //各ID内の古い入力情報を消していく　全て無くなったらそのIDを削除する
            if (dictIDList_Update_KeepData(recordTime, ref targetIDCls.Value.FrmInfoData))
            {
                delID[i++] = targetIDCls.Key;
            }
            else
            {
                //各IDの入力終了の有無を判断する
                dictIDList[targetIDCls.Key].isEnd = dictIDList_Update_isEnd(recordTime, targetIDCls.Value.lastTime);
                //一定期間変化しなかった場合、入力終了と判断する
                //dictIDList[targetIDCls.Key].isEnd = dictIDList_Update_isEnd(recordTime, targetIDCls.Value.FrmInfoData);
            }
        }

        //削除対象IDを処理する
        foreach (int _id in delID)
        {
            dictIDList.Remove(_id);
        }
    }

    /// <summary>
    /// 古い情報を消していく データがなくなったらそのIDは削除する
    /// </summary>
    /// <param name="_info">ID内の入力情報ライブラリー</param>
    /// <param name="nowTime">現在の経過時間(ms)</param>
    /// <returns>true…データ無し</returns>
    bool dictIDList_Update_KeepData(float nowTime, ref Dictionary<float, FrmInfoCls> _info)
    {
        //Debug.LogWarning(nowTime + "     " + _info.Count);
        
        float[] delInfo = new float[_info.Count];
        int i = 0;
        foreach(KeyValuePair<float, FrmInfoCls> _d in _info)
        {
            if(_d.Value.getaddTime() + MAX_DATA_KEEP_TIME <= nowTime)
            {

                delInfo[i++] = _d.Key;
            }
        }
        
        foreach(float _key in delInfo)
        {
            _info.Remove(_key);
        }

        if (_info.Count <= 0) return true;
        return false;
    }

    /// <summary>
    /// 各IDの入力終了の有無を判断する　一定期間入力がなかったらtrue
    /// </summary>
    /// <param name="nowTime"></param>
    /// <param name="lastTime"></param>
    /// <returns></returns>
    bool dictIDList_Update_isEnd(float nowTime, float lastTime)
    {
        if (lastTime + WATCH_WAIT_TIME <= nowTime) return true;
        return false;
    }

    /// <summary>
    /// 一定期間変化しなかった場合、入力終了と判断する
    /// </summary>
    /// <param name="_info"></param>
    /// <returns></returns>
    bool dictIDList_Update_isEnd(float nowTime, Dictionary<float, FrmInfoCls> _info)
    {
        bool sw = false;
        var sorted = _info.OrderByDescending((x) => x.Key);    //経過時間で降順

        //一定期間内に一つでもスカラー値がゼロ以上のものがあれば、まだ終了していない
        foreach (var _d in sorted)
        {
            //チェック対象の時間内
            if(recordTime - _d.Key <= WATCH_NOUPDATE_TIME)
            {
                if(_d.Value.getscalar() > 0.0f)
                {
                    sw = false;
                    break;
                }
            } else
            {
                //チェック対象の時間外　＝　チェックしたデータ全てゼロだった
                sw = true;
                break;
            }
        }
        return sw;
    }



    /// <summary>
    /// 外部から入力情報を追加(衝突イベント時など)
    /// </summary>
    /// <param name="nowPos"></param>
    public void dictIDList_add(Vector3 nowPos)
    {
        //新規なのか、更新なのか判断する
        int _id = dictIDList_isNewID();
        if(_id < 0)
        {
            lastAddID++;
            lastAddID_Time = recordTime;
            IDCls _IDCls = new IDCls(lastAddID, lastAddID_Time);
            dictIDList.Add(lastAddID, _IDCls);
            dictIDList[lastAddID].addFrmInfoData(recordTime, nowPos);
        }
        else
        {
            lastAddID_Time = recordTime;
            dictIDList[_id].addFrmInfoData(recordTime, nowPos);
        }
    }

    /// <summary>
    /// 新規なのか、更新なのか判断する
    /// </summary>
    /// <returns>新規の場合、-1,更新の場合、対象ID</returns>
    int dictIDList_isNewID()
    {
        int _id = -1;

        //登録IDがゼロ個の場合、追加
        if (dictIDList.Count <= 0) return _id;

        //入力終了したIDしかない場合、追加
        var sorted = dictIDList.OrderByDescending((x) => x.Key);    //IDで降順
        foreach (var _d in sorted)
        {
            //入力終了していないIDがあれば、そのIDにデータを追加する
            if (!_d.Value.isEnd)
            {
                _id = _d.Key;
                return _id;
            }
        }

        //入力終了したIDしかなく、かつIDの登録数を超えている場合は一番古いデータを削除する
        if(dictIDList.Count >= MAX_ID_CNT)
        {
            KeyValuePair<int, IDCls> pair = sorted.Last();
            dictIDList.Remove(pair.Key);
        }

        return _id;
    }

    public string getDebugInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Length = 0;

        sb.Append("[現在のID数]");
        sb.Append(dictIDList.Count);
        sb.Append("\n");

        sb.Append("[経過時間(s)]");
        sb.Append(recordTime);
        sb.Append("\n");
        sb.Append("--------------------");
        sb.Append("\n");
#if false
        sb.Append("[最終登録ID,時間]");
        sb.Append(lastAddID);
        sb.Append("  ");
        sb.Append(lastAddID_Time);
        sb.Append("\n");
        sb.Append("--------------------");
        sb.Append("\n");
#endif
        sb.Append("[IDデータ]");
        var sorted = dictIDList.OrderBy((x) => x.Key);    //IDで昇順
        foreach (var _d in sorted)
        {
            sb.Append("ID:");
            sb.Append(_d.Key);
            sb.Append("\n");
            sb.Append("lastTime:");
            sb.Append(_d.Value.lastTime);
            sb.Append("\n");
            sb.Append("isEnd:");
            sb.Append(_d.Value.isEnd);

            sb.Append("\n");
            sb.Append(_d.Value.lastInfo.getbefPos());
            sb.Append(" -> ");
            sb.Append(_d.Value.lastInfo.getnowPos());
            sb.Append("\n");
            //            sb.Append(_d.Value.lastInfo.getscalar());
            //            sb.Append(" , ");
            //            sb.Append(_d.Value.lastInfo.getdegree());
            //            sb.Append("\n");


            sb.Append("--------------------");
            sb.Append("\n");
        }

        return sb.ToString();
    }
}
