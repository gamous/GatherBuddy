using System;

namespace GatherBuddy.Enums;

public enum HookSet : byte
{
    Unknown    = 0,
    Precise    = 1,
    Powerful   = 2,
    Hook       = 3,
    DoubleHook = 4,
    TripleHook = 5,
    None       = 255,
}

public static class HookSetExtensions
{
    public static string ToName(this HookSet value)
        => value switch
        {
            HookSet.Unknown    => "未知",
            HookSet.Precise    => "精准提钩",
            HookSet.Powerful   => "强力提钩",
            HookSet.Hook       => "普通提钩",
            HookSet.DoubleHook => "双重提钩",
            HookSet.TripleHook => "三重提钩",
            HookSet.None       => "暂无数据",
            _                  => "错误数据",
        };
}
