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

    [SerializeField]
    Text txtDispDebug_2;

    [SerializeField]
    GameObject Button;

    // Use this for initialization
    void Start()
    {
        btn_isTarget();
    }

    // Update is called once per frame
    void Update()
    {
        keyEvent();
        createDispDebugList_1();
        createDispDebugList_2();
    }

    void keyEvent()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            return;
        }
    }

    public void btn_Click(Button btn)
    {
        string nm = btn.name;
        switch (nm)
        {
            case "Button11":
                //[example] scene change 
                //sceneManage.Instance.chgScene(sceneManage_Name.SCENE_NAME.MENU);

                //[example] serial connect
                //if (SerialConnect_Arduino_Unipolar.Instance_Unipolar == null) return;
                //SerialConnect_Arduino_Unipolar.Instance_Unipolar.Connect();
                break;
            case "Button12":
                break;
            case "Button13":
                break;
            case "Button21":
                break;
            case "Button22":
                break;
            case "Button23":
                break;
            case "Button31":
                break;
            case "Button32":
                SerialHandler.Instance.reGetComportList();
                break;
            case "Button33":
                Application.Quit();
                break;
        }

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
        StringBuilder sb = new StringBuilder();
        sb.Append(displaySetting.Instance.DebugList());
        sb.Append("\n");
        sb.Append(SerialHandler.Instance.DebugList());
        sb.Append("\n");
        txtDispDebug_1.text = sb.ToString();
    }

    void createDispDebugList_2()
    {
        if (txtDispDebug_2 == null) return;
        StringBuilder sb = new StringBuilder();

        if (SerialConnect_Sponge.Instance != null) sb.Append(SerialConnect_Sponge.Instance.DebugList());
        if (SerialConnect_Arduino_Unipolar.Instance_Unipolar != null) sb.Append(SerialConnect_Arduino_Unipolar.Instance_Unipolar.DebugList());
        if (SerialConnect_Arduino_DCMotor.Instance_DCMotor != null) sb.Append(SerialConnect_Arduino_DCMotor.Instance_DCMotor.DebugList());
        if (SerialConnect_Arduino_Air.Instance_Air != null) sb.Append(SerialConnect_Arduino_Air.Instance_Air.DebugList());
        if (SerialConnect_Arduino_PotentioMeter.Instance_PotentioMeter != null) sb.Append(SerialConnect_Arduino_PotentioMeter.Instance_PotentioMeter.DebugList());
        if (SerialConnect_Arduino_mpu6050.Instance_mpu6050 != null) sb.Append(SerialConnect_Arduino_mpu6050.Instance_mpu6050.DebugList());
        if (SerialConnect_BlueTooth.Instance_BlueTooth != null) sb.Append(SerialConnect_BlueTooth.Instance_BlueTooth.DebugList());
        if (SerialConnect_Zigbee.Instance_Zigbee != null) sb.Append(SerialConnect_Zigbee.Instance_Zigbee.DebugList());
        if (SerialConnect_Arduino_3DFilm.Instance != null) sb.Append(SerialConnect_Arduino_3DFilm.Instance.DebugList());
        if (SerialConnect_JetsonNano.Instance != null) sb.Append(SerialConnect_JetsonNano.Instance.DebugList());

        txtDispDebug_2.text = sb.ToString();

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