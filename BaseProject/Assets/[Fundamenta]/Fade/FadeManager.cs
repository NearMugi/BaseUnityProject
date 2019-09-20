using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// シーン遷移時のフェードイン・アウトを制御するためのクラス .
/// </summary>
public class FadeManager : MonoBehaviour
{
    #region Singleton

    private static FadeManager instance;

    public static FadeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (FadeManager)FindObjectOfType(typeof(FadeManager));

                if (instance == null)
                {
                    Debug.LogError(typeof(FadeManager) + "is nothing");
                }
            }

            return instance;
        }
    }

    #endregion Singleton

	/// <summary>フェード完了かどうか</summary>
	private bool isFadeEnd = false;

    [SerializeField]
    GameObject fadeOb;     //フェード用オブジェクト
    Material fadeMat;


    Coroutine NowCoroutine;

    /// <summary>
    /// フェードイン
    /// </summary>
    /// <param name='interval'>暗転にかかる時間(秒)</param>
    public void StartFadeIn (float interval, GameObject camera)
	{
        //Debug.LogWarning("StartFadeIn");
        NowCoroutine = StartCoroutine(FadeIn(OnFinishedCoroutine, interval, camera));
	}

    /// <summary>
    /// フェードアウト
    /// </summary>
    /// <param name='interval'>暗転にかかる時間(秒)</param>
    public void StartFadeOut(float interval, GameObject camera)
    {
        //Debug.LogWarning("StartFadeOut");
        NowCoroutine = StartCoroutine(FadeOut(OnFinishedCoroutine, interval, camera));
    }

    /// <summary>
    /// フェード用オブジェクトのマテリアルのα値を指定する
    /// </summary>
    /// <param name="_a"></param>
    /// <param name="camera"></param>
    public void FadeAlpha(float _a, GameObject camera)
    {
        //Debug.LogWarning("FadeAlpha");
        NowCoroutine = StartCoroutine(SetAlpha(OnFinishedCoroutine, _a, camera));
    }


    // コルーチンからのコールバック
    public void OnFinishedCoroutine(bool flg)
    {
        isFadeEnd = flg;
        if(flg)
        {
            //Debug.LogWarning("OnFinishedCoroutine");
            if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        }
        //Debug.LogWarning("OnFinishedCoroutine isFadeEnd=" + isFadeEnd);
    }

    public bool GetisFadeEnd()
    {
        return isFadeEnd;
    }

    /// <summary>
    /// フェードアウト用コルーチン
    /// </summary>
    /// <param name='interval'>暗転にかかる時間(秒)</param>
    private IEnumerator FadeOut (UnityAction<bool> callback, float interval, GameObject camera)
	{
        callback(false);


        this.fadeOb.SetActive(false);

        //フェード用オブジェクトの位置をカメラの位置に移動させる
        Transform t = this.fadeOb.GetComponent<Transform>();
        t.position = camera.GetComponent<Transform>().position;
        yield return null;


        this.fadeMat = this.fadeOb.GetComponent<Renderer>().material;
        
        float time = 0;
        Color col = this.fadeMat.color;
        col.a = 0.0f;
        this.fadeMat.color = col;
        this.fadeOb.SetActive(true);
        yield return null;

        while (time <= interval) {
            //だんだん暗く
            col.a = Mathf.Lerp(0f, 1f, time / interval);
            this.fadeMat.color = col;

            time += Time.deltaTime;
			yield return null;
		}

        col.a = 1.0f;
        this.fadeMat.color = col;
        this.fadeOb.SetActive(true);
        callback(true);

        yield break;
    }


    /// <summary>
    /// フェードイン用コルーチン
    /// </summary>
    /// <param name='interval'>暗転にかかる時間(秒)</param>
    private IEnumerator FadeIn(UnityAction<bool> callback, float interval, GameObject camera)
    {
        callback(false);

        //フェード用オブジェクトの位置をカメラの位置に移動させる
        Transform t = this.fadeOb.GetComponent<Transform>();
        t.position = camera.GetComponent<Transform>().position;

        this.fadeMat = this.fadeOb.GetComponent<Renderer>().material;
        Color col = this.fadeMat.color;
        col.a = 1.0f;
        this.fadeMat.color = col;

        this.fadeOb.SetActive(true);
        yield return null;

        //少し黒をキープ　全体の1/4
        float time = 0;
        float limit = interval / 4;
        while (time <= limit)
        {
            time += Time.deltaTime;
            yield return null;
        }


        //だんだん明るく　全体の3/4
        time = 0;
        limit = limit * 3;
        while (time <= limit)
        {
            col.a = Mathf.Lerp(1f, 0f, time / interval);
            this.fadeMat.color = col;
            time += Time.deltaTime;
            yield return null;
        }

        col.a = 0.0f;
        this.fadeMat.color = col;
        this.fadeOb.SetActive(false);

        callback(true);

        yield break;
    }

    /// <summary>
    /// α値を指定するコルーチン
    /// </summary>
    /// <param name='interval'>α値</param>
    private IEnumerator SetAlpha(UnityAction<bool> callback, float _a, GameObject camera)
    {
        callback(false);

        if(camera == null)
        {
            callback(true);
            yield break;
        }

        Transform t = this.fadeOb.GetComponent<Transform>();
        t.position = camera.GetComponent<Transform>().position;
        this.fadeMat = this.fadeOb.GetComponent<Renderer>().material;

        Color col = this.fadeMat.color;
        col.a = _a;
        this.fadeMat.color = col;
        yield return null;

        callback(true);

        yield break;

    }
}

