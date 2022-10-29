﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Dalamud;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using GatherBuddy.Alarms;
using GatherBuddy.Classes;
using GatherBuddy.Config;
using GatherBuddy.Enums;
using GatherBuddy.Interfaces;
using GatherBuddy.Structs;
using GatherBuddy.Time;

namespace GatherBuddy.Plugin;

internal static class SeStringBuilderExtension
{
    public static SeStringBuilder AddColoredText(this SeStringBuilder builder, string text, int colorId)
        => builder.AddUiForeground((ushort)colorId)
            .AddText(text)
            .AddUiForegroundOff();

    public static SeStringBuilder AddFullMapLink(this SeStringBuilder builder, string name, Territory territory, float xCoord, float yCoord,
        bool openMapLink = false, bool withCoordinates = true, float fudgeFactor = 0.05f)
    {
        var mapPayload = new MapLinkPayload(territory.Id, territory.Data.Map.Row, xCoord, yCoord, fudgeFactor);
        if (openMapLink)
            Dalamud.GameGui.OpenMapWithMapLink(mapPayload);
        if (withCoordinates)
            name = $"{name} ({xCoord.ToString("00.0", CultureInfo.InvariantCulture)}, {yCoord.ToString("00.0", CultureInfo.InvariantCulture)})";
        return builder.AddUiForeground(0x0225)
            .AddUiGlow(0x0226)
            .Add(mapPayload)
            .AddUiForeground(500)
            .AddUiGlow(501)
            .AddText($"{(char)SeIconChar.LinkMarker}")
            .AddUiGlowOff()
            .AddUiForegroundOff()
            .AddText(name)
            .Add(RawPayload.LinkTerminator)
            .AddUiGlowOff()
            .AddUiForegroundOff();
    }

    public static SeStringBuilder AddFullItemLink(this SeStringBuilder builder, uint itemId, string itemName)
        => builder.AddUiForeground(0x0225)
            .AddUiGlow(0x0226)
            .AddItemLink(itemId, false)
            .AddUiForeground(0x01F4)
            .AddUiGlow(0x01F5)
            .AddText($"{(char)SeIconChar.LinkMarker}")
            .AddUiGlowOff()
            .AddUiForegroundOff()
            .AddText(itemName)
            .Add(RawPayload.LinkTerminator)
            .AddUiGlowOff()
            .AddUiForegroundOff();

    public static SeStringBuilder DelayString(this SeStringBuilder builder, TimeInterval uptime)
    {
        if (uptime.Start > GatherBuddy.Time.ServerTime)
            return builder.AddText("的可采集时间段为 ")
                .AddColoredText(TimeInterval.DurationString(uptime.Start, GatherBuddy.Time.ServerTime, false),
                    GatherBuddy.Config.SeColorArguments);

        return builder.AddText("的下一次可采集时间段为 ")
            .AddColoredText(TimeInterval.DurationString(uptime.End, GatherBuddy.Time.ServerTime, false), GatherBuddy.Config.SeColorArguments);
    }
}

public static class Communicator
{
    public delegate SeStringBuilder ReplacePlaceholder(SeStringBuilder builder, string placeholder);

    public static void Print(SeString message)
    {
        var entry = new XivChatEntry()
        {
            Message = message,
            Name    = SeString.Empty,
            Type    = GatherBuddy.Config.ChatTypeMessage,
        };
        Dalamud.Chat.PrintChat(entry);
    }

    public static void PrintError(SeString message)
    {
        var entry = new XivChatEntry()
        {
            Message = message,
            Name    = SeString.Empty,
            Type    = GatherBuddy.Config.ChatTypeError,
        };
        Dalamud.Chat.PrintChat(entry);
    }

    public static void Print(string message)
        => Print((SeString)message);

    public static void PrintError(string message)
        => PrintError((SeString)message);

    public static void Print(string left, string center, int color, string right)
    {
        SeStringBuilder builder = new();
        builder.AddText(left).AddColoredText(center, color).AddText(right);
        Print(builder.BuiltString);
    }

    public static void PrintError(string left, string center, int color, string right)
    {
        SeStringBuilder builder = new();
        builder.AddText(left).AddColoredText(center, color).AddText(right);
        PrintError(builder.BuiltString);
    }

    public static void PrintClipboardMessage(string objectType, string name, Exception? e = null)
    {
        if (e != null)
        {
            name = name.Length > 0 ? name : "<未命名>";
            PluginLog.Error($"Could not save {objectType}{name} to Clipboard:\n{e}");
            PrintError($"未能保存 {objectType}", name, GatherBuddy.Config.SeColorNames, " 到粘贴板。");
        }
        else if (GatherBuddy.Config.PrintClipboardMessages)
        {
            Print(objectType, name.Length > 0 ? name : "<未命名>", GatherBuddy.Config.SeColorNames, " 已保存到粘贴板。");
        }
    }

    public static void PrintUptime(TimeInterval uptime)
    {
        if (!GatherBuddy.Config.PrintUptime
         || uptime.Equals(TimeInterval.Always)
         || uptime.Equals(TimeInterval.Invalid)
         || uptime.Equals(TimeInterval.Never))
            return;

        if (uptime.Start > GatherBuddy.Time.ServerTime)
            Print("当前的可采集时间段为 ",      TimeInterval.DurationString(uptime.Start, GatherBuddy.Time.ServerTime, false),
                GatherBuddy.Config.SeColorArguments, "。");
        else
            Print("下一个可采集时间段为 ",      TimeInterval.DurationString(uptime.End, GatherBuddy.Time.ServerTime, false),
                GatherBuddy.Config.SeColorArguments, "。");
    }

    public static void PrintCoordinates(SeString link)
    {
        if (GatherBuddy.Config.WriteCoordinates)
            Print(link);
    }


    // Split a format string with '{text}' placeholders into a SeString with Payloads, 
    // and replace all placeholders by the returned payloads.
    private static SeString Format(string format, ReplacePlaceholder func)
    {
        SeStringBuilder builder     = new();
        var             lastPayload = 0;
        var             openBracket = -1;
        for (var i = 0; i < format.Length; ++i)
        {
            if (format[i] == '{')
            {
                openBracket = i;
            }
            else if (openBracket != -1 && format[i] == '}')
            {
                builder.AddText(format.Substring(lastPayload,   openBracket - lastPayload));
                var placeholder = format.Substring(openBracket, i - openBracket + 1);
                Debug.Assert(placeholder.StartsWith('{') && placeholder.EndsWith('}'));
                func(builder, placeholder);
                lastPayload = i + 1;
                openBracket = -1;
            }
        }

        if (lastPayload != format.Length)
            builder.AddText(format[lastPayload..]);
        return builder.BuiltString;
    }


    public static void PrintIdentifiedItem(string name, IGatherable? item)
    {
        if (item == null)
        {
            Print("未能找到名为 \"", name, GatherBuddy.Config.SeColorNames, "\" 的采集目标。");
            PluginLog.Verbose($"Could not find item corresponding to \"{name}\".");
            return;
        }

        if (GatherBuddy.Config.IdentifiedGatherableFormat.Length > 0)
            Print(FormatIdentifiedItemMessage(GatherBuddy.Config.IdentifiedGatherableFormat, name, item));
        PluginLog.Verbose(Configuration.DefaultIdentifiedGatherableFormat, item.ItemId, item.Name[ClientLanguage.ChineseSimplified], name);
    }

    public static void PrintAlarmMessage(Alarm alarm, ILocation location, TimeInterval uptime)
    {
        if (GatherBuddy.Config.AlarmFormat.Length > 0)
            Print(FormatAlarmMessage(GatherBuddy.Config.AlarmFormat, alarm, location, uptime));
        PluginLog.Verbose(Configuration.DefaultAlarmFormat, alarm.Name, alarm.Item.Name[ClientLanguage.ChineseSimplified], string.Empty,
            location.Name); // Duration string too ugly.
    }


    public static void LocationNotFound(IGatherable? item, GatheringType? type)
    {
        SeStringBuilder sb = new();
        sb.AddText("没有找到相关的位置或共鸣过的以太之光");
        if (item != null)
            sb.AddFullItemLink(item.ItemId, item.Name[GatherBuddy.Language]);
        else
            sb.AddColoredText("未知", GatherBuddy.Config.SeColorNames);

        if (type != null)
            sb.AddText(" 存在条件 ")
                .AddColoredText(type.Value.ToString(), GatherBuddy.Config.SeColorArguments);
        sb.AddText(".");
        Print(sb.BuiltString);
        PluginLog.Verbose(sb.BuiltString.TextValue);
    }

    public static void NoItemName(string command, string itemType)
    {
        PrintError(new SeStringBuilder().AddText($"请提供一个 {itemType} 名称 (部分也可), ")
            .AddColoredText("alarm", GatherBuddy.Config.SeColorArguments)
            .AddText(" 或 ")
            .AddColoredText("next", GatherBuddy.Config.SeColorArguments)
            .AddText(" 来 ")
            .AddColoredText(command, GatherBuddy.Config.SeColorCommands)
            .AddText("。").BuiltString);
    }

    public static void NoBaitFound(Bait bait)
    {
        PrintError(new SeStringBuilder().AddText("鱼饵 ")
            .AddFullItemLink(bait.Id, bait.Name)
            .AddText(" 未能装备到钓竿，因为你没有携带它。").BuiltString);
    }

    public static void NoGatherGroup(string groupName)
        => PrintError("采集组 ", groupName, GatherBuddy.Config.SeColorNames, " 不存在。");

    public static void NoGatherGroupItem(string groupName, int minute)
    {
        SeStringBuilder sb = new();
        sb.AddText("采集组 ")
            .AddColoredText(groupName, GatherBuddy.Config.SeColorNames)
            .AddText(" 在当前艾欧泽亚时间没有可采集目标。 ")
            .AddColoredText($"{minute / RealTime.MinutesPerHour:D2}:{minute % RealTime.MinutesPerHour:D2}",
                GatherBuddy.Config.SeColorArguments)
            .AddText(".");
        PrintError(sb.BuiltString);
    }

    private static SeString FormatIdentifiedItemMessage(string format, string input, IGatherable item)
    {
        SeStringBuilder Replace(SeStringBuilder builder, string s)
            => s.ToLowerInvariant() switch
            {
                "{item}"  => builder.AddFullItemLink(item.ItemId, item.Name[GatherBuddy.Language]),
                "{input}" => builder.AddColoredText(input, GatherBuddy.Config.SeColorArguments),
                _         => builder.AddText(s),
            };

        return Format(format, Replace);
    }


    private static SeString FormatAlarmMessage(string format, Alarm alarm, ILocation location, TimeInterval uptime)
    {
        SeStringBuilder NodeReplace(SeStringBuilder builder, string s)
            => s.ToLowerInvariant() switch
            {
                "{alarm}"       => builder.AddColoredText(alarm.Name.Any() ? $"[{alarm.Name}]" : "[Alarm]", GatherBuddy.Config.SeColorNames),
                "{item}"        => builder.AddFullItemLink(alarm.Item.ItemId, alarm.Item.Name[GatherBuddy.Language]),
                "{offset}"      => builder.AddText(alarm.SecondOffset.ToString()),
                "{delaystring}" => builder.DelayString(uptime),
                "{location}" => builder.AddFullMapLink(location.Name, location.Territory, location.IntegralXCoord / 100f,
                    location.IntegralYCoord / 100f),
                _ => builder.AddText(s),
            };

        var msg = Format(format, NodeReplace);
        msg.Payloads.Insert(0, new UIForegroundPayload((ushort)GatherBuddy.Config.SeColorAlarm));
        msg.Payloads.Add(UIForegroundPayload.UIForegroundOff);
        return msg;
    }
}
