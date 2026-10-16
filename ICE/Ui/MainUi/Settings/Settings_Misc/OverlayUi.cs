using Dalamud.Interface.Utility;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;

namespace ICE.Ui.MainUi.Settings.Settings_Misc;

public static partial class SettingsUi
{
    private const string OverlayCategory = "悬浮窗";

    private static readonly Dictionary<uint, string> OverlayClassNames = new()
    {
        [8] = "CRP",
        [9] = "BSM",
        [10] = "ARM",
        [11] = "GSM",
        [12] = "LTW",
        [13] = "WVR",
        [14] = "ALC",
        [15] = "CUL",
        [16] = "MIN",
        [17] = "BTN",
        [18] = "FSH",
    };

    private static readonly SettingEntry Overlay_AutoOpen = new()
    {
        Label = "自动打开悬浮窗",
        Category = OverlayCategory,
        Keywords = new[] { "Overlay", "Open", "Auto" },
        Draw = () =>
        {
            var showOverlay = C.ShowOverlay;
            if (ImGui.Checkbox("自动打开悬浮窗", ref showOverlay))
            {
                C.ShowOverlay = showOverlay;
                C.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button("打开悬浮窗") && !P.overlayWindow.IsOpen)
                P.overlayWindow.IsOpen = true;
        }
    };

    private static readonly SettingEntry Overlay_CogsIcon = Toggle(
        "使用齿轮按钮替代主页按钮", OverlayCategory,
        new[] { "Overlay", "Cogs", "Home", "Button", "Icon" },
        () => C.Overlay_UseCogsIcon,
        v => C.Overlay_UseCogsIcon = v);

    private static readonly SettingEntry Overlay_ShowSeconds = Toggle(
        "显示秒数", OverlayCategory,
        new[] { "Overlay", "Seconds", "Time" },
        () => C.ShowSeconds,
        v => C.ShowSeconds = v);

    private static readonly SettingEntry Overlay_ExpBars = new()
    {
        Label = "在悬浮窗显示宇宙工具研究数据",
        Category = OverlayCategory,
        Keywords = new[] { "Overlay", "Experience", "Exp", "XP", "Bars", "Maxed" },
        Draw = () =>
        {
            var showExp = C.ShowExpBars;
            if (ImGui.Checkbox("在悬浮窗显示宇宙工具研究数据", ref showExp))
            {
                C.ShowExpBars = showExp;
                C.Save();
            }

            if (!showExp)
                return;

            ImGui.SameLine();
            var hideWhenMaxed = C.ShowExpBars_HideWhenMaxed;
            if (ImGui.Checkbox("仅在达到上限前显示", ref hideWhenMaxed))
            {
                C.ShowExpBars_HideWhenMaxed = hideWhenMaxed;
                C.Save();
            }
        }
    };

    private static readonly SettingEntry Overlay_Scores = new()
    {
        Label = "技巧点显示",
        Category = OverlayCategory,
        Keywords = new[] { "Overlay", "Score", "Class", "Total", "Mastery", "Current" },
        Draw = () =>
        {
            var showClassScore = C.ShowCurrentScore;
            if (ImGui.Checkbox("显示当前职业技巧点", ref showClassScore))
            {
                C.ShowCurrentScore = showClassScore;
                C.Save();
            }

            ImGui.SameLine();
            var showTotalScore = C.ShowTotalScore;
            if (ImGui.Checkbox("显示总技巧点", ref showTotalScore))
            {
                C.ShowTotalScore = showTotalScore;
                C.Save();
            }

            ImGui.SameLine();
            var showMasteryScore = C.ShowMasteryScore;
            if (ImGui.Checkbox("显示当前职业熟练点", ref showMasteryScore))
            {
                C.ShowMasteryScore = showMasteryScore;
                C.Save();
            }
        }
    };

    private static readonly SettingEntry Overlay_AutoResize = Toggle(
        "自动调整悬浮窗大小", OverlayCategory,
        new[] { "Overlay", "Resize", "Size" },
        () => C.Overlay_AutoResize,
        v => C.Overlay_AutoResize = v);

    private static readonly SettingEntry Overlay_HighlightWeather = Toggle(
        "高亮 EX+ 票据天气", OverlayCategory,
        new[] { "Overlay", "Highlight", "Weather", "Token", "EX+" },
        () => C.Overlay_HighlightTokenWeather,
        v => C.Overlay_HighlightTokenWeather = v);

    private static readonly SettingEntry Overlay_WeatherSelected = Toggle(
        "悬停天气时显示已启用任务", OverlayCategory,
        new[] { "Overlay", "Weather", "Hover", "Missions", "Enabled", "Selected" },
        () => C.Overlay_WeatherSelected,
        v => C.Overlay_WeatherSelected = v);

    private static readonly SettingEntry Overlay_JobFilter = new()
    {
        Label = "仅按当前职业筛选",
        Category = OverlayCategory,
        Keywords = new[] { "Overlay", "Filter", "Job", "Class", "Current" },
        Draw = () =>
        {
            var filterByCurrentJob = C.Overlay_FilterByCurrentJob;
            if (ImGui.Checkbox("仅按当前职业筛选", ref filterByCurrentJob))
            {
                C.Overlay_FilterByCurrentJob = filterByCurrentJob;
                C.Save();
            }

            if (filterByCurrentJob)
                return;

            var iconSize = 26 * ImGuiHelpers.GlobalScale;
            const float iconSpacing = 4;

            foreach (var (jobId, name) in OverlayClassNames)
            {
                var isSelected = C.Overlay_FilterJobs.Contains(jobId);
                var icon = isSelected
                    ? CosmicHelper.ClassInfoDict.TryGetValue(jobId, out var tex) ? tex.JobIcon.GetWrapOrEmpty() : null
                    : ImGui_Ice.GetGreyscaleJob(jobId);

                if (icon != null && ImGui_Ice.DrawStyledImageButton(icon, new Vector2(iconSize, iconSize), isSelected))
                {
                    if (isSelected)
                        C.Overlay_FilterJobs.Remove(jobId);
                    else
                        C.Overlay_FilterJobs.Add(jobId);
                    C.Save();
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(name);

                ImGui.SameLine(0, iconSpacing);
            }
            ImGui.NewLine();
        }
    };

    private static readonly SettingEntry Overlay_HudClipping = Toggle(
        "禁用 HUD 裁切", OverlayCategory,
        new[] { "Overlay", "HUD", "Clipping", "Native", "UI" },
        () => C.DisableHudClipping,
        v => C.DisableHudClipping = v,
        "启用后，绘制叠加层将渲染在原生 UI 元素上方");
}
