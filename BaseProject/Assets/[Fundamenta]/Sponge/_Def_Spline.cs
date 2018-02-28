using System;
using UnityEngine;

namespace ns_Spline
{
    class _Def_Spline
    {

        /// <summary>
        /// スプライン補間に使うデータの個数
        /// </summary>
        int Cnt_input;

        /// <summary>
        /// スプライン補間に使うデータ(x)
        /// </summary>
        float[] Input_x;

        /// <summary>
        /// スプライン補間に使うデータ(y)
        /// </summary>
        float[] Input_y;


        /// <summary>
        /// 出力するデータの個数
        /// </summary>
        int Cnt_output;

        /// <summary>
        /// スプライン補間後のデータ(x) n×ピッチ
        /// </summary>
        float[] Output_x;

        /// <summary>
        /// スプライン補間後のデータ(y)
        /// </summary>
        float[] Output_y;

        /// <summary>
        /// 区分多項式の個数
        /// </summary>
        int Cnt_Spline;

        //計算用
        float[] a;
        float[] b;
        float[] c;
        float[] d;

        float[] h;
        float[] v;

        float[] u;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public _Def_Spline(int input_size, float[] o_x)
        {

            try
            {
                //インプットデータ
                Cnt_input = input_size;
                
                Cnt_output = o_x.Length;
                Output_x = new float[Cnt_output];
                Output_x = o_x;
                Output_y = new float[Cnt_output];


                //計算用
                Cnt_Spline = Cnt_input - 1;
                a = new float[Cnt_Spline];
                b = new float[Cnt_Spline];
                c = new float[Cnt_Spline];
                d = new float[Cnt_Spline];

                h = new float[Cnt_Spline];
                v = new float[Cnt_Spline];
                u = new float[Cnt_input];  //境界点の２次導関数はインプットデータの個数分必要

                //Debug.LogWarning("Cnt_output " + Cnt_output +  " Cnt_Spline " + Cnt_Spline);
            }
            catch
            {
                Debug.LogWarning("【エラー】スプライン補間のインスタンス作成ミス");
            }
            
        }
        /// <summary>
        /// スプライン補間のメイン処理
        /// </summary>
        /// <param name="_MeshPlot"></param>
        /// <returns></returns>
        public bool Func_MainSpline(float[] i_x, float[] i_y, ref float[] o_y)
        {
            bool SW = false;

            Input_x = i_x;
            Input_y = i_y;

            SW = Func_SetSpline();
            if (!SW)
            {
                Debug.LogWarning("【エラー】スプライン補間(Set)");
                return SW;
            }
            
            SW = Func_GetSpline();
            if (!SW)
            {
                Debug.LogWarning("【エラー】スプライン補間(Get)");
                return SW;
            }

            o_y = Output_y;

            return SW;
        }

        /// <summary>
        /// インプットデータを基にスプライン補間を行う。
        /// </summary>
        /// <returns></returns>
        bool Func_SetSpline()
        {
            bool SW = false;

            try
            {
                //入力データが4つ以上の場合はガウスジョルダン法でuの値を求める
                if(Cnt_input > 3)
                {
                    //h
                    for (int j = 0; j <= Cnt_Spline - 1; j++)
                    {
                        h[j] = Input_x[j + 1] - Input_x[j];
                        //                    Debug.LogWarning("j:" + j +" h[j]:"+h[j]);
                    }
                    //Debug.LogWarning("h ALL OK");

                    //v
                    for (int j = 1; j <= Cnt_Spline - 1; j++)
                    {
                        v[j] = 6 * ((Input_y[j + 1] - Input_y[j]) / h[j] - (Input_y[j] - Input_y[j - 1]) / h[j - 1]);
                        //                    Debug.LogWarning("j:{0} Input[]:{1} {2} {3}  v[j]:{4}", j,Input[j-1,1], Input[j, 1], Input[j + 1, 1], v[j]);
                    }
                    //Debug.LogWarning("v ALL OK");

                    //u
                    u[0] = 0;
                    u[Cnt_input - 1] = 0;


                    //連立１次方程式を解いてu[1]～u[Cnt_input - 1]を求める

                    //初期設定
                    int size = Cnt_input - 2;    //配列のサイズ
                    float[,] A = new float[size, size];
                    float[] V = new float[size];
                    float[] U = new float[size];

                    //ゼロ番目
                    A[0, 0] = 2 * (h[0] + h[1]);
                    A[0, 1] = h[1];
                    V[0] = v[1];

                    //１～MAX-2番目
                    for (int i = 1; i < size - 1; i++)
                    {
                        A[i, i - 1] = h[i];
                        A[i, i] = 2 * (h[i] + h[i + 1]);
                        A[i, i + 1] = h[i + 1];
                        V[i] = v[i + 1];
                    }
                    //最後(N-1)
                    A[size - 1, size - 2] = h[Cnt_Spline - 2];
                    A[size - 1, size - 1] = 2 * (h[Cnt_Spline - 2] + h[Cnt_Spline - 1]);
                    V[size - 1] = v[size];

                    SW = Func_GaussJordan(size, A, V, out U);
                    if (!SW)
                    {
                        Debug.LogWarning("【エラー】ガウスジョルダン法");
                        return SW;
                    }

                    for (int j = 1; j <= size; j++)
                    {
                        u[j] = U[j - 1];
                    }

                } else
                {
                    //入力データが3個の場合は行列にならないので、そのまま求める。
                    u[0] = 0;
                    u[1] = 6 * ((Input_y[2] - Input_y[1]) / (Input_x[2] - Input_x[1]) - (Input_y[1] - Input_y[0]) / (Input_x[1] - Input_x[0])) / (2 * (Input_x[2] - Input_x[0]));
                    u[Cnt_input - 1] = 0;
                }




                //a,b,c,d
                for (int j = 0; j <= Cnt_Spline - 1; j++)
                {
                    a[j] = (u[j + 1] - u[j]) / (6 * (Input_x[j + 1] - Input_x[j]));
                    b[j] = u[j] / 2;
                    c[j] = (Input_y[j + 1] - Input_y[j]) / (Input_x[j + 1] - Input_x[j]) - ((Input_x[j + 1] - Input_x[j]) * (2 * u[j] + u[j + 1]) / 6);
                    d[j] = Input_y[j];

                  //Debug.LogWarning("j:{0} a[j]:{1} b[j]:{2} c[j]:{3} d[j]:{4} u[j]:{5}", j, a[j], b[j], c[j], d[j], u[j]);
                  //  Debug.LogWarning("j:" +j +" a[j]:" +a[j]);
                }


                SW = true;
            }
            catch
            {
                SW = false;
            }
            
            return SW;
        }

        /// <summary>
        /// スプライン補間したデータをアウトプットデータに反映する
        /// </summary>
        /// <returns></returns>
        bool Func_GetSpline()
        {
            bool SW = false;

            float aj = 0.0f;
            float bj = 0.0f;
            float cj = 0.0f;
            float dj = 0.0f;
            float x = 0.0f;
            int input_cnt = 0;
            int output_cnt = 0;
            float point_x;


            try
            {
                //インプットデータの座標と比較して、スプライン補間にて計算した関数を基にアウトプットデータを作成する

                //初期値
                float range_min = Input_x[input_cnt];
                float range_max = Input_x[input_cnt + 1];
                aj = a[input_cnt];
                bj = b[input_cnt];
                cj = c[input_cnt];
                dj = d[input_cnt];

                while (output_cnt < Cnt_output)
                {
                    point_x = Output_x[output_cnt];  //データを取得したいx座標

                    //境界点(下限)の場合
                    //インプットデータのYを代入する
                    if (point_x == range_min)
                    {
                        Output_y[output_cnt] = Input_y[input_cnt];
                    }

                    //境界点(上限)の場合
                    //次の関数の範囲へ移行する　＆
                    //インプットデータのYを代入する
                    else if (point_x == range_max)
                    {
                        input_cnt++;
                        Output_y[output_cnt] = Input_y[input_cnt];

                        //次の準備
                        if (input_cnt < Cnt_Spline)
                        {
                            range_min = Input_x[input_cnt];
                            range_max = Input_x[input_cnt + 1];
                            aj = a[input_cnt];
                            bj = b[input_cnt];
                            cj = c[input_cnt];
                            dj = d[input_cnt];
                        }


                    }
                    //境界以外の場合は関数より値を求める
                    else if(range_min < point_x && point_x < range_max)
                    {
                        x = point_x - range_min;
                        Output_y[output_cnt] = aj * (float)Math.Pow(x, 3) + bj * (float)Math.Pow(x, 2) + cj * x + dj;
                        
                    }
                    
                    output_cnt++;
                }
                
                SW = true;
            }
            catch(Exception ex)
            {
                Debug.LogWarning("[スプライン補間したデータをアウトプットデータに反映] input_cnt:" + input_cnt +  " output_cnt:" + output_cnt + "    " + ex);
                SW = false;
            }
            

            return SW;
        }

        bool Func_GaussJordan(int size, float[,] A, float[] V, out float[] U)
        {
            bool SW = true;

            U = V;

            int ipv, i, j;
            float inv_pivot, temp;
            float big;
            int pivot_row = 0;
            int[] row = new int[A.GetLength(0)];


 //           Debug.LogWarning("+++ ガウス・ジョルダン法 +++");
 //           Debug.LogWarning("+++ 初期データ +++");
 //           Debug_DispMatrix(size, A, U);



            //1行ごとに処理を行っている
            for (ipv = 0; ipv <= size - 1; ipv++)
            {

                /* ---- 最大値探索 ---------------------------- */
                big = 0.0f;
                for (i = ipv; i <= size - 1; i++)
                {
                    if (Math.Abs(A[i, ipv]) > big)
                    {
                        big = Math.Abs(A[i, ipv]);
                        pivot_row = i;
                    }
                }

                if (big <= 0.0)
                {
                    SW = false;
                    return SW;
                }

                row[ipv] = pivot_row;

                /* ---- 行の入れ替え -------------------------- */
                //ピボット行(ipv,ipv)が最大値の場合は何もしない
                if (ipv != pivot_row)
                {
                    //Aの入れ替え
                    for (i = 0; i <= size - 1; i++)
                    {
                        temp = A[ipv, i];
                        A[ipv, i] = A[pivot_row, i];
                        A[pivot_row, i] = temp;
                    }
                    //Uの入れ替え
                    temp = U[ipv];
                    U[ipv] = U[pivot_row];
                    U[pivot_row] = temp;
                }

                /* ---- 対角成分=1(ピボット行の処理) ---------- */
                inv_pivot = 1.0f / A[ipv, ipv];
                //Aの更新
                A[ipv, ipv] = 1.0f;
                for (j = ipv+1; j <= size - 1; j++)
                {
                    A[ipv, j] *= inv_pivot;
                }
                //Uの更新
                U[ipv] *= inv_pivot;

                /* ---- ピボット列=0(ピボット行以外の処理) ---- */
                for (i = 0; i <= size - 1; i++)
                {
                    if (i != ipv)
                    {
                        temp = A[i, ipv];
                        //Aの更新
                        A[i, ipv] = 0.0f;
                        for (j = ipv+1; j <= size - 1; j++)
                        {
                            A[i, j] -= temp * A[ipv, j];
                        }
                        //Uの更新
                        U[i] -= temp * U[ipv];
                      //  if (ipv == 1) Debug.LogWarning("temp:{0} U[ipv]:{1}", temp,U[ipv]);

                    }
                }

                //1列処理した結果を表示
 //               Debug.LogWarning("+++ ipv:{0} +++", ipv);
 //               Debug_DispMatrix(size, A, U);

            }

            //↑1列ごとに行う処理はここまで

            /* ---- 列の入れ替え(逆行列) -------------------------- */
            for (j = size - 1; j >= 0; j--)
            {
                if (j != row[j])
                {
                    for (i = 0; i <= size - 1; i++)
                    {
                        temp = A[i, j];
                        A[i, j] = A[i, row[j]];
                        A[i, row[j]] = temp;
                    }
                }
            }

            return SW;
        }

        void Debug_DispMatrix(int size, double[,] A, double[] U)
        {
            string s;
            Debug.LogWarning("行列A");
            for (int i = 0; i <= size - 1; i++)
            {
                s = "";
                for (int j = 0; j <= size - 1; j++)
                {
                    s = s + " " + A[i, j];
                }
                Debug.LogWarning(s);
            }
            Debug.LogWarning("行列U");
            s = "";
            for (int i = 0; i <= size - 1; i++)
            {
                s = s + " " + U[i];
            }
            Debug.LogWarning(s);
            Debug.LogWarning("");
        }
    }
}
