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
            SpearfishSpeed.Unknown       => "δ֪�ٶ�",
            SpearfishSpeed.SuperSlow     => "�����޵���",
            SpearfishSpeed.ExtremelySlow => "������",
            SpearfishSpeed.VerySlow      => "�ǳ���",
            SpearfishSpeed.Slow          => "�е���",
            SpearfishSpeed.Average       => "һ���",
            SpearfishSpeed.Fast          => "�е��",
            SpearfishSpeed.VeryFast      => "�ǳ���",
            SpearfishSpeed.ExtremelyFast => "������",
            SpearfishSpeed.SuperFast     => "�����޵п�",
            SpearfishSpeed.HyperFast     => "�����޵п�",
            SpearfishSpeed.LynFast       => "�쵽ģ��",
            SpearfishSpeed.None          => "û���ٶ�",
            _                            => $"{(ushort)speed}",
        };
}
