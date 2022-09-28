using System;

namespace GatherBuddy.Config;

[Flags]
public enum ItemFilter
{
    NoItems    = 0,
    Logging    = 0x000001,
    Harvesting = 0x000002,
    Mining     = 0x000004,
    Quarrying  = 0x000008,

    Regular   = 0x000010,
    Ephemeral = 0x000020,
    Unspoiled = 0x000040,
    Legendary = 0x000080,

    ARealmReborn   = 0x000100,
    Heavensward    = 0x000200,
    Stormblood     = 0x000400,
    Shadowbringers = 0x000800,
    Endwalker      = 0x001000,

    Available    = 0x010000,
    Unavailable  = 0x020000,

    All = 0x071FFF,
}
public static class ItemFilterExtension
{
    public static string ToName(this ItemFilter type)
    {
        return type switch
        {
            ItemFilter.Logging    => "采伐",
            ItemFilter.Harvesting => "割草",
            ItemFilter.Mining     => "采掘",
            ItemFilter.Quarrying  => "碎石",

            ItemFilter.Regular   => "常规",
            ItemFilter.Ephemeral => "限时",
            ItemFilter.Unspoiled => "全新",
            ItemFilter.Legendary => "传说",


            ItemFilter.ARealmReborn   => "重生之境",
            ItemFilter.Heavensward    => "苍穹之禁城",
            ItemFilter.Stormblood     => "红莲之狂潮",
            ItemFilter.Shadowbringers => "暗影之逆焰",
            ItemFilter.Endwalker      => "晓月之终途",

            ItemFilter.Available    => "可采集",
            ItemFilter.Unavailable  => "不可采",

            _ => "未知",
        };
    }
}