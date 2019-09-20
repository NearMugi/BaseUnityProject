using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キー入力やArduinoからの信号などをトリガーにして発生するイベントをまとめたクラス
/// </summary>
public class EventTrigger : MonoBehaviour {

    #region Singleton

    private static EventTrigger instance;

    public static EventTrigger Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (EventTrigger)FindObjectOfType(typeof(EventTrigger));

                if (instance == null)
                {
                    Debug.LogError(typeof(EventTrigger) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton



    private void Start()
    {
        _reset();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void _reset()
    {
    }
}
