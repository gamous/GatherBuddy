using System;

namespace GatherBuddy.Enums;

public enum SpearfishSize : byte
{
    Unknown = 0,
    Small   = 1,
    Average = 2,
    Large   = 3,
    None    = 255,
}

public static class SpearFishSizeExtensions
{
    public static string ToName(this SpearfishSize size)
        => size switch
        {
            SpearfishSize.Unknown => "δ֪",
            SpearfishSize.Small   => "С��",
            SpearfishSize.Average => "����",
            SpearfishSize.Large   => "����",
            SpearfishSize.None    => "����",
            _                     => throw new ArgumentOutOfRangeException(nameof(size), size, null),
        };
}