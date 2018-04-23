using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン名を管理するだけのクラス
/// </summary>
public class sceneManage_Name : MonoBehaviour {

    //※MAINシーンはプロジェクト管理用なので対象に入れない。
    public enum SCENE_NAME
    {
        NONE = 0,
        MENU,
        MAINEVENT,

        CONNECT_UNIPOLAR,
        CONNECT_AIR,
        CONNECT_DCMOTOR,
        CONNECT_MPU6050,

        TERMINAL,
    }
}

