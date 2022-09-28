using System;

namespace GatherBuddy.Config;

[Flags]
public enum JobFlags
{
    Logging      = 0x01,
    Harvesting   = 0x02,
    Mining       = 0x04,
    Quarrying    = 0x08,
    Fishing      = 0x10,
    Spearfishing = 0x20,
}

public static class JobFlagsExtension
{
    public static string ToName(this JobFlags type)
    {
        return type switch
        {
            JobFlags.Logging => "采伐",
            JobFlags.Harvesting => "割草",
            JobFlags.Mining => "采掘",
            JobFlags.Quarrying => "碎石",
            JobFlags.Fishing => "钓鱼",
            JobFlags.Spearfishing => "刺鱼",

            _ => "未知",
        };
    }
}