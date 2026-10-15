using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Ui.MainUi;
using ICE.Ui.MainUi.HelpFolder;
using ICE.Ui.MainUi.ModeSelect_Modes;
using ICE.Ui.MainUi.Settings;
using ICE.Ui.MainUi.Settings.Settings_Misc;
using ICE.Ui.MainUi.Settings.Settings_Table;
using System.Collections.Generic;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace ICE.Ui
{
    internal class MainWindow : Window
    {
        public MainWindow() :
#if DEBUG
        base($"Ice's Cosmic Exploration {P.GetType().Assembly.GetName().Version} [Debug Build] ###ICEMainWindow2")
#else
        base($"Ice's Cosmic Exploration {P.GetType().Assembly.GetName().Version} ###ICEMainWindow2")
#endif
        {
            Flags = ImGuiWindowFlags.NoScrollbar;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(500, 500),
                MaximumSize = new Vector2(4000, 4000),
            };
            TitleBarButtons.Add(new() { ShowTooltip = () => ImGui.SetTooltip("♥ Ko-fi (请我喝杯冰咖啡)"), Icon = FontAwesomeIcon.Heart, IconOffset = new(1, 1), Click = _ => GenericHelpers.ShellStart("https://ko-fi.com/ice643269") });

            P.windowSystem.AddWindow(this);

            AllowPinning = true;
            AllowClickthrough = true;
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public override void Draw()
        {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 10).Push(ImGuiStyleVar.ChildBorderSize, 1);

            SelectableSidebar.Draw();

            ImGui.SameLine(0, 5);

            var windowSizeRemaining = ImGui.GetContentRegionAvail();
            using (var mainBody = ImRaii.Child("mainBody_WindowV3", windowSizeRemaining, true))
            {
                if (!mainBody.Success) return;
                MainBody();
            }
        }

        private static readonly Dictionary<WindowSelection, Action> SelectedView = new()
        {
            // Cosmic Helper
            [WindowSelection.MissionSetup] = () =>  Mission_Setup.Draw(),
            [WindowSelection.CosmicAgenda] = () => Cosmic_Agenda.Draw(),
            [WindowSelection.ExpeditionLogs] = () => Expedition_Log.Draw(),

            // Settings
            [WindowSelection.StopWhen] = () => StopWhen.Draw(),
            [WindowSelection.GatheringProfiles] = () => GatherSettings.Draw(),
            [WindowSelection.MissionPriority] = () => Priority_Settings.Draw(),
            [WindowSelection.CharacterSettings] = () => Character_Settings.Draw(),
            [WindowSelection.MiscSettings] = () => SettingsUi.Draw(),
            [WindowSelection.TravelSettings] = () => TravelSettings.Draw(),

            // Hub Activities
            [WindowSelection.CreditShopping] = () => Shop_Credits.Draw(),
            [WindowSelection.GambaShopping] = () => GambaWheel.Draw(),
            [WindowSelection.DroneShopping] = () => Shop_Dronebit.Draw(),
            [WindowSelection.MountShopping] = () => Shop_Tokens.Draw(),

            // Help Section
            [WindowSelection.Plugin_Install] = () => helpSelect_Required.Draw(),
            [WindowSelection.Plugin_Logs] = () => helpSelect_Logs.Draw_Helper(),
            [WindowSelection.Plugin_Tips] = () => helpSelect_Tips.Draw(),
        };

        private static void MainBody()
        {
            var selectedWindow = C.SelectedTab;

            if (SelectedView.TryGetValue(selectedWindow, out var drawAction))
            {
                drawAction();
            }
            else
            {
                ImGui.Text("嘿嘿");
            }
        }

        public static void ModeSelection()
        {
            bool standard = C.SelectedMode == ModeSelect.Standard;
            bool relicMode = C.SelectedMode == ModeSelect.RelicMode;
            bool xpLeveling = C.SelectedMode == ModeSelect.LevelMode;
            bool goldMode = C.SelectedMode == ModeSelect.MissionGoldMode;
            bool agendaMode = C.SelectedMode == ModeSelect.AgendaMode;

            ImGui.Text("选择模式");
            ImGui.Separator();

            if (ImGui.RadioButton("标准模式", standard))
            {
                C.SelectedMode = ModeSelect.Standard;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.Standard));
            if (ImGui.RadioButton("宇宙工具研究数据刷取模式", relicMode))
            {
                C.SelectedMode = ModeSelect.RelicMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.RelicMode));

            if (ImGui.RadioButton("练级模式", xpLeveling))
            {
                C.SelectedMode = ModeSelect.LevelMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.LevelMode));
            if (ImGui.RadioButton("金星完成模式", goldMode))
            {
                C.SelectedMode = ModeSelect.MissionGoldMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.MissionGoldMode));
            if (ImGui.RadioButton("日程模式", agendaMode))
            {
                C.SelectedMode = ModeSelect.AgendaMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.AgendaMode));
        }
        public static string HelpInfoText(ModeSelect mode)
        {
            return mode switch
            {
                ModeSelect.Standard =>
                    "标准模式\n" +
                    "-> 用于选择您想要刷取的任务。此模式按以下优先级顺序执行：\n" +
                    "-> 紧急探索任务 -> 临时性任务[连续/时间限定/天气限定任务] -> 标准任务[A->D]\n" +
                    "-> 选择你想要做的任务，然后开始吧。\n" +
                    "-> 如需更改优先级，请在 设置 -> 任务优先级 中调整",
                ModeSelect.LevelMode =>
                    "练级模式\n" +
                    "-> 根据您当前职业所处的等级区间，自动选择最适合升级的任务\n" +
                    "-> 这些任务由我手动挑选，依据完成所需时间决定\n" +
                    "-> 能工巧匠职业会优先选择制作进展需求最低的任务\n" +
                    "-> 大地使者职业会优先选择在最低技能需求下最不折磨的任务\n" +
                    "**启用这些模式时会自动临时调整相关设置**",
                ModeSelect.RelicMode =>
                    "宇宙工具研究数据刷取模式\n" +
                    "-> 自动选择最适合完成宇宙工具的任务\n" +
                    "-> 任务选择的权重依据为完成宇宙工具到下一阶段所需的研究数据\n" +
                    "-> 如果您只想做特定任务，可以启用此选项并选择您要做的任务",
                ModeSelect.AgendaMode =>
                    "日程模式\n" +
                    "此模式用于按特定顺序执行一系列操作。例如，你想连续刷完所有职业的宇宙工具\n" +
                    "或者你想先做裁衣的宇宙工具 -> 再在园艺上刷技巧点 -> 再在锻铁上刷信用点\n" +
                    "本质上就是“我想按这个顺序做事”的那种模式。\n" +
                    "注意：如果你一直开着它导致被封号，我概不负责。我个人不赞成挂机离开电脑，但总有人在盯着。请牢记这一点",
                ModeSelect.MissionGoldMode =>
                    "金星完成模式\n" +
                    "-> 会自动挑选所有你目前尚未拿到金星评价的任务，且仅限这些任务。\n" +
                    "-> 如果它属于某个连续任务链，会自动抓取完成它所需的前置任务，必要时也会抓取后续任务\n" +
                    "-> 如果没有可重抽的任务，它会不断切换页签，直到任务可用（通过临时性任务或紧急探索任务）\n" +
                    "**它会尊重你关于是否刷非本职业临时性任务与紧急探索任务的设置",
                _ => "???? 不知为何这里缺失了此项。请向 Ice 报告"
            };
        }
    }
}
