using System.Runtime.InteropServices;
using System.Text;
static class Lib
{
    [DllImport("DllComport", CallingConvention = CallingConvention.StdCall)]
    public static extern int getComportList(StringBuilder ret, int retSize);
}