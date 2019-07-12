using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Interface : MonoBehaviour
{

    [SerializeField]
    Text txtActiveObjectList;

    [SerializeField]
    Text txtDispDebug_1;
    StringBuilder sb_1;

    [SerializeField]
    Text txtDispDebug_2;
    StringBuilder sb_2;

    [SerializeField]
    GameObject Button;

    // Use this for initialization
    void Start()
    {
        sb_1 = new StringBuilder();
        sb_2 = new StringBuilder();
        btn_isTarget();
    }

    // Update is called once per frame
    void Update()
    {
        createActiveObjectList();
        createDispDebugList_1();
        createDispDebugList_2();
    }

    public void btn_Exit()
    {
        Application.Quit();
    }

    public void btn_MoveToMenu()
    {
        sceneManage.Instance.chgScene(sceneManage_Name.SCENE_NAME.MENU);
    }
    public void btn_MoveToMainEvent()
    {
        sceneManage.Instance.chgScene(sceneManage_Name.SCENE_NAME.MAINEVENT);
    }
    public void btn_MoveToConnect_Unipolar()
    {
        sceneManage.Instance.chgScene(sceneManage_Name.SCENE_NAME.CONNECT_UNIPOLAR);
    }



    public void btn_Connect_UnipolarMotor()
    {
        if (SerialConnect_Arduino_Unipolar.Instance_Unipolar == null) return;
        SerialConnect_Arduino_Unipolar.Instance_Unipolar.Connect();
    }
    public void btn_Connect_DCMotor()
    {
        if (SerialConnect_Arduino_DCMotor.Instance_DCMotor == null) return;
        SerialConnect_Arduino_DCMotor.Instance_DCMotor.Connect();
        SerialConnect_Arduino_DCMotor.Instance_DCMotor.SerialInit();
    }
    public void btn_Connect_Sponge()
    {
        if (SerialConnect_Sponge.Instance == null) return;
        SerialConnect_Sponge.Instance.Connect();
    }
    public void btn_Connect_Air()
    {
        if (SerialConnect_Arduino_Air.Instance_Air == null) return;
        SerialConnect_Arduino_Air.Instance_Air.Connect();
    }
    public void btn_Connect_PotentioMeter()
    {
        if (SerialConnect_Arduino_PotentioMeter.Instance_PotentioMeter == null) return;
        SerialConnect_Arduino_PotentioMeter.Instance_PotentioMeter.Connect();
    }
    public void btn_Connect_MPU6050()
    {
        if (SerialConnect_Arduino_mpu6050.Instance_mpu6050 == null) return;
        SerialConnect_Arduino_mpu6050.Instance_mpu6050.Connect();
    }
    public void btn_Connect_BlueTooth()
    {
        if (SerialConnect_BlueTooth.Instance_BlueTooth == null) return;
        SerialConnect_BlueTooth.Instance_BlueTooth.Connect();
    }

    public void btn_Disconnect_BlueTooth()
    {
        if (SerialConnect_BlueTooth.Instance_BlueTooth == null) return;
        SerialConnect_BlueTooth.Instance_BlueTooth.Disconnect();
    }

    public void btn_Connect_Zigbee()
    {
        if (SerialConnect_Zigbee.Instance_Zigbee == null) return;
        SerialConnect_Zigbee.Instance_Zigbee.Connect();
    }

    public void btn_Connect_3DFilm()
    {
        if (SerialConnect_Arduino_3DFilm.Instance == null) return;
        SerialConnect_Arduino_3DFilm.Instance.Connect();
    }

    /// <summary>
    /// ボタン名の記載されていないボタンは無効にする
    /// </summary>
    void btn_isTarget()
    {
        GameObject _c = null;
        bool sw;
        foreach (Transform child in Button.transform)
        {
            sw = false;
            _c = child.Find("Text").gameObject;
            if (_c != null)
            {
                //Debug.LogWarning(child.name + "  " + _c.name);
                if (_c.GetComponent<Text>().text.Length != 0)
                {
                    sw = true;
                }
            }
            if (!sw) child.GetComponent<Button>().interactable = false;
        }
    }

    void createDispDebugList_1()
    {
        if (txtDispDebug_1 == null) return;

        sb_1.Length = 0;
        sb_1.Append(SetupProject.Instance.DebugList());
        sb_1.Append("\n");
        sb_1.Append(SerialHandler.Instance.DebugList());
        sb_1.Append("\n");
        txtDispDebug_1.text = sb_1.ToString();
    }

    void createDispDebugList_2()
    {
        if (txtDispDebug_2 == null) return;
        sb_2.Length = 0;
        if (SerialConnect_Sponge.Instance != null) sb_2.Append(SerialConnect_Sponge.Instance.DebugList());

        if (SerialConnect_Arduino_Unipolar.Instance_Unipolar != null) sb_2.Append(SerialConnect_Arduino_Unipolar.Instance_Unipolar.DebugList());

        if (SerialConnect_Arduino_DCMotor.Instance_DCMotor != null) sb_2.Append(SerialConnect_Arduino_DCMotor.Instance_DCMotor.DebugList());

        if (SerialConnect_Arduino_Air.Instance_Air != null) sb_2.Append(SerialConnect_Arduino_Air.Instance_Air.DebugList());

        if (SerialConnect_Arduino_PotentioMeter.Instance_PotentioMeter != null) sb_2.Append(SerialConnect_Arduino_PotentioMeter.Instance_PotentioMeter.DebugList());

        if (SerialConnect_Arduino_mpu6050.Instance_mpu6050 != null) sb_2.Append(SerialConnect_Arduino_mpu6050.Instance_mpu6050.DebugList());

        if (SerialConnect_BlueTooth.Instance_BlueTooth != null) sb_2.Append(SerialConnect_BlueTooth.Instance_BlueTooth.DebugList());

        if (SerialConnect_Zigbee.Instance_Zigbee != null) sb_2.Append(SerialConnect_Zigbee.Instance_Zigbee.DebugList());

        if (SerialConnect_Arduino_3DFilm.Instance != null) sb_2.Append(SerialConnect_Arduino_3DFilm.Instance.DebugList());

        txtDispDebug_2.text = sb_2.ToString();

    }


    /// <summary>
    /// アクティブなオブジェクトの一覧(孫まで)とVideoPlayerを持つオブジェクトの数をリストにする
    /// </summary>
    void createActiveObjectList()
    {
        if (txtActiveObjectList == null) return;

        StringBuilder sb = new StringBuilder();
        VideoPlayer _video;
        int sceneCnt = SceneManager.sceneCount;
        Scene sc;
        int videocnt = 0;
        for (int i = 0; i < sceneCnt; i++)
        {
            sc = SceneManager.GetSceneAt(i);
            sb.Append("[");
            sb.Append(sc.name);
            sb.Append("]");
            sb.Append("\n");

            //親
            foreach (GameObject ob in sc.GetRootGameObjects())
            {
                if (!ob.activeSelf) continue;

                sb.Append(ob.name);
                sb.Append("\n");
                _video = ob.GetComponent<VideoPlayer>();
                if (_video != null)
                {
                    if (_video.isPrepared || _video.isPlaying) videocnt++;
                }
                //子
                foreach (Transform child in ob.transform)
                {
                    if (!child.gameObject.activeSelf) continue;

                    _video = child.gameObject.GetComponent<VideoPlayer>();
                    if (_video != null)
                    {
                        if (_video.isPrepared || _video.isPlaying) videocnt++;
                    }
                    sb.Append(" + ");
                    sb.Append(child.name);
                    sb.Append("\n");

                    //孫
                    foreach (Transform childchild in child.transform)
                    {
                        if (!childchild.gameObject.activeSelf) continue;

                        _video = childchild.gameObject.GetComponent<VideoPlayer>();
                        if (_video != null)
                        {
                            if (_video.isPrepared || _video.isPlaying) videocnt++;
                        }
                        sb.Append("    + ");
                        sb.Append(childchild.name);
                        sb.Append("\n");
                    }

                }
            }
        }

        txtActiveObjectList.text = "MOVIE :" + videocnt + "\n" + sb.ToString();
    }


}
