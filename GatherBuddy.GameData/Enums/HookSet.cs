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
            HookSet.Unknown    => "δ֪",
            HookSet.Precise    => "��׼�ṳ",
            HookSet.Powerful   => "ǿ���ṳ",
            HookSet.Hook       => "��ͨ�ṳ",
            HookSet.DoubleHook => "˫���ṳ",
            HookSet.TripleHook => "�����ṳ",
            HookSet.None       => "��������",
            _                  => "��������",
        };
}
