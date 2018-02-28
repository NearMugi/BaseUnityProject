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

    bool isPrepare;
    bool isStart;
    
    VideoPlayer player;
    Coroutine NowCoroutine;

    private void Start()
    {
        isPrepare = false;
        isStart = false;
        
        //同一オブジェクトにある前提
        player = GetComponent<VideoPlayer>();

        //動画再生まで後ろのほうに移動させておく。
        //デコード完了後、1frm目が表示されるので、意図的に見せたい場合は前のほうにする
        Vector3 pos = DispOb.GetComponent<Transform>().localPosition;
        pos.z = preparePos_z;
        DispOb.GetComponent<Transform>().localPosition = pos;

    }


    // Update is called once per frame
    void Update () {
        if (!isPrepare)
        {
            isPrepare = true;
            if (NowCoroutine != null) StopCoroutine(NowCoroutine);
            NowCoroutine = StartCoroutine(DecodeAndPlayCoroutine());
        }
    }

    public void SetisStart()
    {
        isStart = true;
    }

    /// <summary>
    /// 動画実行中　true
    /// </summary>
    /// <returns></returns>
    public bool isPlay()
    {
        return player.isPlaying;
    }

    /// <summary>
    /// 外部から準備を指示
    /// </summary>
    public void Reset()
    {
        isPrepare = false;
    }

    private IEnumerator DecodeAndPlayCoroutine()
    {
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

        //座標を戻す
        Vector3 pos = DispOb.GetComponent<Transform>().localPosition;
        pos.z = preparePos_z;
        DispOb.GetComponent<Transform>().localPosition = pos;

        //デコード
        player.Prepare();
        yield return null;

        //外部から再生指示があるまで待つ
        while (!isStart)
        {
            yield return null;
        }

        if (!player.isPrepared) Debug.LogWarning( player.name + " Not Prepared !");

        player.Play();
        //指定した座標へ移動する
        pos = DispOb.GetComponent<Transform>().localPosition;
        pos.z = activePos_z;
        DispOb.GetComponent<Transform>().localPosition = pos;

        //Debug.LogWarning(this.name + " pos:" + pos);

        //サウンドがある場合は有効にする
        if(snd != null)
        {
            snd.Play();
        }


        yield break;

    }

    public void StopMovie()
    {
        isStart = false;
        isPrepare = false;
        
        //座標を戻す
        Vector3 pos = DispOb.GetComponent<Transform>().localPosition;
        pos.z = preparePos_z;
        DispOb.GetComponent<Transform>().localPosition = pos;

        //動画、サウンドを止める
        if(player != null) player.Stop();
        AudioSource snd = GetComponent<AudioSource>();
        if (snd != null)
        {
            snd.Stop();
        }
        //Debug.LogWarning(gameObject.name + "  isPrepare " + isPrepare);

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
