using System;

/// <summary>
/// Enum 型の拡張メソッドを管理するクラス
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// 現在のインスタンスで 1 つ以上のビット フィールドが設定されているかどうかを判断します
    /// </summary>
    public static bool HasFlag(this Enum self, Enum flag)
    {
        if (self.GetType() != flag.GetType())
        {
            throw new ArgumentException("flag の型が、現在のインスタンスの型と異なっています。");
        }

        var selfValue = Convert.ToUInt64(self);
        var flagValue = Convert.ToUInt64(flag);

        return (selfValue & flagValue) == flagValue;
    }
}
