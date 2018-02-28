using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Interface : MonoBehaviour {

    [SerializeField]
    Text txtActiveObjectList;

    [SerializeField]
    Text txtDispDebug;
    StringBuilder sb;

    [SerializeField]
    GameObject Button;

    // Use this for initialization
    void Start () {
        sb = new StringBuilder();
        btn_isTarget();
    }
	
	// Update is called once per frame
	void Update () {
        createActiveObjectList();
        createDispDebugList();
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
            if(_c != null)
            {
                Debug.LogWarning(child.name + "  " + _c.name);
                if (_c.GetComponent<Text>().text.Length != 0 )
                {
                    sw = true;
                }
            }
            if (!sw) child.GetComponent<Button>().interactable = false;
        }
    }

    void createDispDebugList()
    {
        if (txtDispDebug == null) return;

        sb.Length = 0;
        sb.Append(SerialHandler.Instance.DebugList());
        sb.Append("\n");
        txtDispDebug.text = sb.ToString();
    }


    /// <summary>
    /// アクティブなオブジェクトの一覧(孫まで)とVideoPlayerを持つオブジェクトの数をリストにする
    /// </summary>
    void createActiveObjectList()
    {
        if (txtActiveObjectList == null) return;

        StringBuilder sb = new StringBuilder();

        int sceneCnt = SceneManager.sceneCount;
        Scene sc;
        int videocnt = 0;
        for (int i = 0; i<sceneCnt; i++)
        {
            sc = SceneManager.GetSceneAt(i);
            sb.Append("[");
            sb.Append(sc.name);
            sb.Append("]");
            sb.Append("\n");
            
            //親
            foreach(GameObject ob in sc.GetRootGameObjects())
            {
                if (!ob.activeSelf) continue;

                sb.Append(ob.name);
                sb.Append("\n");
                if (ob.GetComponent<VideoPlayer>() != null) videocnt++;
                
                //子
                foreach (Transform child in ob.transform)
                {
                    if (!child.gameObject.activeSelf) continue;

                    if (child.gameObject.GetComponent<VideoPlayer>() != null) videocnt++;
                    sb.Append(" + ");
                    sb.Append(child.name);
                    sb.Append("\n");

                    //孫
                    foreach(Transform childchild in child.transform)
                    {
                        if (!childchild.gameObject.activeSelf) continue;

                        if (childchild.gameObject.GetComponent<VideoPlayer>() != null) videocnt++;
                        sb.Append("  + ");
                        sb.Append(childchild.name);
                        sb.Append("\n");
                    }

                }
            }
        }

        txtActiveObjectList.text = "MOVIE :" + videocnt + "\n" + sb.ToString();
    }
    

}
