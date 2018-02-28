using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Sponge_PlotMesh : MonoBehaviour {
    
    [SerializeField]
    string MeshName;

    [SerializeField]
    string MeshShaderName;

    [SerializeField]
    bool isMeshObject;  //メッシュをオブジェクトに表示するか、UGUI上に表示するか

    [SerializeField]
    GameObject MeshPlotSpace;

    float[,] PlotMesh_z;

    int plot_x;
    int plot_y;

    int size_VerticesTable;
    Vector3[] VerticesTable;   //プロットする座標に変換した頂点

    Color[] ColorTable;       //頂点の色

    int size_TrianglesTable;
    int[] TrianglesTable;   //三角形
                            //こんなイメージ
                            // 6       7       8
                            //
                            //   [1,0]   [1,1]
                            //
                            // 3       4       5
                            //
                            //   [0,0]   [1,0]
                            //
                            // 0       1       2

    //TrianglesTable[0] = 0
    //TrianglesTable[1] = 4
    //TrianglesTable[2] = 1

    //TrianglesTable[3] = 0
    //TrianglesTable[4] = 3
    //TrianglesTable[5] = 4

    //TrianglesTable[6] = 1
    //TrianglesTable[7] = 5
    //TrianglesTable[8] = 2
    //・・・



    /// <summary>
    /// 変換したデータ分のprefabを生成する
    /// </summary>
    public void SetDefine(SerialConnect_Sponge _serial, int id)
    {
        //解析用テーブル分オブジェクトを生成する
        int cnt_x = Sponge_Manage.Matrix_x;
        int cnt_y = Sponge_Manage.Matrix_y;
        float pitch = Sponge_Manage.pitch / 20.0f;

        float ofs_x = (-1) * cnt_x * pitch / 2.0f;  //Unity上の原点に中央が来るようにオフセットを加算する
        float ofs_y = (-1) * cnt_y * pitch / 2.0f;

        //メッシュ用のテーブルを設定
//        Init((-1.0f * pitch * 0.5f) + ofs_x, (-1.0f * pitch * 0.5f) + ofs_y, pitch, cnt_x + 1, cnt_y + 1); //左下の頂点とピッチ、頂点数を渡している
        Init((-1.0f * pitch * 0.5f) + ofs_x, (-1.0f * pitch * 0.5f) + ofs_y, pitch, cnt_x, cnt_y); //左下の頂点とピッチ、頂点数を渡している

        //メッシュをオブジェクトに表示するか、UGUI上に表示するか
        if (isMeshObject)
        {
            CreatePlotMeshOb(_serial,id);
        }
        else
        {
            //UGUI上に表示する場合は、初期化タイミングがMesh出来た後なので何もしない。
        }

    }

    /// <summary>
    /// プロット用のオブジェクトを追加する関数
    /// ※UGUI上ではなく、オブジェクトのシェーダー
    /// </summary>
    /// <param name="_serial"></param>
    /// <param name="id"></param>
    void CreatePlotMeshOb(SerialConnect_Sponge _serial, int id)
    {
        GameObject _ob;
        Material _mat;

        _ob = AssetBundleManager.Instance.GetGameObjectFromAssetBundle(MeshName, SceneManager.GetActiveScene().name);
        _mat = AssetBundleManager.Instance.GetMaterialFromAssetBundle(MeshShaderName, SceneManager.GetActiveScene().name);

        GameObject _tmp_ob;
        _tmp_ob = Instantiate(_ob, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        _tmp_ob.name = MeshName + "_" + id.ToString("00");

        _tmp_ob.GetComponent<Renderer>().material = _mat;
        _tmp_ob.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/VertexColor");

        _tmp_ob.GetComponent<Sponge_PlotMeshUnit>().Init(_serial, id);  //プロット用オブジェクトに入っているスクリプトに値を渡す

        _tmp_ob.transform.parent = MeshPlotSpace.transform;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos_x"></param>
    /// <param name="pos_y"></param>
    /// <param name="cnt_x"></param>
    /// <param name="cnt_y"></param>
    public void Init(float pos_x,float pos_y ,float pitch, int cnt_x, int cnt_y)
    {
        plot_x = cnt_x;
        plot_y = cnt_y;

        PlotMesh_z = new float[plot_x, plot_y];

        //頂点
        size_VerticesTable = plot_x * plot_y;
        VerticesTable = new Vector3[size_VerticesTable];
        int k = 0;
        for (int i = 0; i < plot_x; i++)
        {
            for (int j = 0; j < plot_y; j++)
            {
                //頂点の指定　Unity上はx,y方向に平面を描き、z方向にセンサー値を与える
                VerticesTable[k++] = new Vector3(pos_x + pitch * j, pos_y + pitch * i, 0.0f);
            }
        }

        //頂点の色
        ColorTable = new Color[size_VerticesTable];

        //三角形を作る番号
        size_TrianglesTable = (plot_x - 1) * (plot_y - 1) * 3 * 2;
        TrianglesTable = new int[size_TrianglesTable];
        InitTrianglesTable();


    }

    void InitTrianglesTable()
    {
        //頂点数-1を対象に、三角形を作る番号を保存する
        // 6       7       8
        //
        //   [1,0]   [1,1]
        //
        // 3       4       5
        // ~       ~
        //   [0,0]   [1,0]
        //
        // 0       1       2
        // ~       ~
        // (0,4,1) (0,3,4)
        // (1,5,2) (1,4,5)
        // (3,7,4) (3,6,7)
        // (4,8,5) (4,7,8) 

        int p0; //左下の頂点
        int p1; //右下
        int p2; //左上
        int p3; //右上
        int k = 0;
        for (int i = 0; i < plot_x - 1; i++)
        {
            for (int j = 0; j < plot_y - 1; j++)
            {
                p0 = plot_x * i + j;
                p1 = p0 + 1;
                p2 = p0 + plot_x;
                p3 = p0 + 1 + plot_x;

                TrianglesTable[k++] = p0;
                TrianglesTable[k++] = p3;
                TrianglesTable[k++] = p1;

                TrianglesTable[k++] = p0;
                TrianglesTable[k++] = p2;
                TrianglesTable[k++] = p3;

                //Debug.LogWarning("(" + p0 + "," + p3 + "," + p1 + ") (" + p0 + "," + p2 + "," + p3 + ")");
            }
        }
    }

    public Mesh SetZ(Sponge_Manage _m, int _id)
    {
        Mesh plotmesh = new Mesh();

        Sponge_Manage.SpongeInfo _info = _m.GetSpongeInfo(_id.ToString());
        if (_info == null) return plotmesh; 

        //境界値(下)
        //1点or2点の平均

        //[0,0]
        PlotMesh_z[0, 0] = _info.MatrixTable_Plot[0, 0].z;
        //[1～plot_x-2,0]
        for (int i = 1; i < plot_x - 1; i++)
        {
            PlotMesh_z[i, 0] = (_info.MatrixTable_Plot[i - 1, 0].z + _info.MatrixTable_Plot[i, 0].z) / 2;
        }
        //[plot_x-1,0]
        PlotMesh_z[plot_x - 1, 0] = _info.MatrixTable_Plot[Sponge_Manage.Matrix_x - 1, 0].z;


        //境界値以外
        //2点or4点の平均
        for (int j = 1; j < plot_y - 1; j++)
        {
            //[0,j]
            PlotMesh_z[0, j] = (_info.MatrixTable_Plot[0, j - 1].z + _info.MatrixTable_Plot[0, j].z) / 2;

            //[1～plot_x-2,j]
            for (int i = 1; i < plot_x - 1; i++)
            {
                PlotMesh_z[i, j] = (_info.MatrixTable_Plot[i - 1, j - 1].z + _info.MatrixTable_Plot[i - 1, j].z + _info.MatrixTable_Plot[i, j - 1].z + _info.MatrixTable_Plot[i, j].z) / 4;
            }

            //[plot_x-1,j]
            PlotMesh_z[plot_x - 1, j] = (_info.MatrixTable_Plot[Sponge_Manage.Matrix_x - 1, j - 1].z + _info.MatrixTable_Plot[Sponge_Manage.Matrix_x - 1, j].z) / 2;
        }


        //境界値(上)
        //1点or2点の平均

        //[0,plot_y-1]
        PlotMesh_z[0, plot_y - 1] = _info.MatrixTable_Plot[0, Sponge_Manage.Matrix_y - 1].z;
        //[1～cnt_x-2,plot_y-1]
        for (int i = 1; i < plot_x - 1; i++)
        {
            PlotMesh_z[i, plot_y - 1] = (_info.MatrixTable_Plot[i - 1, Sponge_Manage.Matrix_y - 1].z + _info.MatrixTable_Plot[i, Sponge_Manage.Matrix_y - 1].z) / 2;
        }
        //[cnt_x-1,plot_y-1]
        PlotMesh_z[plot_x - 1, plot_y - 1] = _info.MatrixTable_Plot[Sponge_Manage.Matrix_x - 1, Sponge_Manage.Matrix_y - 1].z;


        //計算した値をプロット用変数へ反映する

        int k = 0;
        float color = 0.0f;
        for (int i = 0; i < plot_y; i++)
        {
            for (int j = 0; j < plot_x; j++)
            {
                //頂点の指定　Unity上はz方向にセンサー値を与える
                VerticesTable[k].z = PlotMesh_z[j,i];

                color = PlotMesh_z[j, i] / Sponge_Manage.PLOT_OFS / Sponge_Manage.PlaneSize_z_max;
                //端は多少丸める
                if (color > 0.95) color = 1.0f;
                if (color < 0.05) color = 0.0f;

                switch (_id)
                {
                    case 0:
                        ColorTable[k] = new Color(color, 0.0f, 0.0f);
                        break;
                    case 1:
                        ColorTable[k] = new Color(0.0f, color, 0.0f);
                        break;
                    case 2:
                        ColorTable[k] = new Color(0.0f, 0.0f, color);
                        break;
                    case 3:
                        ColorTable[k] = new Color(0.0f, color, 0.0f);
                        break;
                    case 4:
                        ColorTable[k] = new Color(0.0f, 0.0f, color);
                        break;

                    default:
                        break;
                }

                k++;
            }
        }


        //頂点の指定
        plotmesh.vertices = VerticesTable;


        //頂点インデックスの指定
        plotmesh.triangles = TrianglesTable;

        plotmesh.colors = ColorTable;

        return plotmesh;
    }
    



}
