using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using GatherBuddy.Config;
using GatherBuddy.Interfaces;
using GatherBuddy.Time;
using ImGuiNET;
using OtterGui;
using OtterGui.Table;
using ImRaii = OtterGui.Raii.ImRaii;

namespace GatherBuddy.Gui;

public partial class Interface
{
    internal static bool DrawLocationInput(IGatherable item, ILocation? current, out ILocation? ret)
    {
        const string noPreferred = "无偏好地点";
        var          width       = SetInputWidth * 0.85f;
        ret = current;
        if (item.Locations.Count() == 1)
        {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f));
            ImGuiUtil.DrawTextButton(item.Locations.First().Name, new Vector2(width, 0), ImGui.GetColorU32(ImGuiCol.FrameBg));
            DrawLocationTooltip(item.Locations.First());
            return false;
        }

        var text = current?.Name ?? noPreferred;
        ImGui.SetNextItemWidth(width);
        using var combo = ImRaii.Combo("##地点", text);
        DrawLocationTooltip(current);
        if (!combo)
            return false;

        var changed = false;

        if (ImGui.Selectable(noPreferred, current == null))
        {
            ret     = null;
            changed = true;
        }

        var idx = 0;
        foreach (var loc in item.Locations)
        {
            using var id = ImRaii.PushId(idx++);
            if (ImGui.Selectable(loc.Name, loc.Id == (current?.Id ?? 0)))
            {
                ret     = loc;
                changed = true;
            }

            DrawLocationTooltip(loc);
        }

        return changed;
    }

    internal static void DrawTimeInterval(TimeInterval uptime, bool uptimeDependency = false, bool rightAligned = true)
    {
        var active = uptime.ToTimeString(GatherBuddy.Time.ServerTime, false, out var timeString);
        var colorId = (active, uptimeDependency) switch
        {
            (true, true)   => ColorId.DependentAvailableFish.Value(),
            (true, false)  => ColorId.AvailableItem.Value(),
            (false, true)  => ColorId.DependentUpcomingFish.Value(),
            (false, false) => ColorId.UpcomingItem.Value(),
        };
        using var color = ImRaii.PushColor(ImGuiCol.Text, colorId);
        if (rightAligned)
            ImGuiUtil.RightAlign(timeString);
        else
            ImGui.TextUnformatted(timeString);
        color.Pop();
        if ((uptimeDependency || !char.IsLetter(timeString[0])) && ImGui.IsItemHovered())
        {
            using var tt = ImRaii.Tooltip();

            if (uptimeDependency)
                ImGuiUtil.DrawTextButton("时间限制", Vector2.Zero, 0xFF202080);

            if (!char.IsLetter(timeString[0]))
                ImGui.Text($"{uptime.Start}\n{uptime.End}\n{uptime.DurationString()}");
        }
    }

    internal static void HoverTooltip(string text)
    {
        if (!text.StartsWith('\0'))
            ImGuiUtil.HoverTooltip(text);
    }

    public static void AlignTextToSize(string text, Vector2 size)
    {
        var cursor = ImGui.GetCursorPos();
        ImGui.SetCursorPos(cursor + new Vector2(ImGui.GetStyle().ItemSpacing.X / 2, (size.Y - ImGui.GetTextLineHeight()) / 2));
        ImGui.Text(text);
        ImGui.SameLine();
        ImGui.SetCursorPosY(cursor.Y);
        ImGui.NewLine();
    }


    private static void DrawFormatInput(string label, string tooltip, string oldValue, string defaultValue, Action<string> setValue)
    {
        var       tmp = oldValue;
        using var id  = ImRaii.PushId(label);

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 50 * Scale);
        if (ImGui.InputText(string.Empty, ref tmp, 256) && tmp != oldValue)
        {
            setValue(tmp);
            GatherBuddy.Config.Save();
        }

        ImGuiUtil.HoverTooltip(tooltip);

        if (ImGuiUtil.DrawDisabledButton("默认", Vector2.Zero, defaultValue, defaultValue == oldValue))
        {
            setValue(defaultValue);
            GatherBuddy.Config.Save();
        }

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);
    }

    private static void DrawStatusLine<T>(Table<T> table, string name)
    {
        if (!GatherBuddy.Config.ShowStatusLine)
            return;

        ImGui.SameLine();
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        ImGui.NewLine();
        ImGui.TextUnformatted($"{table.CurrentItems} / {table.TotalItems} {name} 可见");
        if (table.TotalColumns != table.VisibleColumns)
        {
            ImGui.SameLine();
            ImGui.Dummy(new Vector2(50 * ImGuiHelpers.GlobalScale, 0));
            ImGui.SameLine();
            ImGui.TextUnformatted($"{table.TotalColumns - table.VisibleColumns} 隐藏列");
        }
    }

    private static void DrawClippy()
    {
        const string popupName = "采集小贴士###小贴士";
        const string text      = "找不到需要的东西？";
        if (GatherBuddy.Config.HideClippy)
            return;

        var textSize   = ImGui.CalcTextSize(text).X;
        var buttonSize = new Vector2(Math.Max(200, textSize) * ImGuiHelpers.GlobalScale, ImGui.GetFrameHeight());
        var padding    = ImGuiHelpers.ScaledVector2(9, 9);

        ImGui.SetCursorPos(ImGui.GetWindowSize() - buttonSize - padding);
        using var child = ImRaii.Child("##clippyChild", buttonSize, false, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);
        if (!child)
            return;

        using var color = ImRaii.PushColor(ImGuiCol.Button, 0xFFA06020);

        if (ImGui.Button(text, buttonSize))
            ImGui.OpenPopup(popupName);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift)
        {
            GatherBuddy.Config.HideClippy = true;
            GatherBuddy.Config.Save();
        }

        ImGuiUtil.HoverTooltip("单击以获得使用该表格的一些帮助信息。\n"
          + "Ctrl + Shift + 右键单击来永久隐藏该按钮。");

        color.Pop();
        var windowSize = new Vector2(1024 * ImGuiHelpers.GlobalScale,
            ImGui.GetTextLineHeightWithSpacing() * 13 + 2 * ImGui.GetFrameHeightWithSpacing());
        ImGui.SetNextWindowSize(windowSize);
        ImGui.SetNextWindowPos((ImGui.GetIO().DisplaySize - windowSize) / 2);
        using var popup = ImRaii.Popup(popupName,
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Modal);
        if (!popup)
            return;

        ImGui.BulletText(
            "您可以使用文本过滤器例如 \"物品名称...\" 来筛选所有包含给定字符串的项。 大小写不敏感且不会跨会话存储。");
        ImGui.BulletText(
            "文本过滤器还支持正则表达式, 例如 \"(饿饿|涩涩)\"匹配所有包括饿饿或者包括涩涩这两个字符串的所有项。");
        ImGui.BulletText("按钮过滤器 \"下次窗口\", \"采集点类型\" or \"种类\" 允许您过滤出所点击的特定类型。");
        ImGui.BulletText("这些过滤器是跨会话存储的。对于有活动过滤器的列，过滤器按钮是红色的。");
        ImGui.NewLine();
        ImGui.BulletText(
            "您可以单击标题的空白处，对该列中的表进行升序或降序排序。这是用一个小三角形来表示的。");
        ImGui.BulletText(
            "您可以在标题栏处右键单击打开采集表的上下文菜单，在其中可以隐藏不感兴趣的列。");
        ImGui.BulletText(
            "您可以通过拖动列的小分隔标记来调整文本列的宽度。文本列的宽度是跨会话存储的。");
        ImGui.BulletText(
            "您可以通过在标题栏空格处左键单击并拖动来重新排序大多数列。数据列的顺序是跨会话存储的。");
        ImGui.NewLine();
        ImGui.BulletText(
            "您可以右键单击物品名称或其他一些列的内容 (比如鱼饵和钓场) 来打开特定目标物品的上下文菜单。");
        ImGui.BulletText("您还可以对选项卡本身进行重新排序，尽管这不是跨会话存储的。");

        ImGui.SetCursorPosY(windowSize.Y - ImGui.GetFrameHeight() - ImGui.GetStyle().WindowPadding.Y);
        if (ImGui.Button("哈哈我学会啦", -Vector2.UnitX))
            ImGui.CloseCurrentPopup();
    }
}
