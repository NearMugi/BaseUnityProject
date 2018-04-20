using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class MovieDecodeAndPlay : MonoBehaviour {

    [SerializeField]
    GameObject DispOb;   //動画を表示するオブジェクト
    [SerializeField]
    float preparePos_z;  //動画デコード時のｚ座標
    [SerializeField]
    float activePos_z;  //動画再生時のｚ座標

    [SerializeField]
    bool isFullScreen;
    [SerializeField]
    Camera targetCamera;
    
    VideoPlayer player;
    Coroutine NowCoroutine;
    bool isDecode;


    private void Start()
    {
        DispMovie(false);
        isDecode = false;

        //同一オブジェクトにある前提
        player = GetComponent<VideoPlayer>();

        //フルスクリーンの指定がある場合、スクリーンのスケールを変更する
        if (isFullScreen)
        {
            float Screen_width = Screen.width;
            float Screen_height = Screen.height;
            try
            {
                int displayNo = targetCamera.targetDisplay;
                Screen_width = Display.displays[displayNo].renderingWidth;
                Screen_height = Display.displays[displayNo].renderingHeight;
            }
            catch (System.Exception)
            {
            }
            Vector3 size = new Vector3(Screen_width / 1000f, 1.0f, Screen_height / 1000f);
            DispOb.GetComponent<Transform>().localScale = size;
        }

    }

    /// <summary>
    /// 動画実行中　true
    /// </summary>
    /// <returns></returns>
    public bool GetisPlay()
    {
        return player.isPlaying;
    }

    public void SettingMovie()
    {
        if (isDecode) return;   //すでにデコード中、もしくは完了している場合は何もしない。
        isDecode = true;
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(DecodeCoroutine());
    }

    private IEnumerator DecodeCoroutine()
    {
        Debug.LogWarning(player.name + " Decode Start");
        //動画が再生していたら止める
        while (player.isPlaying)
        {
            player.Stop();
            yield return null;
        }
        //サウンドを止める
        AudioSource snd = GetComponent<AudioSource>();
        if (snd != null)
        {
            snd.Stop();
        }

        //動画再生まで後ろのほうに移動させておく。
        //デコード完了後、1frm目が表示されるので、意図的に見せたい場合は前のほうにする
        DispMovie(false);

        //デコード
        player.Prepare();

        while (!player.isPrepared) yield return null;

        //Debug.LogWarning(player.name + "Decode End");

        yield break;

    }


    public void StartMovie()
    {
        if (player.isPlaying) return;   //すでに再生している場合は何もしない。

        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(PlayCoroutine());
    }

    private IEnumerator PlayCoroutine()
    {
        isDecode = false;
        if (!player.isPrepared) Debug.LogWarning(player.name + " Not Prepared !");
        
        player.Play();
        //指定した座標へ移動する
        DispMovie(true);

        //Debug.LogWarning(this.name + " pos:" + pos);

        //サウンドがある場合は有効にする
        AudioSource snd = GetComponent<AudioSource>();
        if (snd != null)
        {
            snd.Play();
        }

        yield break;
    }


    public void StopMovie()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(StopCoroutine());

    }

    private IEnumerator StopCoroutine()
    {
        isDecode = false;
        //座標を戻す
        DispMovie(false);
        yield return null;

        //動画、サウンドを止める
        if (player != null)
        {
            player.Stop();
            while (player.isPrepared) yield return null;
        }

        AudioSource snd = GetComponent<AudioSource>();
        if (snd != null)
        {
            snd.Stop();
        }
        yield break;
    }


    /// <summary>
    /// 座標を変えて表示・非表示にする
    /// </summary>
    /// <param name="sw"></param>
    public void DispMovie(bool sw)
    {
        Vector3 pos = DispOb.GetComponent<Transform>().localPosition;
        if (sw)
        {
            //指定した座標へ移動する
            pos.z = activePos_z;
        } else
        {
            //座標を戻す
            pos.z = preparePos_z;
        }
        DispOb.GetComponent<Transform>().localPosition = pos;
    }

}
