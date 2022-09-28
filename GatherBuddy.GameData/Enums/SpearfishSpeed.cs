using System;

namespace GatherBuddy.Enums;

public enum SpearfishSpeed : ushort
{
    Unknown       = 0,
    SuperSlow     = 100,
    ExtremelySlow = 150,
    VerySlow      = 200,
    Slow          = 250,
    Average       = 300,
    Fast          = 350,
    VeryFast      = 400,
    ExtremelyFast = 450,
    SuperFast     = 500,
    HyperFast     = 550,
    LynFast       = 600,

    None = ushort.MaxValue,
}

public static class SpearFishSpeedExtensions
{
    public static string ToName(this SpearfishSpeed speed)
        => speed switch
        {
            SpearfishSpeed.Unknown       => "未知速度",
            SpearfishSpeed.SuperSlow     => "超级无敌慢",
            SpearfishSpeed.ExtremelySlow => "超级慢",
            SpearfishSpeed.VerySlow      => "非常慢",
            SpearfishSpeed.Slow          => "有点慢",
            SpearfishSpeed.Average       => "一般般",
            SpearfishSpeed.Fast          => "有点快",
            SpearfishSpeed.VeryFast      => "非常快",
            SpearfishSpeed.ExtremelyFast => "超级快",
            SpearfishSpeed.SuperFast     => "超级无敌快",
            SpearfishSpeed.HyperFast     => "究极无敌快",
            SpearfishSpeed.LynFast       => "快到模糊",
            SpearfishSpeed.None          => "没有速度",
            _                            => $"{(ushort)speed}",
        };
}
