using System;
using System.Linq;

namespace GatherBuddy.Enums;

public enum Patch : ushort
{
    Unknown                    = 0,
    ARealmReborn               = 200,
    ARealmAwoken               = 210,
    ThroughTheMaelstrom        = 220,
    DefendersOfEorzea          = 230,
    DreamsOfIce                = 240,
    BeforeTheFall              = 250,
    Heavensward                = 300,
    AsGoesLightSoGoesDarkness  = 310,
    TheGearsOfChange           = 320,
    RevengeOfTheHorde          = 330,
    SoulSurrender              = 340,
    TheFarEdgeOfFate           = 350,
    Stormblood                 = 400,
    TheLegendReturns           = 410,
    RiseOfANewSun              = 420,
    UnderTheMoonlight          = 430,
    PreludeInViolet            = 440,
    ARequiemForHeroes          = 450,
    Shadowbringers             = 500,
    VowsOfVirtueDeedsOfCruelty = 510,
    EchoesOfAFallenStar        = 520,
    ReflectionsInCrystal       = 530,
    FuturesRewritten           = 540,
    DeathUntoDawn              = 550,
    Endwalker                  = 600,
    NewfoundAdventure          = 610,
    BuriedMemory               = 620,
}

[Flags]
public enum PatchFlag : ulong
{
    ARealmReborn               = 1 << 0,
    ARealmAwoken               = 1 << 1,
    ThroughTheMaelstrom        = 1 << 2,
    DefendersOfEorzea          = 1 << 3,
    DreamsOfIce                = 1 << 4,
    BeforeTheFall              = 1 << 5,
    Heavensward                = 1 << 6,
    AsGoesLightSoGoesDarkness  = 1 << 7,
    TheGearsOfChange           = 1 << 8,
    RevengeOfTheHorde          = 1 << 9,
    SoulSurrender              = 1 << 10,
    TheFarEdgeOfFate           = 1 << 11,
    Stormblood                 = 1 << 12,
    TheLegendReturns           = 1 << 13,
    RiseOfANewSun              = 1 << 14,
    UnderTheMoonlight          = 1 << 15,
    PreludeInViolet            = 1 << 16,
    ARequiemForHeroes          = 1 << 17,
    Shadowbringers             = 1 << 18,
    VowsOfVirtueDeedsOfCruelty = 1 << 19,
    EchoesOfAFallenStar        = 1 << 20,
    ReflectionsInCrystal       = 1 << 21,
    FuturesRewritten           = 1 << 22,
    DeathUntoDawn              = 1 << 23,
    Endwalker                  = 1 << 24,
    NewfoundAdventure          = 1 << 25,
    BuriedMemory               = 1 << 26,
}

public static class PatchExtensions
{
    public static byte ToMajor(this Patch value)
        => (byte)((ushort)value / 100);

    public static byte ToMinor(this Patch value)
    {
        var val = (ushort)value % 100;
        if (val % 10 == 0)
            return (byte)(val / 10);

        return (byte)val;
    }

    public static Patch ToExpansion(this Patch value)
        => (Patch)(value.ToMajor() * 100);

    public static string ToVersionString(this Patch value)
        => value == Patch.Unknown ? "???" : $"{value.ToMajor()}.{value.ToMinor()}";

    public static string ToPatchName(this Patch value)
    {
        return ((ushort)value / 10) switch
        {
            20 => "����֮��",
            21 => "����֮��",
            22 => "���������",
            23 => "��ŷ���ǵ��ػ���",
            24 => "�����Ļ���",
            25 => "ϣ���ĵƻ�",
            30 => "���֮����",
            31 => "���밵�ķֽ�",
            32 => "���˵ĳ���",
            33 => "����ŭ��",
            34 => "���̳���",
            35 => "���˵�ֹ��",
            40 => "����֮��",
            41 => "Ӣ�۹���",
            42 => "���΢��",
            43 => "���·���",
            44 => "����ǰ��",
            45 => "Ӣ�����",
            50 => "��Ӱ֮����",
            51 => "���������",
            52 => "׷�������",
            53 => "ˮ���Ĳй�",
            54 => "��һ��δ��",
            55 => "����������",
            60 => "����֮��;",
            61 => "ո�µ�ð��",
            62 => "���ɵļ���",
            _  => "δ֪��ʫƪ",
        };
    }

    public static PatchFlag ToPatchFlag(this Patch value)
    {
        return value switch
        {
            Patch.ARealmReborn               => PatchFlag.ARealmReborn,
            Patch.ARealmAwoken               => PatchFlag.ARealmAwoken,
            Patch.ThroughTheMaelstrom        => PatchFlag.ThroughTheMaelstrom,
            Patch.DefendersOfEorzea          => PatchFlag.DefendersOfEorzea,
            Patch.DreamsOfIce                => PatchFlag.DreamsOfIce,
            Patch.BeforeTheFall              => PatchFlag.BeforeTheFall,
            Patch.Heavensward                => PatchFlag.Heavensward,
            Patch.AsGoesLightSoGoesDarkness  => PatchFlag.AsGoesLightSoGoesDarkness,
            Patch.TheGearsOfChange           => PatchFlag.TheGearsOfChange,
            Patch.RevengeOfTheHorde          => PatchFlag.RevengeOfTheHorde,
            Patch.SoulSurrender              => PatchFlag.SoulSurrender,
            Patch.TheFarEdgeOfFate           => PatchFlag.TheFarEdgeOfFate,
            Patch.Stormblood                 => PatchFlag.Stormblood,
            Patch.TheLegendReturns           => PatchFlag.TheLegendReturns,
            Patch.RiseOfANewSun              => PatchFlag.RiseOfANewSun,
            Patch.UnderTheMoonlight          => PatchFlag.UnderTheMoonlight,
            Patch.PreludeInViolet            => PatchFlag.PreludeInViolet,
            Patch.ARequiemForHeroes          => PatchFlag.ARequiemForHeroes,
            Patch.Shadowbringers             => PatchFlag.Shadowbringers,
            Patch.VowsOfVirtueDeedsOfCruelty => PatchFlag.VowsOfVirtueDeedsOfCruelty,
            Patch.EchoesOfAFallenStar        => PatchFlag.EchoesOfAFallenStar,
            Patch.ReflectionsInCrystal       => PatchFlag.ReflectionsInCrystal,
            Patch.FuturesRewritten           => PatchFlag.FuturesRewritten,
            Patch.DeathUntoDawn              => PatchFlag.DeathUntoDawn,
            Patch.Endwalker                  => PatchFlag.Endwalker,
            Patch.NewfoundAdventure          => PatchFlag.NewfoundAdventure,
            Patch.BuriedMemory               => PatchFlag.BuriedMemory,
            _                                => 0,
        };
    }

    public static Patch ToPatch(this PatchFlag value)
    {
        return value switch
        {
            PatchFlag.ARealmReborn               => Patch.ARealmReborn,
            PatchFlag.ARealmAwoken               => Patch.ARealmAwoken,
            PatchFlag.ThroughTheMaelstrom        => Patch.ThroughTheMaelstrom,
            PatchFlag.DefendersOfEorzea          => Patch.DefendersOfEorzea,
            PatchFlag.DreamsOfIce                => Patch.DreamsOfIce,
            PatchFlag.BeforeTheFall              => Patch.BeforeTheFall,
            PatchFlag.Heavensward                => Patch.Heavensward,
            PatchFlag.AsGoesLightSoGoesDarkness  => Patch.AsGoesLightSoGoesDarkness,
            PatchFlag.TheGearsOfChange           => Patch.TheGearsOfChange,
            PatchFlag.RevengeOfTheHorde          => Patch.RevengeOfTheHorde,
            PatchFlag.SoulSurrender              => Patch.SoulSurrender,
            PatchFlag.TheFarEdgeOfFate           => Patch.TheFarEdgeOfFate,
            PatchFlag.Stormblood                 => Patch.Stormblood,
            PatchFlag.TheLegendReturns           => Patch.TheLegendReturns,
            PatchFlag.RiseOfANewSun              => Patch.RiseOfANewSun,
            PatchFlag.UnderTheMoonlight          => Patch.UnderTheMoonlight,
            PatchFlag.PreludeInViolet            => Patch.PreludeInViolet,
            PatchFlag.ARequiemForHeroes          => Patch.ARequiemForHeroes,
            PatchFlag.Shadowbringers             => Patch.Shadowbringers,
            PatchFlag.VowsOfVirtueDeedsOfCruelty => Patch.VowsOfVirtueDeedsOfCruelty,
            PatchFlag.EchoesOfAFallenStar        => Patch.EchoesOfAFallenStar,
            PatchFlag.ReflectionsInCrystal       => Patch.ReflectionsInCrystal,
            PatchFlag.FuturesRewritten           => Patch.FuturesRewritten,
            PatchFlag.DeathUntoDawn              => Patch.DeathUntoDawn,
            PatchFlag.Endwalker                  => Patch.Endwalker,
            PatchFlag.NewfoundAdventure          => Patch.NewfoundAdventure,
            PatchFlag.BuriedMemory               => Patch.BuriedMemory,
            _                                    => Patch.Unknown,
        };
    }
}
