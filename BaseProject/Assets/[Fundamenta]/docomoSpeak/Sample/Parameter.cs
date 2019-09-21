using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// さまざまなパラメータを管理するクラス。
/// </summary>
public class Parameter : MonoBehaviour
{
    // Unityちゃんが動いているか
    public static bool isMoving;
    // 表情が変わっているか
    public static bool isExpressChanging;
    // 話し中か
    public static bool isTalking;
    // Unityちゃんが現れているか
    public static bool isAppearing;

    // 音声認識APIのAPIキー
    public static readonly string recognizeAPIkey = "";
    // 雑談対話APIのAPIキー
    public static readonly string talkAPIkey = "";
    // 音声合成APIのAPIキー
    public static readonly string compositionAPIkey = "";

    private Text debugText;
}