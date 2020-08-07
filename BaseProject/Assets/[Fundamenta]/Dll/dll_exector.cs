// 参考URL [UnityでC++を使う方法]
// https://qiita.com/8128/items/7cd0bf0b3f5bad60f709
using UnityEngine;
using System.Runtime.InteropServices;

static class DLL
{
    [DllImport("add", CallingConvention = CallingConvention.StdCall)]
    public static extern int add_function(int a, int b);
}

public class dll_exector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(DLL.add_function(1, 1));
    }
}
