// 参考URL [UnityでC++を使う方法]
// https://qiita.com/8128/items/7cd0bf0b3f5bad60f709
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

static class Lib {
    [DllImport ("DllComport", CallingConvention = CallingConvention.StdCall)]
    public static extern void getComportList (StringBuilder ret, int retLen);
}

public class dll_comport : MonoBehaviour {
    // Start is called before the first frame update
    void Start () {
        int STRING_MAX_LENGTH = 512;
        StringBuilder str = new StringBuilder (STRING_MAX_LENGTH);

        Lib.getComportList (str, STRING_MAX_LENGTH);
        Debug.Log (str.ToString ());
    }
}