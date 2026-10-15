using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.ImGuiTools;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings.Settings_Misc;

public static partial class SettingsUi
{
    private const string AutoUseCategory = "自动使用";
    private const string PostMissionCategory = "任务后设置";
    private const string RecordCategory = "统计设置";
    private const string PostCommandsCategory = "任务后命令";
    private const string FunCategory = "趣味设置";
    private const string NotificationCategory = "Notification Master";

    #region Auto Use

    private static readonly SettingEntry AutoUse_RemoveStellar = new()
    {
        Label = "自动取消贡献之星状态效果",
        Category = AutoUseCategory,
        Keywords = new[] { "Stellar", "Status", "Remove", "Star Contributor", "Glow", "Buff" },
        Draw = () =>
        {
            var removeStellar = C.RemoveStellarStatus;
            if (ImGui.Checkbox("自动取消贡献之星状态效果", ref removeStellar))
            {
                C.RemoveStellarStatus = removeStellar;
                C.Save();
            }

            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.InfoCircle,
                "自动移除贡献之星视觉效果（身为顶级贡献者时获得的发光效果）。\n" +
                "重新进入该区域时增益会自行恢复。");
        }
    };

    private static readonly SettingEntry AutoUse_StartOnMoon = new()
    {
        Label = "进入宇宙探索地图时自动启动 ICE 运行",
        Category = AutoUseCategory,
        Keywords = new[] { "Auto", "Start", "Enter", "Moon", "Cosmic Exploration", "Zone" },
        Draw = () =>
        {
            var startOnMoon = C.StartUponEnterMoon;
            if (ImGui.Checkbox("进入宇宙探索地图时自动启动 ICE 运行", ref startOnMoon))
            {
                C.StartUponEnterMoon = startOnMoon;
                C.Save();
            }

            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "首次进入宇宙探索地图时，若检测到您的职业为能工巧匠或大地使者，\n" +
                "将自动启动 ICE 运行，等同于您手动按下开始按钮。\n" +
                "适用于使用自动登录工具/只想进入宇宙探索地图后直接开始的情况。\n" +
                "注意：此功能仅在【首次】进入宇宙探索地图时生效。");
        }
    };

    #endregion

    #region Post Mission Settings

    private static readonly SettingEntry Post_GoldRemover = new()
    {
        Label = "金星完成时移除任务",
        Category = PostMissionCategory,
        Keywords = new[] { "Remove", "Mission", "Gold", "Completion", "A Rank", "Keep" },
        Draw = () =>
        {
            var removeGold = C.RemoveAfterGold;
            if (ImGui.Checkbox("金星完成时移除任务", ref removeGold))
            {
                C.RemoveAfterGold = removeGold;
                C.Save();
            }

            using (ImRaii.Disabled(!removeGold))
            {
                var keepARanks = C.KeepARanks;
                if (ImGui.Checkbox("保留“A 类任务”及以下任务", ref keepARanks))
                {
                    C.KeepARanks = keepARanks;
                    C.Save();
                }
            }
        }
    };

    #endregion

    #region Record Settings

    private static readonly SettingEntry Record_TimeHistory = new()
    {
        Label = "平均时间历史保留数",
        Category = RecordCategory,
        Keywords = new[] { "Time", "History", "Average", "Records", "Logs", "Limit" },
        Draw = () =>
        {
            var timeHistory = C.TimeHistoryLimit;
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("平均时间历史保留数", ref timeHistory))
            {
                C.TimeHistoryLimit = timeHistory;
                C.Save();
            }

            ImGui.SameLine();
            ImGui.TextDisabled("?");
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("小于 0：保留全部日志\n" +
                                 "大于 0：按设定数量保留");
            }
        }
    };

    #endregion

    #region Post Mission Commands

    private static readonly SettingEntry Post_Commands = new()
    {
        Label = "任务后命令",
        Category = PostCommandsCategory,
        Keywords = new[] { "Post", "Mission", "Commands", "Script", "Delay", "SND", "Macro" },
        Draw = DrawPostMissionCommands
    };

    private static void DrawPostMissionCommands()
    {
        ImGui.TextWrapped("请在下方输入希望在运行完成后自动执行的一系列命令。\n" +
                          "这算是我提供的一种方式，让您能够在一定程度上编写脚本/设置一系列您想做但插件本身可能未包含的其他操作。\n" +
                          "如果您需要更复杂的功能，直接编写一个 SND 脚本即可，然后让它在任务结束后自动执行。");

        if (ImGui.Button("添加新命令"))
        {
            C.PostMissionCommands.Add(new MissionCommand
            {
                command = "",
                Delay = 0,
            });
            C.Save();
        }

        if (!ImGui.BeginTable("Mission Commands", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            return;

        ImGui.TableSetupColumn("命令");
        ImGui.TableSetupColumn("延迟");
        ImGui.TableSetupColumn("移除");
        ImGui.TableHeadersRow();

        MissionCommand? toRemove = null;
        var entryCounter = 0;

        foreach (var entry in C.PostMissionCommands)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.SetNextItemWidth(200);

            ImGui.PushID($"{entryCounter}_MissionCommand");

            var command = entry.command;
            if (ImGui.InputText("##Command", ref command))
            {
                entry.command = command;
                C.SaveDebounced();
            }

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(100);
            var delay = entry.Delay;
            if (ImGui.InputInt("###Delay", ref delay))
            {
                entry.Delay = delay;
                C.SaveDebounced();
            }

            ImGui.TableNextColumn();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"remove{entryCounter}"))
                toRemove = entry;

            ImGui.PopID();
            entryCounter++;
        }

        if (toRemove != null)
        {
            C.PostMissionCommands.Remove(toRemove);
            C.Save();
        }

        ImGui.EndTable();
    }

    #endregion

    #region Dev Favorites

    private static readonly SettingEntry Fun_CrazyTaxi = Toggle(
        "寻路时显示疯狂出租车箭头", FunCategory,
        new[] { "Crazy Taxi", "Arrow", "Navmesh", "Fun" },
        () => C.CrazyTaxiArrow,
        v => C.CrazyTaxiArrow = v);

    private static readonly SettingEntry Fun_Placebo = new()
    {
        Label = "提升采集与制作速度",
        Category = FunCategory,
        Keywords = new[] { "Gathering", "Crafting", "Speed", "Placebo", "Fun" },
        Draw = () =>
        {
            var placebo = C.PlaceboCheckbox;
            if (ImGui.Checkbox("提升采集与制作速度", ref placebo))
            {
                C.PlaceboCheckbox = placebo;
                C.Save();
            }

            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "这完全没有任何作用\n" +
                "但我知道肯定有人会勾上它然后不看说明，所以这是个恶作剧。\n" +
                "不过还是感谢你使用我的插件，这对我意义重大 <3", false);
        }
    };

    private static readonly SettingEntry Fun_FakeFisher = new()
    {
        Label = "提升钓鱼速度",
        Category = FunCategory,
        Keywords = new[] { "Fishing", "Speed", "Clown", "Glamourer", "Fun", "Joke" },
        Draw = () =>
        {
            var fakeFishing = C.FakeIncreaseFisher;
            if (ImGui.Checkbox("提升钓鱼速度", ref fakeFishing))
            {
                C.FakeIncreaseFisher = fakeFishing;
                C.SaveDebounced();
            }

            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "提醒一下，这只会每隔一分钟左右通过 Glamourer 给你套一个傻瓜头。\n" +
                "100% 是个玩笑设置，别当真。我并没有实现这功能的技术", false);
        }
    };

    #endregion

    #region Notification Master IPC

    private static readonly SettingEntry Notification_Foreground = new()
    {
        Label = "Notification Master：前台",
        Category = NotificationCategory,
        Keywords = new[] { "Notification", "Master", "Notification Master", "Foreground" },
        Draw = () =>
        {
            var v = C.Notification_Foreground;
            if (ImGui.Checkbox("将游戏带到前台", ref v))
            {
                C.Notification_Foreground = v;
                C.Save();
            }
            if (!P.NotificationIPC.Installed)
            {
                ImGui.SameLine();
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle, "需要安装 Notification Master", false);
            }
        }
    };

    private static readonly SettingEntry Notification_Toast = new()
    {
        Label = "Notification Master：弹窗提示",
        Category = NotificationCategory,
        Keywords = new[] { "Notification", "Master", "Notification Master", "Toast", "Notification" },
        Draw = () =>
        {
            var v = C.Notification_Toast;
            if (ImGui.Checkbox("停止时发送弹窗提示", ref v))
            {
                C.Notification_Toast = v;
                C.Save();
            }
            if (!P.NotificationIPC.Installed)
            {
                ImGui.SameLine();
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle, "需要安装 Notification Master", false);
            }
        }
    };

    private static readonly SettingEntry Notification_Flash = new()
    {
        Label = "Notification Master：闪烁",
        Category = NotificationCategory,
        Keywords = new[] { "Notification", "Master", "Notification Master", "Flash", "Notification" },
        Draw = () =>
        {
            var v = C.Notification_FlashTaskbar;
            if (ImGui.Checkbox("闪烁任务栏图标", ref v))
            {
                C.Notification_FlashTaskbar = v;
                C.Save();
            }
            if (!P.NotificationIPC.Installed)
            {
                ImGui.SameLine();
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle, "需要安装 Notification Master", false);
            }
        }
    };

    #endregion
}
