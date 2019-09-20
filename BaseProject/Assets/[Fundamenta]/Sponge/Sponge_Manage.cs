using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sponge_Manage : MonoBehaviour {

    /// <summary>
    /// true…変数の初期化終了
    /// </summary>
    public bool _isEndSetting { get; private set; }

    /// <summary>
    /// true…解析中
    /// </summary>
    public bool _isAnalysis { get; set; }



    /// <summary>
    /// センサー数(個)　※SerialConnect_Sponge.csの値を使う
    /// </summary>
    public int SensorCnt { get; private set; }

    /// <summary>
    /// 格子のピッチ(mm)(固定)
    /// </summary>
    public const int pitch = 3;

    /// <summary>
    /// x方向の格子数
    /// </summary>
    public const int Matrix_x = 5;

    /// <summary>
    /// y方向の格子数
    /// </summary>
    public const int Matrix_y = 5;

    /// <summary>
    /// 左端をゼロとしたときの格子それぞれの距離(㎜)
    /// ※スプライン補間する際に使う
    /// </summary>
    float[] Matrix_dist = new float[] { 0, pitch, pitch * 2, pitch * 3, pitch * 4 };



    /// <summary>
    /// 平面のサイズ(mm)(水平)
    /// </summary>
    public const int PlaneSize_h = (Matrix_x - 1) * pitch;

    /// <summary>
    /// 平面のサイズ(mm)(垂直)
    /// </summary>
    public const int PlaneSize_v = (Matrix_y - 1) * pitch;

    /// <summary>
    /// Ｚ方向の最小値(mm)
    /// </summary>
    public const float PlaneSize_z_min = 0.0f;
    /// <summary>
    /// Ｚ方向の最大値(mm)
    /// </summary>
    public const float PlaneSize_z_max = 6.0f;


    /// <summary>
    /// シリアル通信で取得したデータを解析用テーブル向けに変換したもの
    /// キーはスポンジセンサーのID　※ID=01はキー=0にしている
    /// </summary>
    static private Dictionary<string, SpongeInfo> dictSpongeInfo;
    static Sponge_Manage()
    {
        dictSpongeInfo = new Dictionary<string, SpongeInfo>();
    }

    //スポンジ情報
    public const int DATA_CNT = 25;
    //P1～P9に対応する格子の添え字
    static readonly System.Collections.ObjectModel.ReadOnlyCollection<int> BasePos = Array.AsReadOnly(new int[] { 0, 2, 4, 10, 12, 14, 20, 22, 24 });
    //スポンジを5×5に分割している
    //20 21 22 23 24
    //15 16 17 18 19
    //10 11 12 13 14
    // 5  6  7  8  9
    // 0  1  2  3  4

    public class SpongeInfo
    {
        public int no;         //解析用テーブル向けのスポンジナンバー
        public float[] ch = new float[DATA_CNT];           //チャンネルの値(=A/D変換値)
        public float[] voltValue = new float[DATA_CNT]; //電圧(v)
        public float[] distValue = new float[DATA_CNT]; //距離(㎜)
        public float distValue_ave; //距離の平均値(㎜)　※スポンジ全体の変化量として扱う

        public float[,] MatrixTable = new float[Matrix_x, Matrix_y];       //解析用テーブル　MatrixTable[x,y] = z; 
        public Vector3[,] MatrixTable_Plot = new Vector3[Matrix_x, Matrix_y];  //画面上にプロットするデータ　※解析用テーブルにオフセットして移動距離を調整したもの 


    };

    /// <summary>
    /// UGUIで指定したナンバーとスポンジセンサーのIDを紐づける
    /// ナンバー＝センサーID
    /// </summary>
    /// <param name="key"></param>
    /// <param name="No"></param>
    public void SetNo(string key, int No)
    {
        SpongeInfo _info = new SpongeInfo();
        _info.no = No;
        dictSpongeInfo[key] = _info;
    }

    void GetVoltAndDist(float ad, out float v, out float d)
    {
        v = ad / 1023 * 3.3f;    //電圧に変換
        d = 9.302f * v;       //距離に変換(定数をかけている ※6㎜下がるときA/D変換値が200になるところから算出した)
    }

    /// <summary>
    /// スポンジセンサーの値をセットする
    /// </summary>
    /// <param name="key"></param>
    /// <param name="input"></param>
    public void SetSpongeInfo(string key, int[] input)
    {
        SpongeInfo _info = new SpongeInfo();

        //既に該当キーが存在する前提
        if (dictSpongeInfo.TryGetValue(key, out _info))
        {
            //プロット用データをセット(チャンネル4つを9つに増やす)
            //
            //以下のように分解
            // P7 P8 P9
            // P4 P5 P6
            // P1 P2 P3
            //
            //※インプットデータは以下の通り紐づいている。
            //  input[0] -> P2
            //  input[1] -> P4
            //  input[2] -> P8
            //  input[3] -> P6

            _info.ch[BasePos[0]] = (input[0] + input[1]) / 2;
            _info.ch[BasePos[1]] = input[0];
            _info.ch[BasePos[2]] = (input[0] + input[3]) / 2;
            _info.ch[BasePos[3]] = input[1];
            _info.ch[BasePos[4]] = (input[0] + input[1] + input[2] + input[3]) / 4;
            _info.ch[BasePos[5]] = input[3];
            _info.ch[BasePos[6]] = (input[1] + input[2]) / 2;
            _info.ch[BasePos[7]] = input[2];
            _info.ch[BasePos[8]] = (input[2] + input[3]) / 2;


            //ad値から電圧・距離を求める
            foreach (int i in BasePos)
            {
                GetVoltAndDist(_info.ch[i], out _info.voltValue[i], out _info.distValue[i]);
            }
            
            //Debug.LogWarning("KEY:" + key + "  _info._p[4] " + _info._p[4]);
            dictSpongeInfo[key] = _info;
        }
    }

    /// <summary>
    /// スポンジセンサーのチャンネルの値を消去
    /// </summary>
    /// <param name="key"></param>
    public void SetSpongeInfo_ChClear(string key)
    {
        SpongeInfo _info = new SpongeInfo();
        SpongeInfo _info_tmp = new SpongeInfo();

        //既に該当キーが存在する前提
        if (dictSpongeInfo.TryGetValue(key, out _info_tmp))
        {
            _info.no = _info_tmp.no;
            dictSpongeInfo[key] = _info;
        }
    }

    /// <summary>
    /// センサーのIDに一致するデータを取得する
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public SpongeInfo GetSpongeInfo(string key)
    {
        SpongeInfo _info = new SpongeInfo();

        if (dictSpongeInfo.TryGetValue(key, out _info))
        {
            return _info;
        } else
        {
            return null;
        }
    }

    //変数の初期化
    public void SetDefine()
    {
        _isEndSetting = false;

        SensorCnt = GetComponent<SerialConnect_Sponge>().GetDef_SensorCnt();

        //センサーの個数分テーブルを用意する
        for (int i=0; i < SensorCnt; i++)
        {
            SetNo(i.ToString(), i);
        }

        _isEndSetting = true;
    }

    /// <summary>
    /// 取得したデータを解析する
    /// </summary>
    public bool Analysis()
    {
        _isAnalysis = true;
        //Debug.LogWarning("[Sponge_Manage][Analysis]");

        for(int i = 0; i < SensorCnt; i++)
        {
            Analysis_Step1(i.ToString());
            Analysis_Step2(i.ToString());
            DataArrangement(i.ToString());  //変換したデータを使って値を整理する
        }

        _isAnalysis = false;
        return true;
    }

    /// <summary>
    ///スプライン補間　1回目
    ///水平方向に見ていく
    /// </summary>
    void Analysis_Step1(string _id)
    {

        // P7  * P8  * P9←
        //  *  *  *  *  *
        // P4  * P5  * P6←
        //  *  *  *  *  *
        // P1  * P2  * P3←

        //20 21 22 23 24←
        //15 16 17 18 19
        //10 11 12 13 14←
        // 5  6  7  8  9
        // 0  1  2  3  4←

        SpongeInfo _info = GetSpongeInfo(_id);

        float[] Input_x = new float[] { Matrix_dist[0], Matrix_dist[2], Matrix_dist[4] }; //スプライン補間を行うデータ(x)　※インプットデータに対応する場所は決まっているので固定
        float[] Input_y = new float[3]; //スプライン補間を行うデータ(y)
        float[] Output_y = new float[Matrix_y];   //スプライン補間より取得したデータ


        //x座標をスプライン補間クラスに渡す
        ns_Spline._Def_Spline _spline = new ns_Spline._Def_Spline(3, Matrix_dist);


        //行にあるスポンジの個数分繰り返す。
        
        for(int p = 0; p < 9; p+=3)
        {
            Input_y = new float[] { _info.ch[BasePos[p]], _info.ch[BasePos[p + 1]], _info.ch[BasePos[p + 2]] };
            //スプライン補間
            _spline.Func_MainSpline(Input_x, Input_y, ref Output_y);
            //SpongeInfoに反映
            int i = (int)(p/3) * 10;
            foreach (float f in Output_y)
            {
                _info.ch[i++] = f;
            }
        
        }

        dictSpongeInfo[_id] = _info;
    }

    /// <summary>
    /// スプライン補間　2回目
    /// 垂直方向に見ていく
    /// </summary>
    void Analysis_Step2(string _id)
    {
        //1回目の補間で全ての列に値が入っている。
        //(スポンジのある行の部分)
        //その値を基に全ての列でスプライン補間を行う。

        // P7  @ P8  @ P9
        //  *  *  *  *  *
        // P4  @ P5  @ P6
        //  *  *  *  *  *
        // P1  @ P2  @ P3
        // ↑ ↑ ↑ ↑ ↑ ※@の部分は1回目の補間で取得済み

        //20 21 22 23 24
        //15 16 17 18 19
        //10 11 12 13 14
        // 5  6  7  8  9
        // 0  1  2  3  4
        // ↑ ↑ ↑ ↑ ↑



        SpongeInfo _info = GetSpongeInfo(_id);

        float[] Input_x = new float[] { Matrix_dist[0], Matrix_dist[2], Matrix_dist[4] }; //スプライン補間を行うデータ(x)　※インプットデータに対応する場所は決まっているので固定
        float[] Input_y = new float[3]; //スプライン補間を行うデータ(y)
        float[] Output_y = new float[Matrix_y];   //スプライン補間より取得したデータ


        //x座標をスプライン補間クラスに渡す
        ns_Spline._Def_Spline _spline = new ns_Spline._Def_Spline(3, Matrix_dist);


        //行にあるスポンジの個数分繰り返す。
        for (int p = 0; p < Matrix_x; p++)
        {
            Input_y = new float[] { _info.ch[p], _info.ch[p + 10], _info.ch[p + 20] };
            //スプライン補間
            _spline.Func_MainSpline(Input_x, Input_y, ref Output_y);
            //SpongeInfoに反映
            int i = 0;
            foreach (float f in Output_y)
            {
                _info.ch[p + 5 * (i++)] = f;
            }

        }

        dictSpongeInfo[_id] = _info;
        
    }

    public const float PLOT_OFS = 0.01f;
    /// <summary>
    /// 変換したデータを使って値を整理する
    /// </summary>
    void DataArrangement(string _id)
    {
        SpongeInfo _info = GetSpongeInfo(_id);

        //ad値から電圧・距離・距離の平均値を求める
        int h = 0;
        _info.distValue_ave = 0;
        foreach (float ch in _info.ch)
        {
            GetVoltAndDist(ch, out _info.voltValue[h], out _info.distValue[h]);
            _info.distValue_ave += _info.distValue[h];
            h++;
        }
        if(h > 0) _info.distValue_ave /= h;
        dictSpongeInfo[_id] = _info;


        //画面表示用のテーブルを更新する
        int k = 0;
        float dist = 0;
        for (int i= 0; i< Matrix_y; i++)
        {
            for(int j = 0; j < Matrix_x; j++)
            {
                dist = _info.distValue[k++];
                _info.MatrixTable[j, i] = dist;
                _info.MatrixTable_Plot[j, i].z = dist * PLOT_OFS;//画面上にプロットするデータはこっちを使う。
            }
        }
    }


}
