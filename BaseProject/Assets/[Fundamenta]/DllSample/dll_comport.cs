// 参考URL [UnityでC++を使う方法]
// https://qiita.com/8128/items/7cd0bf0b3f5bad60f709
using UnityEngine;
using System.Runtime.InteropServices;

static class Lib
{
    [DllImport("DllComport", CallingConvention = CallingConvention.StdCall)]
    public static extern string getComportList();
}

public class dll_comport : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Lib.getComportList());
    }
}
