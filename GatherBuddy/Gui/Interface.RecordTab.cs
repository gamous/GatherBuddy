using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using GatherBuddy.Config;
using GatherBuddy.Enums;
using GatherBuddy.FishTimer;
using GatherBuddy.Plugin;
using ImGuiNET;
using OtterGui;
using OtterGui.Table;
using ImGuiScene;
using Newtonsoft.Json;
using ImRaii = OtterGui.Raii.ImRaii;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private sealed class RecordTable : Table<FishRecord>
    {
        public const string FileNamePopup = "FileNamePopup";

        public RecordTable()
            : base("Fish Records", _plugin.FishRecorder.Records, _catchHeader, _baitHeader, _durationHeader, _castStartHeader,
                _biteTypeHeader, _hookHeader, _amountHeader, _spotHeader, _contentIdHeader, _gatheringHeader, _perceptionHeader, _sizeHeader,
                _flagHeader)
            => Flags |= ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable;

        private        int _lastCount;
        private static int _deleteIdx = -1;

        protected override void PreDraw()
        {
            ExtraHeight = ImGui.GetFrameHeightWithSpacing() / ImGuiHelpers.GlobalScale;
            if (_deleteIdx > -1)
            {
                _plugin.FishRecorder.Remove(_deleteIdx);
                _deleteIdx = -1;
            }

            if (_lastCount != Items.Count)
            {
                FilterDirty = true;
                _lastCount  = Items.Count;
            }
        }

        private static readonly ContentIdHeader  _contentIdHeader  = new() { Label = "Content ID" };
        private static readonly BaitHeader       _baitHeader       = new() { Label = "鱼饵" };
        private static readonly SpotHeader       _spotHeader       = new() { Label = "钓场" };
        private static readonly CatchHeader      _catchHeader      = new() { Label = "上钩的鱼" };
        private static readonly CastStartHeader  _castStartHeader  = new() { Label = "时间戳" };
        private static readonly BiteTypeHeader   _biteTypeHeader   = new() { Label = "咬钩力度" };
        private static readonly HookHeader       _hookHeader       = new() { Label = "提钩方式" };
        private static readonly DurationHeader   _durationHeader   = new() { Label = "咬钩时长" };
        private static readonly GatheringHeader  _gatheringHeader  = new() { Label = "获得力" };
        private static readonly PerceptionHeader _perceptionHeader = new() { Label = "鉴别力" };
        private static readonly AmountHeader     _amountHeader     = new() { Label = "数量" };
        private static readonly SizeHeader       _sizeHeader       = new() { Label = "尺寸" };
        private static readonly FlagHeader       _flagHeader       = new() { Label = "状态" };

        private sealed class GatheringHeader : ColumnString<FishRecord>
        {
            public override string ToName(FishRecord record)
                => record.Gathering.ToString();

            public override float Width
                => 50 * ImGuiHelpers.GlobalScale;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.Gathering.CompareTo(rhs.Gathering);

            public override void DrawColumn(FishRecord record, int _)
                => ImGuiUtil.RightAlign(ToName(record));
        }

        private sealed class PerceptionHeader : ColumnString<FishRecord>
        {
            public override string ToName(FishRecord record)
                => record.Perception.ToString();

            public override float Width
                => 50 * ImGuiHelpers.GlobalScale;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.Perception.CompareTo(rhs.Gathering);

            public override void DrawColumn(FishRecord record, int _)
                => ImGuiUtil.RightAlign(ToName(record));
        }

        private sealed class AmountHeader : ColumnString<FishRecord>
        {
            public override string ToName(FishRecord record)
                => record.Amount.ToString();

            public override float Width
                => 35 * ImGuiHelpers.GlobalScale;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.Amount.CompareTo(rhs.Amount);

            public override void DrawColumn(FishRecord record, int _)
            {
                ImGuiUtil.RightAlign(ToName(record));
            }
        }

        private sealed class SizeHeader : ColumnString<FishRecord>
        {
            public override string ToName(FishRecord record)
                => $"{record.Size / 10f:F1}";

            public override float Width
                => 50 * ImGuiHelpers.GlobalScale;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.Size.CompareTo(rhs.Size);

            public override void DrawColumn(FishRecord record, int _)
            {
                var tt = string.Empty;
                if (record.Flags.HasFlag(FishRecord.Effects.Large))
                    tt = "大鱼！";
                if (record.Flags.HasFlag(FishRecord.Effects.Collectible))
                    tt += tt.Length > 0 ? "\n具有收藏价值！" : "具有收藏价值！";
                using var color = ImRaii.PushColor(ImGuiCol.Text, ColorId.DisabledText.Value(), tt.Length == 0);
                ImGuiUtil.RightAlign(ToName(record));
                ImGuiUtil.HoverTooltip(tt);
            }
        }


        private sealed class ContentIdHeader : ColumnString<FishRecord>
        {
            public override string ToName(FishRecord item)
                => item.Flags.HasFlag(FishRecord.Effects.Legacy) ? "Legacy" : item.ContentIdHash.ToString("X8");

            public override float Width
                => 75 * ImGuiHelpers.GlobalScale;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.ContentIdHash.CompareTo(rhs.ContentIdHash);
        }

        private sealed class BaitHeader : ColumnString<FishRecord>
        {
            public override string ToName(FishRecord item)
                => item.Bait.Name;

            public override float Width
                => 150 * ImGuiHelpers.GlobalScale;
        }

        private sealed class SpotHeader : ColumnString<FishRecord>
        {
            public override string ToName(FishRecord item)
                => item.FishingSpot?.Name ?? "未知";

            public override float Width
                => 200 * ImGuiHelpers.GlobalScale;
        }

        private sealed class CatchHeader : ColumnString<FishRecord>
        {
            public CatchHeader()
            {
                Flags |= ImGuiTableColumnFlags.NoHide;
                Flags |= ImGuiTableColumnFlags.NoReorder;
            }

            public override string ToName(FishRecord record)
                => record.Catch?.Name[GatherBuddy.Language] ?? "暂无/脱钩";

            public override float Width
                => 200 * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(FishRecord record, int idx)
            {
                base.DrawColumn(record, idx);
                if (ImGui.GetIO().KeyCtrl && ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    _deleteIdx = idx;
                ImGuiUtil.HoverTooltip("按住Ctrl键并右键单击以删除...");
            }
        }

        private sealed class CastStartHeader : ColumnString<FishRecord>
        {
            public override string ToName(FishRecord record)
                => (record.TimeStamp.Time / 1000).ToString();

            public override float Width
                => 80 * ImGuiHelpers.GlobalScale;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.TimeStamp.CompareTo(rhs.TimeStamp);

            public override void DrawColumn(FishRecord record, int _)
            {
                base.DrawColumn(record, _);
                ImGuiUtil.HoverTooltip(record.TimeStamp.ToString());
            }
        }

        [Flags]
        private enum TugTypeFilter : byte
        {
            Weak      = 0x01,
            Strong    = 0x02,
            Legendary = 0x04,
            Invalid   = 0x08,
        }
        private static string ToName(TugTypeFilter type)
        {
            return type switch
            {
                TugTypeFilter.Weak      =>"轻杆",
                TugTypeFilter.Strong    =>"中杆",
                TugTypeFilter.Legendary =>"重杆",
                TugTypeFilter.Invalid   =>"错误",
                _ => "未知֪",
            };
        }
        private sealed class BiteTypeHeader : ColumnFlags<TugTypeFilter, FishRecord>
        {
            private TugTypeFilter[] FlagValues = Array.Empty<TugTypeFilter>();
            private string[] FlagNames = Array.Empty<string>();

            public BiteTypeHeader()
            {
                FlagValues = new TugTypeFilter[]{
                    TugTypeFilter.Weak , 
                    TugTypeFilter.Strong , 
                    TugTypeFilter.Legendary , 
                    TugTypeFilter.Invalid
                };
                FlagNames = FlagValues.Select(f => ToName(f)).ToArray();
                AllFlags = FlagValues.Aggregate((f, g) => f | g);
                _filter  = AllFlags;
            }
            protected sealed override string[] Names
                => FlagNames;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.Tug.CompareTo(rhs.Tug);

            public override void DrawColumn(FishRecord item, int idx)
                => ImGui.Text(FishRecord.ToName(item.Tug));

            private TugTypeFilter _filter;

            protected override void SetValue(TugTypeFilter value, bool enable)
            {
                if (enable)
                    _filter |= value;
                else
                    _filter &= ~value;
            }

            public override TugTypeFilter FilterValue
                => _filter;

            public override bool FilterFunc(FishRecord item)
                => item.Tug switch
                {
                    BiteType.Weak      => _filter.HasFlag(TugTypeFilter.Weak),
                    BiteType.Strong    => _filter.HasFlag(TugTypeFilter.Strong),
                    BiteType.Legendary => _filter.HasFlag(TugTypeFilter.Legendary),
                    _                  => _filter.HasFlag(TugTypeFilter.Invalid),
                };

            public override float Width
                => 60 * ImGuiHelpers.GlobalScale;
        }

        [Flags]
        private enum HookSetFilter : byte
        {
            Regular  = 0x01,
            Precise  = 0x02,
            Powerful = 0x04,
            Double   = 0x08,
            Triple   = 0x10,
            Invalid  = 0x20,
        }
        private static string ToName(HookSetFilter type)
        {
            return type switch
            {
                HookSetFilter.Regular  =>"普通提钩",
                HookSetFilter.Precise  =>"精准提钩",
                HookSetFilter.Powerful =>"强力提钩",
                HookSetFilter.Double   =>"双重提钩",
                HookSetFilter.Triple   =>"三重提钩",
                HookSetFilter.Invalid  =>"错误数据",
                _ => "未知֪",
            };
        }

        private sealed class HookHeader : ColumnFlags<HookSetFilter, FishRecord>
        {
            private HookSetFilter[] FlagValues = Array.Empty<HookSetFilter>();
            private string[] FlagNames = Array.Empty<string>();

            public HookHeader()
            {
                FlagValues = new HookSetFilter[]{ HookSetFilter.Precise,
                    HookSetFilter.Powerful,
                    HookSetFilter.Regular,
                    HookSetFilter.Double,
                    HookSetFilter.Triple,
                    HookSetFilter.Invalid
                 };
                FlagNames = FlagValues.Select(f => ToName(f)).ToArray();
                AllFlags = FlagValues.Aggregate((f, g) => f | g);
                _filter = AllFlags;

            }
            protected sealed override string[] Names
                => FlagNames;

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.Hook.CompareTo(rhs.Hook);

            public override void DrawColumn(FishRecord item, int idx)
                => ImGui.Text(item.Hook.ToName());

            private HookSetFilter _filter;

            protected override void SetValue(HookSetFilter value, bool enable)
            {
                if (enable)
                    _filter |= value;
                else
                    _filter &= ~value;
            }

            public override HookSetFilter FilterValue
                => _filter;

            public override bool FilterFunc(FishRecord item)
                => item.Hook switch
                {
                    HookSet.Precise    => _filter.HasFlag(HookSetFilter.Precise),
                    HookSet.Powerful   => _filter.HasFlag(HookSetFilter.Powerful),
                    HookSet.Hook       => _filter.HasFlag(HookSetFilter.Regular),
                    HookSet.DoubleHook => _filter.HasFlag(HookSetFilter.Double),
                    HookSet.TripleHook => _filter.HasFlag(HookSetFilter.Triple),
                    _                  => _filter.HasFlag(HookSetFilter.Invalid),
                };

            public override float Width
                => 75 * ImGuiHelpers.GlobalScale;
        }

        private sealed class DurationHeader : ColumnString<FishRecord>
        {
            public override string ToName(FishRecord record)
                => $"{record.Bite / 1000}.{record.Bite % 1000:D3}";

            public override float Width
                => 50 * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(FishRecord record, int _)
                => ImGuiUtil.RightAlign(ToName(record));

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.Bite.CompareTo(rhs.Bite);
        }


        private class FlagHeader : ColumnFlags<FishRecord.Effects, FishRecord>
        {
            private readonly float                               _iconScale;
            private readonly (TextureWrap, FishRecord.Effects)[] _effects;

            private static readonly FishRecord.Effects[] _values =
            {
                FishRecord.Effects.Patience,
                (FishRecord.Effects)((uint)FishRecord.Effects.Patience << 16),
                FishRecord.Effects.Patience2,
                (FishRecord.Effects)((uint)FishRecord.Effects.Patience2 << 16),
                FishRecord.Effects.Intuition,
                (FishRecord.Effects)((uint)FishRecord.Effects.Intuition << 16),
                FishRecord.Effects.Snagging,
                (FishRecord.Effects)((uint)FishRecord.Effects.Snagging << 16),
                FishRecord.Effects.FishEyes,
                (FishRecord.Effects)((uint)FishRecord.Effects.FishEyes << 16),
                FishRecord.Effects.Chum,
                (FishRecord.Effects)((uint)FishRecord.Effects.Chum << 16),
                FishRecord.Effects.PrizeCatch,
                (FishRecord.Effects)((uint)FishRecord.Effects.PrizeCatch << 16),
                FishRecord.Effects.IdenticalCast,
                (FishRecord.Effects)((uint)FishRecord.Effects.IdenticalCast << 16),
                FishRecord.Effects.SurfaceSlap,
                (FishRecord.Effects)((uint)FishRecord.Effects.SurfaceSlap << 16),
                FishRecord.Effects.Collectible,
                (FishRecord.Effects)((uint)FishRecord.Effects.Collectible << 16),
            };

            private const FishRecord.Effects Mask = FishRecord.Effects.Patience
              | FishRecord.Effects.Patience2
              | FishRecord.Effects.Intuition
              | FishRecord.Effects.Snagging
              | FishRecord.Effects.FishEyes
              | FishRecord.Effects.Chum
              | FishRecord.Effects.PrizeCatch
              | FishRecord.Effects.IdenticalCast
              | FishRecord.Effects.SurfaceSlap
              | FishRecord.Effects.Collectible;

            private static readonly string[] _names =
            {
                "耐心 开",
                "耐心 关",
                "耐心II 开",
                "耐心II 关",
                "鱼识 开",
                "鱼识 关",
                "钓组 开",
                "钓组 关",
                "鱼眼 开",
                "鱼眼 关",
                "撒饵 开",
                "撒饵 关",
                "大鱼猎手 开",
                "大鱼猎手 关",
                "专一垂钓 开",
                "专一垂钓 关",
                "拍击水面 开",
                "拍击水面 关",
                "收藏品采集 开",
                "收藏品采集 关",
            };

            protected override IReadOnlyList<FishRecord.Effects> Values
                => _values;

            protected override string[] Names
                => _names;

            protected override void SetValue(FishRecord.Effects value, bool enable)
            {
                if (enable)
                    _filter |= value;
                else
                    _filter &= ~value;
            }

            private FishRecord.Effects _filter;

            public FlagHeader()
            {
                _effects = new[]
                {
                    (Icons.DefaultStorage[16023], _values[0]),
                    (Icons.DefaultStorage[11106], _values[2]),
                    (Icons.DefaultStorage[11101], _values[4]),
                    (Icons.DefaultStorage[11102], _values[6]),
                    (Icons.DefaultStorage[11103], _values[8]),
                    (Icons.DefaultStorage[11104], _values[10]),
                    (Icons.DefaultStorage[11119], _values[12]),
                    (Icons.DefaultStorage[11116], _values[14]),
                    (Icons.DefaultStorage[11115], _values[16]),
                    (Icons.DefaultStorage[11008], _values[18]),
                };
                _iconScale = (float)_effects[0].Item1.Width / _effects[0].Item1.Height;
                AllFlags   = Mask | (FishRecord.Effects)((uint)Mask << 16);
                _filter    = AllFlags;
            }

            public override float Width
                => 10 * (_iconScale * TextHeight + 1);

            public override bool FilterFunc(FishRecord item)
            {
                var enabled  = _filter & Mask;
                var disabled = (FishRecord.Effects)((int)_filter >> 16) & Mask;
                var flags    = item.Flags & Mask;
                var invFlags = ~flags & Mask;
                return (flags & enabled) == flags && (invFlags & disabled) == invFlags;
            }

            public override int Compare(FishRecord lhs, FishRecord rhs)
                => lhs.Flags.CompareTo(rhs.Flags);

            public override FishRecord.Effects FilterValue
                => _filter;

            private void DrawIcon(FishRecord item, TextureWrap icon, FishRecord.Effects flag)
            {
                var size = new Vector2(TextHeight * _iconScale, TextHeight);
                var tint = item.Flags.HasFlag(flag) ? Vector4.One : new Vector4(0.75f, 0.75f, 0.75f, 0.5f);
                ImGui.Image(icon.ImGuiHandle, size, Vector2.Zero, Vector2.One, tint);
                if (!ImGui.IsItemHovered())
                    return;

                using var tt = ImRaii.Tooltip();
                ImGui.Image(icon.ImGuiHandle, new Vector2(icon.Width, icon.Height));
                ImGui.Text(FishRecord.EffectsToName(flag));
            }

            public override void DrawColumn(FishRecord item, int idx)
            {
                using var space = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.One);
                foreach (var (icon, flag) in _effects)
                {
                    DrawIcon(item, icon, flag);
                    ImGui.SameLine();
                }

                ImGui.NewLine();
            }
        }
    }

    private readonly RecordTable _recordTable;


    private void DrawRecordTab()
    {
        using var id  = ImRaii.PushId("Fish Records");
        using var tab = ImRaii.TabItem("钓鱼记录");
        ImGuiUtil.HoverTooltip("钓鱼记录总能彰显出我不可思议的钓鱼实力。\n"
          + "查找、清理并分享所有当你在钓鱼时自动收集的钓鱼数据。");
        if (!tab)
            return;

        _recordTable.Draw(ImGui.GetTextLineHeightWithSpacing());
        if (ImGui.Button("清理"))
        {
            _plugin.FishRecorder.RemoveDuplicates();
            _plugin.FishRecorder.RemoveInvalid();
        }

        ImGuiUtil.HoverTooltip("删除所有因为种种原因被标记为错误的数据，\n"
          + "以及所有重复的数据 (有相同的 content id 和时间戳)。\n"
          + "通常来说不会产生这样的数据项。\n"
          + "使用该功能需要风险自负，插件不会自动备份这些数据。");

        ImGui.SameLine();
        try
        {
            if (ImGui.Button("复制到粘贴板"))
                ImGui.SetClipboardText(_plugin.FishRecorder.ExportBase64(0, _plugin.FishRecorder.Records.Count));
            ImGuiUtil.HoverTooltip("导出所有钓鱼记录到粘贴板来分享给你的朋友，这可能非常多。");
        }
        catch
        {
            // ignored
        }

        ImGui.SameLine();
        try
        {
            if (ImGui.Button("从粘贴板导入"))
                _plugin.FishRecorder.ImportBase64(ImGui.GetClipboardText());
            ImGuiUtil.HoverTooltip("从你的粘贴板导入一系列的钓鱼记录。会自动跳过重复项。");
        }
        catch
        {
            // ignored
        }

        ImGui.SameLine();
        try
        {
            if (ImGui.Button("导出为JSON"))
                ImGui.OpenPopup(RecordTable.FileNamePopup);
            ImGuiUtil.HoverTooltip("给定一个地址，导出所有钓鱼记录到一个JSON文件。");
        }
        catch
        {
            // ignored
        }

        ImGui.SameLine();
        try
        {
            if (ImGui.Button("复制鱼获数据JSON"))
            {
                var logFish = GatherBuddy.GameData.Fishes.Values.Where(f => f.InLog && f.FishingSpots.Count > 0).ToArray();
                var ids     = logFish.Where(f => GatherBuddy.FishLog.IsUnlocked(f)).Select(f => f.ItemId).ToArray();
                Communicator.PrintClipboardMessage("总计 ", $"{ids.Length}/{logFish.Length} 已捕获的鱼 ");
                ImGui.SetClipboardText(JsonConvert.SerializeObject(ids, Formatting.Indented));
            }
        }
        catch
        {
            // ignored
        }

        var name = string.Empty;
        if (!ImGuiUtil.OpenNameField(RecordTable.FileNamePopup, ref name) || name.Length <= 0)
            return;

        try
        {
            var file = new FileInfo(name);
            _plugin.FishRecorder.ExportJson(file);
        }
        catch
        {
            // ignored
        }
    }
}
