using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyboardManage : MonoBehaviour {

    #region Singleton

    private static keyboardManage instance;

    public static keyboardManage Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (keyboardManage)FindObjectOfType(typeof(keyboardManage));

                if (instance == null)
                {
                    Debug.LogError(typeof(keyboardManage) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton

    /// <summary>
    /// セッティング画面の表示・非表示を切り替える
    /// </summary>
    [HideInInspector]
    public bool SETTING_DISP;

    /// <summary>
    /// センサーからの情報の表示・非表示を切り替える
    /// </summary>
    [HideInInspector]
    public bool SENSORINFO_DISP;

    private void Start()
    {
        SETTING_DISP = false;
        SENSORINFO_DISP = false;
    }

    // Update is called once per frame
    void Update () {

        if (Input.GetKeyDown(KeyCode.F1))
        {
            SETTING_DISP = !SETTING_DISP;
        }

        if (Input.GetKeyDown(KeyCode.F2)){
            SENSORINFO_DISP = !SENSORINFO_DISP;
        }

    }
}
