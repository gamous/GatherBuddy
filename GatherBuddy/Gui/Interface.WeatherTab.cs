﻿using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using GatherBuddy.Config;
using GatherBuddy.Time;
using ImGuiNET;
using OtterGui;
using OtterGui.Table;
using ImGuiScene;
using ImRaii = OtterGui.Raii.ImRaii;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private sealed class WeatherTable : Table<CachedWeather>, IDisposable
    {
        private static readonly string[] WeatherTimeStrings = new string[CachedWeather.NumWeathers];

        private static float _textHeightIconOffset = 0;
        private static float _centerOffset         = 0;
        private static float _zoneSize             = 0;
        private static float _weatherSize          = 0;
        private static float _headerSize           = 0;
        private        bool  _weathersDirty        = true;

        public WeatherTable()
            : base("WeatherTable", CachedWeather.CreateWeatherCache(),
                Enumerable.Range(0, CachedWeather.NumWeathers).Select(i => (Column<CachedWeather>)new WeatherHeader(i))
                    .Prepend(new ZoneHeader()).ToArray())
        {
            GatherBuddy.Time.WeatherChanged += SetDirty;
            Flags                           &= ~ImGuiTableFlags.NoBordersInBody;
            Flags                           &= ~ImGuiTableFlags.BordersInnerH;
            Sortable                        =  false;
        }

        public void Dispose()
            => GatherBuddy.Time.WeatherChanged -= SetDirty;

        internal void SetDirty()
            => _weathersDirty = true;

        private sealed class ZoneHeader : ColumnString<CachedWeather>
        {
            public ZoneHeader()
                => Label = "地区...";

            public override float Width
                => _zoneSize * ImGuiHelpers.GlobalScale;

            public override string ToName(CachedWeather item)
                => item.Zone;

            public override void DrawColumn(CachedWeather item, int _)
            {
                var pos = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(pos + _textHeightIconOffset);
                ImGui.Text(item.Zone);
            }
        }

        private sealed class WeatherHeader : Column<CachedWeather>
        {
            private readonly ColorId _headerColor;
            private readonly ColorId _cellColor;
            private readonly int     _idx;

            public override float Width
                => GatherBuddy.Config.ShowWeatherNames
                    ? _weatherSize * ImGuiHelpers.GlobalScale + WeatherIconSize.X + ItemSpacing.X
                    : _headerSize;

            public WeatherHeader(int idx)
            {
                _idx = idx;
                _headerColor = idx switch
                {
                    0 => ColorId.WeatherTabHeaderLast,
                    1 => ColorId.WeatherTabHeaderCurrent,
                    _ => 0,
                };
                _cellColor = idx switch
                {
                    0 => ColorId.WeatherTabLast,
                    1 => ColorId.WeatherTabCurrent,
                    _ => 0,
                };
            }

            public override bool DrawFilter()
            {
                if (_headerColor != 0)
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, _headerColor.Value());

                ImGui.Text(WeatherTimeStrings[_idx]);
                return false;
            }

            public override void DrawColumn(CachedWeather line, int _)
            {
                if (_cellColor != 0)
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, _cellColor.Value());

                var (weather, icon) = line.Weathers[_idx];
                if (GatherBuddy.Config.ShowWeatherNames)
                    NamedWeather(icon, weather.Name);
                else
                    CenteredWeather(icon, weather, _centerOffset);
            }
        }

        protected override void PreDraw()
        {
            if (_weatherSize == 0)
            {
                _zoneSize    = Items.Max(c => ImGui.CalcTextSize(c.Zone).X) / ImGuiHelpers.GlobalScale;
                _weatherSize = GatherBuddy.GameData.Weathers.Values.Max(w => ImGui.CalcTextSize(w.Name).X) / ImGuiHelpers.GlobalScale;
                _headerSize  = ImGui.CalcTextSize(" 88:88:88 ").X / ImGuiHelpers.GlobalScale;
            }

            _centerOffset         = (_headerSize - WeatherIconSize.X - ImGui.GetStyle().ItemInnerSpacing.X / 2) / 2;
            _textHeightIconOffset = (WeatherIconSize.Y - TextHeight) / 2;

            if (!_weathersDirty)
                return;

            // Update times
            var sync = GatherBuddy.Time.ServerTime.SyncToEorzeaWeather();
            for (var i = 0; i < CachedWeather.NumWeathers; ++i)
            {
                var time = sync.AddEorzeaHours((i - 1) * 8).LocalTime;
                WeatherTimeStrings[i] = $" {time.TimeOfDay} ";
            }

            // Update weathers
            foreach (var item in Items)
                item.Update();
            _weathersDirty = false;
        }

        private static void NamedWeather(TextureWrap icon, string name)
        {
            var cursor = ImGui.GetCursorPos();
            ImGui.Image(icon.ImGuiHandle, WeatherIconSize);
            ImGui.SetCursorPos(cursor + new Vector2(WeatherIconSize.X + ItemSpacing.X / 2, _textHeightIconOffset));
            ImGui.Text(name);
        }

        private static void CenteredWeather(TextureWrap icon, Structs.Weather weather, float offset)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
            ImGui.Image(icon.ImGuiHandle, WeatherIconSize);
            ImGuiUtil.HoverTooltip($"{weather.Name} ({weather.Id})");
        }
    }

    private readonly WeatherTable _weatherTable = new();

    private void DrawWeatherTab()
    {
        using var id  = ImRaii.PushId("Weather");
        using var tab = ImRaii.TabItem("天气");
        ImGuiUtil.HoverTooltip("是的，'妖雾'(阴郁) 也是一种天气。\n"
          + "查看所有地区接下来的天气预报，以及刚刚那个。");

        if (!tab)
            return;

        _weatherTable.Draw(WeatherIconSize.Y + ImGui.GetStyle().ItemSpacing.Y);
    }
}
