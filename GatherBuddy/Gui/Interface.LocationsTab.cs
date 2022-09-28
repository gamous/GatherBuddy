﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using GatherBuddy.Classes;
using GatherBuddy.Config;
using GatherBuddy.Enums;
using GatherBuddy.Interfaces;
using ImGuiNET;
using OtterGui;
using OtterGui.Table;
using OtterGui.Widgets;
using ImRaii = OtterGui.Raii.ImRaii;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private sealed class LocationTable : Table<ILocation>
    {
        private static float _nameColumnWidth      = 0;
        private static float _territoryColumnWidth = 0;
        private static float _aetheryteColumnWidth = 0;
        private static float _coordColumnWidth     = 0;
        private static float _radiusColumnWidth    = 0;
        private static float _typeColumnWidth      = 0;

        protected override void PreDraw()
        {
            if (_nameColumnWidth != 0)
                return;

            _nameColumnWidth      = _plugin.LocationManager.AllLocations.Max(l => TextWidth(l.Name)) / ImGuiHelpers.GlobalScale;
            _territoryColumnWidth = _plugin.LocationManager.AllLocations.Max(l => TextWidth(l.Territory.Name)) / ImGuiHelpers.GlobalScale;
            _aetheryteColumnWidth = GatherBuddy.GameData.Aetherytes.Values.Max(a => TextWidth(a.Name)) / ImGuiHelpers.GlobalScale;
            _coordColumnWidth     = TextWidth("X-Coord") / ImGuiHelpers.GlobalScale + Table.ArrowWidth;
            _radiusColumnWidth    = TextWidth("Radius") / ImGuiHelpers.GlobalScale + Table.ArrowWidth;
            _typeColumnWidth      = Enum.GetValues<GatheringType>().Max(t => TextWidth(t.ToString())) / ImGuiHelpers.GlobalScale;
        }

        private static readonly NameColumn      _nameColumn      = new() { Label = "地点名称" };
        private static readonly TypeColumn      _typeColumn      = new() { Label = "类型" };
        private static readonly TerritoryColumn _territoryColumn = new() { Label = "地图" };
        private static readonly AetheryteColumn _aetheryteColumn = new() { Label = "以太之光" };
        private static readonly XCoordColumn    _xCoordColumn    = new() { Label = "X轴坐标" };
        private static readonly YCoordColumn    _yCoordColumn    = new() { Label = "Y轴坐标" };
        private static readonly RadiusColumn    _radiusColumn    = new() { Label = "范围" };
        private static readonly MarkerColumn    _markerColumn    = new() { Label = "场景标点" };
        private static readonly ItemColumn      _itemColumn      = new() { Label = "物品列表" };

        private sealed class NameColumn : ColumnString<ILocation>
        {
            public NameColumn()
                => Flags |= ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.NoReorder;

            public override string ToName(ILocation location)
                => location.Name;

            public override float Width
                => _nameColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ILocation item, int _)
            {
                ImGui.AlignTextToFramePadding();
                base.DrawColumn(item, _);
            }
        }

        private sealed class TypeColumn : ColumnFlags<JobFlags, ILocation>
        {
            private JobFlags[] FlagValues = Array.Empty<JobFlags>();
            private string[] FlagNames = Array.Empty<string>();
            protected sealed override string[] Names => FlagNames;
            public TypeColumn()
            {
                FlagValues = Enum.GetValues<JobFlags>();
                AllFlags = FlagValues.Aggregate((a, b) => a | b);
                FlagNames = FlagValues.Select(f => f.ToName()).ToArray();
            }

            public override JobFlags FilterValue
                => GatherBuddy.Config.LocationFilter;

            protected override void SetValue(JobFlags value, bool enable)
            {
                var val = enable
                    ? GatherBuddy.Config.LocationFilter | value
                    : GatherBuddy.Config.LocationFilter & ~value;
                if (val != GatherBuddy.Config.LocationFilter)
                {
                    GatherBuddy.Config.LocationFilter = val;
                    GatherBuddy.Config.Save();
                }
            }

            public override void DrawColumn(ILocation location, int _)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text(location.GatheringType.ToName());
            }

            public override int Compare(ILocation a, ILocation b)
                => a.GatheringType.CompareTo(b.GatheringType);

            public override bool FilterFunc(ILocation location)
            {
                return location.GatheringType switch
                {
                    GatheringType.Mining       => FilterValue.HasFlag(JobFlags.Mining),
                    GatheringType.Quarrying    => FilterValue.HasFlag(JobFlags.Quarrying),
                    GatheringType.Logging      => FilterValue.HasFlag(JobFlags.Logging),
                    GatheringType.Harvesting   => FilterValue.HasFlag(JobFlags.Harvesting),
                    GatheringType.Spearfishing => FilterValue.HasFlag(JobFlags.Spearfishing),
                    GatheringType.Fisher       => FilterValue.HasFlag(JobFlags.Fishing),
                    _                          => false,
                };
            }

            public override float Width
                => _typeColumnWidth * ImGuiHelpers.GlobalScale;
        }

        private sealed class TerritoryColumn : ColumnString<ILocation>
        {
            public override string ToName(ILocation location)
                => location.Territory.Name;

            public override float Width
                => _territoryColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ILocation item, int _)
            {
                ImGui.AlignTextToFramePadding();
                base.DrawColumn(item, _);
            }
        }

        private sealed class ItemColumn : ColumnString<ILocation>
        {
            public ItemColumn()
                => Flags |= ImGuiTableColumnFlags.WidthStretch;

            public override string ToName(ILocation location)
                => string.Join(", ", location.Gatherables.Select(g => g.Name[GatherBuddy.Language]));

            public override float Width
                => 0;

            public override void DrawColumn(ILocation item, int _)
            {
                ImGui.AlignTextToFramePadding();
                base.DrawColumn(item, _);
            }
        }

        private sealed class AetheryteColumn : ColumnString<ILocation>
        {
            private readonly List<Aetheryte>                   _aetherytes;
            private readonly ClippedSelectableCombo<Aetheryte> _aetheryteCombo;

            public AetheryteColumn()
            {
                _aetherytes     = GatherBuddy.GameData.Aetherytes.Values.ToList();
                _aetheryteCombo = new ClippedSelectableCombo<Aetheryte>("##aetheryte", string.Empty, 200, _aetherytes, a => a.Name);
            }

            public override string ToName(ILocation location)
                => location.ClosestAetheryte?.Name ?? "暂无";

            public override float Width
                => _aetheryteColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ILocation location, int _)
            {
                var       overwritten = location.DefaultAetheryte != location.ClosestAetheryte;
                using var color       = ImRaii.PushColor(ImGuiCol.FrameBg, ColorId.ChangedLocationBg.Value(), overwritten);
                var       currentName = location.ClosestAetheryte?.Name ?? "暂无";
                if (_aetheryteCombo.Draw(currentName, out var newIdx))
                    _plugin.LocationManager.SetAetheryte(location, _aetherytes[newIdx]);
                if (overwritten)
                {
                    ImGuiUtil.HoverTooltip($"右键恢复默认值。 ({location.DefaultAetheryte?.Name ?? "暂无"})");
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        _plugin.LocationManager.SetAetheryte(location, location.DefaultAetheryte);
                }
            }
        }

        private sealed class XCoordColumn : ColumnString<ILocation>
        {
            public override string ToName(ILocation location)
                => (location.IntegralXCoord / 100f).ToString("0.00", CultureInfo.InvariantCulture);

            public override float Width
                => _coordColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ILocation location, int _)
            {
                var       overwritten = location.DefaultXCoord != location.IntegralXCoord;
                using var color       = ImRaii.PushColor(ImGuiCol.FrameBg, ColorId.ChangedLocationBg.Value(), overwritten);
                var       x           = location.IntegralXCoord / 100f;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.DragFloat("##x", ref x, 0.05f, 1f, 42f, "%.2f", ImGuiSliderFlags.AlwaysClamp))
                    _plugin.LocationManager.SetXCoord(location, (int)(x * 100f + 0.5f));
                if (overwritten)
                {
                    ImGuiUtil.HoverTooltip($"右键恢复默认值。 ({location.DefaultXCoord / 100f:0.00})");
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        _plugin.LocationManager.SetXCoord(location, location.DefaultXCoord);
                }
            }

            public override int Compare(ILocation a, ILocation b)
                => a.IntegralXCoord.CompareTo(b.IntegralXCoord);
        }

        private sealed class YCoordColumn : ColumnString<ILocation>
        {
            public override string ToName(ILocation location)
                => location.IntegralYCoord.ToString("0.00", CultureInfo.InvariantCulture);

            public override float Width
                => _coordColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ILocation location, int _)
            {
                var       overwritten = location.DefaultYCoord != location.IntegralYCoord;
                using var color       = ImRaii.PushColor(ImGuiCol.FrameBg, ColorId.ChangedLocationBg.Value(), overwritten);
                var       y           = location.IntegralYCoord / 100f;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.DragFloat("##y", ref y, 0.05f, 1f, 42f, "%.2f", ImGuiSliderFlags.AlwaysClamp))
                    _plugin.LocationManager.SetYCoord(location, (int)(y * 100f + 0.5f));
                if (overwritten)
                {
                    ImGuiUtil.HoverTooltip($"右键恢复默认值。 ({location.DefaultYCoord / 100f:0.00})");
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        _plugin.LocationManager.SetYCoord(location, location.DefaultYCoord);
                }
            }

            public override int Compare(ILocation a, ILocation b)
                => a.IntegralYCoord.CompareTo(b.IntegralYCoord);
        }

        private sealed class RadiusColumn : ColumnString<ILocation>
        {
            public override string ToName(ILocation location)
                => location.Radius.ToString();

            public override float Width
                => _radiusColumnWidth * ImGuiHelpers.GlobalScale;

            public override void DrawColumn(ILocation location, int _)
            {
                var       overwritten = location.DefaultRadius != location.Radius;
                using var color       = ImRaii.PushColor(ImGuiCol.FrameBg, ColorId.ChangedLocationBg.Value(), overwritten);
                ImGui.SetNextItemWidth(-1);
                int radius = location.Radius;
                if (ImGui.DragInt("##radius", ref radius, 0.1f, 0, IMarkable.RadiusMax))
                    _plugin.LocationManager.SetRadius(location, Math.Clamp((ushort)radius, (ushort)0, IMarkable.RadiusMax));
                if (overwritten)
                {
                    ImGuiUtil.HoverTooltip($"右键恢复默认值。 ({location.DefaultRadius})");
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        _plugin.LocationManager.SetRadius(location, location.DefaultRadius);
                }
            }

            public override int Compare(ILocation a, ILocation b)
                => a.Radius.CompareTo(b.Radius);
        }

        [Flags]
        private enum MarkerFlags : byte
        {
            None = 0x01,
            Any  = 0x02,
        }

        private sealed class MarkerColumn : ColumnFlags<MarkerFlags, ILocation>
        {
            public override int Compare(ILocation lhs, ILocation rhs)
            {
                if (lhs.Markers.Length != rhs.Markers.Length)
                    return lhs.Markers.Length - rhs.Markers.Length;

                var diff = lhs.Territory.Id.CompareTo(rhs.Territory.Id);
                if (diff != 0)
                    return diff;

                foreach (var (l, r) in lhs.Markers.Zip(rhs.Markers))
                {
                    diff = l.X.CompareTo(r.X);
                    if (diff != 0)
                        return diff;

                    diff = l.Y.CompareTo(r.Y);
                    if (diff != 0)
                        return diff;

                    diff = l.Z.CompareTo(r.Z);
                    if (diff != 0)
                        return diff;
                }

                return 0;
            }

            private MarkerFlags _filter = MarkerFlags.None | MarkerFlags.Any;

            public override MarkerFlags FilterValue
                => _filter;

            protected override void SetValue(MarkerFlags value, bool enable)
            {
                _filter = enable ? _filter | value : _filter & ~value;
            }


            public override bool FilterFunc(ILocation item)
                => FilterValue.HasFlag(item.Markers.Length == 0 ? MarkerFlags.None : MarkerFlags.Any);

            public override float Width
                => ImGui.GetFrameHeight() * 2 + ImGui.GetStyle().ItemSpacing.X + Table.ArrowWidth;

            public override void DrawColumn(ILocation location, int id)
            {
                using var _ = ImRaii.PushId(id);
                var markers = GatherBuddy.WaymarkManager.GetWaymarks();
                var invalid = Dalamud.ClientState.TerritoryType != location.Territory.Id;
                var tt = invalid ? "当前不在该地点。" :
                    markers.Count == 0 ? "当前没有可用于存储到该地点的场地标点。" :
                                         $"存储当前场地标点到该采集点:\n\n{string.Join("\n", markers.Select(m => $"{m.X:F2} - {m.Y:F2} - {m.Z:F2}"))}";

                if (location.Markers.Length > 0)
                    tt +=
                        $"\n\n存储到该地点的场地标点:\n\n{string.Join("\n", location.Markers.Select(m => $"{m.X:F2} - {m.Y:F2} - {m.Z:F2}"))}";

                if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Map.ToIconString(), new Vector2(ImGui.GetFrameHeight()), tt,
                        markers.Count == 0 || invalid, true))
                    _plugin.LocationManager.SetMarkers(location, markers);

                ImGui.SameLine();
                tt = location.Markers.Length == 0
                    ? "没有存储到该地点的场地标点。"
                    : $"移除存储到该地点的场地标点:\n\n{string.Join("\n", location.Markers.Select(m => $"{m.X:F2} - {m.Y:F2} - {m.Z:F2}"))}";
                if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight()), tt,
                        location.Markers.Length == 0, true))
                    _plugin.LocationManager.SetMarkers(location, Array.Empty<Vector3>());
            }
        }

        public LocationTable()
            : base("##LocationTable", _plugin.LocationManager.AllLocations, _nameColumn,
                _typeColumn, _aetheryteColumn, _xCoordColumn, _yCoordColumn, _radiusColumn, _markerColumn, _territoryColumn, _itemColumn)
        { }
    }

    private readonly LocationTable _locationTable;

    private void DrawLocationsTab()
    {
        using var id  = ImRaii.PushId("Locations");
        using var tab = ImRaii.TabItem("地点");
        ImGuiUtil.HoverTooltip("默认的地点位置让你出糗了?\n"
          + "为特定采集地点设置自定义的传送点和场地标点。");

        if (!tab)
            return;

        _locationTable.Draw(ImGui.GetFrameHeightWithSpacing());
    }
}
