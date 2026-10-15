using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.Settings.Settings_Misc;

public static partial class SettingsUi
{
    private const string SafetyCategory = "安全设置";

    private static readonly SettingEntry Safety_MissionDelay = new()
    {
        Label = "任务菜单添加延迟",
        Category = SafetyCategory,
        Keywords = new[] { "Delay", "Mission", "Menu", "Grab", "Safety", "Animation Lock", "ms" },
        Draw = () =>
        {
            var enabled = C.DelayGrabMission;
            if (ImGui.Checkbox("任务菜单添加延迟", ref enabled))
            {
                C.DelayGrabMission = enabled;
                C.Save();
            }

            ImGuiEx.HelpMarker(
                "此为安全保护机制！若想缩短任务间隔时间，请随意调整。\n" +
                "安全值大概在……250？若遇到动画锁，完全可以调得更高\n" +
                "当然，想追求刺激的话……调低也行。反正我不是你老爸（不过老爸笑话管够）。");

            if (!enabled)
                return;

            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            var amount = C.DelayIncrease;
            if (ImGui.SliderInt("毫秒###Mission", ref amount, 0, 1000))
            {
                C.DelayIncrease = amount;
                C.SaveDebounced();
            }
        }
    };

    private static readonly SettingEntry Safety_CraftDelay = new()
    {
        Label = "制作菜单添加延迟",
        Category = SafetyCategory,
        Keywords = new[] { "Delay", "Craft", "Crafting", "Menu", "Turnin", "Safety", "Animation Lock", "ms" },
        Draw = () =>
        {
            var enabled = C.DelayCraft;
            if (ImGui.Checkbox("制作菜单添加延迟", ref enabled))
            {
                C.DelayCraft = enabled;
                C.Save();
            }

            ImGuiEx.HelpMarker(
                "此为安全保护机制！若想缩短汇报延迟时间，请随意调整。\n" +
                "安全值大概在……2500？若遇到动画锁，完全可以调得更高\n" +
                "当然，想追求刺激的话……调低也行。反正我不是你老爸（不过老爸笑话管够）。");

            if (!enabled)
                return;

            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            var amount = C.DelayCraftIncrease;
            if (ImGui.SliderInt("毫秒###Crafting", ref amount, 500, 5000))
            {
                C.DelayCraftIncrease = amount;
                C.SaveDebounced();
            }
        }
    };

    private static readonly SettingEntry Safety_RelicDelay = new()
    {
        Label = "宇宙工具提交后延迟",
        Category = SafetyCategory,
        Keywords = new[] { "Delay", "Relic", "Turnin", "Post", "Safety", "ms" },
        Draw = () =>
        {
            var delay = C.DelayPostRelic;
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("宇宙工具提交后延迟", ref delay, 0, 5000))
            {
                C.DelayPostRelic = delay;
                C.SaveDebounced();
            }
        }
    };

    private static readonly SettingEntry Safety_GatherDelay = Toggle(
        "采集添加延迟", SafetyCategory,
        new[] { "Delay", "Gather", "Gathering", "Safety" },
        () => C.Delay_Gather,
        v => C.Delay_Gather = v);

    private static readonly SettingEntry Safety_CloseReward = Toggle(
        "自动关闭奖励弹窗", SafetyCategory,
        new[] { "Reward", "Popup", "Close", "Hide", "Window", "Auto" },
        () => C.HideRewardWindow,
        v => C.HideRewardWindow = v);
}