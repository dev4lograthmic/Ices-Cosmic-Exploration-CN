using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;

namespace ICE.Ui.MainUi.Settings;

public static class Settings_TableColumns
{
    private static string[] missionSortOptions = 
        ["ID", "任务名称", "宇宙信用点", "行星信用点", 
        "研究数据 I", "研究数据 II", "研究数据 III", "研究数据 IV", "研究数据 V", 
        "地图位置", "职业技巧点", "职业经验值"];

    public static void ColumnSettings()
    {
        int missionSelectedOption = C.TableSortOption;
        if (ImGui.BeginCombo("排序方式", missionSortOptions[missionSelectedOption]))
        {
            for (int i = 0; i < missionSortOptions.Length; i++)
            {
                bool isSelected = (i == missionSelectedOption);
                if (ImGui.Selectable(missionSortOptions[i], isSelected))
                {
                    missionSelectedOption = i;
                }
                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
                if (missionSelectedOption != C.TableSortOption)
                {
                    C.TableSortOption = missionSelectedOption;
                    C.Save();
                }
            }
            ImGui.EndCombo();
        }

        bool hideUnsupported = C.HideUnsupportedMissions;
        if (ImGui.Checkbox("隐藏不支持的任务", ref hideUnsupported))
        {
            C.HideUnsupportedMissions = hideUnsupported;
            C.Save();
        }

        bool autoShowToken = C.Auto_ShowTokens;
        if (ImGui.Checkbox("自动显示/隐藏行星票据", ref autoShowToken))
        {
            C.Auto_ShowTokens = autoShowToken;
            C.Save();
        }

        ImGuiEx.HelpMarker("只在您打算亲自完成任务，而不是依靠插件自动化完成时，才需要启用此选项。" +
                           "另外，如果您使用其他插件来处理汇报、制作、采集等自动化操作，并且不希望 I.C.E. 与这些插件交互，也可以启用此选项");
    }

    private static bool ApplyToAllClasses = true;
    private static bool ApplyToSpecicClass = false;
    private static int SpecificClass = 8;
    private static int selectedClassIndex = 0;

    private static readonly string[] classOptions = new[]
    {
        "刻木匠 (CRP)",      // 0
        "锻铁匠 (BSM)",     // 1
        "铸甲匠 (ARM)",        // 2
        "雕金匠 (GSM)",      // 3
        "制革匠 (LTW)",  // 4
        "裁衣匠 (WVR)",         // 5
        "炼金术士 (ALC)",      // 6
        "烹调师 (CUL)",     // 7
        "采矿工 (MIN)",          // 8
        "园艺工 (BTN)",       // 9
        "捕鱼人 (FSH)"          // 10
    };

    private static readonly int[] classIds = new[]
    {
        8,  // Carpenter
        9,  // Blacksmith
        10, // Armorer
        11, // Goldsmith
        12, // Leatherworker
        13, // Weaver
        14, // Alchemist
        15, // Culinarian
        16, // Miner
        17, // Botanist
        18  // Fisher
    };

    private static TurninState HighestTurnin = TurninState.Gold;

    public static void GeneralMissionSettings()
    {
        if (ImGui.Button("快捷汇报模式应用"))
        {
            ImGui.OpenPopup("Quick Apply_Mission Turnins");
        }

        if (ImGui.BeginPopup("Quick Apply_Mission Turnins"))
        {
            if (ImGui.RadioButton("应用到全部职业", ApplyToAllClasses))
            {
                ApplyToAllClasses = true;
                ApplyToSpecicClass = false;
            }

            if (ImGui.RadioButton("应用到指定职业", ApplyToSpecicClass))
            {
                ApplyToAllClasses = false;
                ApplyToSpecicClass = true;
            }
            if (ImGui.Combo("##ClassSelector", ref selectedClassIndex, classOptions, classOptions.Length))
            {
                // Update SpecificClass when selection changes
                SpecificClass = classIds[selectedClassIndex];
                IceLogging.Debug($"Selected class: {classOptions[selectedClassIndex]}, ID: {SpecificClass}");
            }
            ImGui.Separator();
            ImGui.Text("选择汇报选项");
            ImGui.Dummy(new Vector2(0, 2));

            if (ImGui.RadioButton("金星", HighestTurnin is TurninState.Gold))
            {
                HighestTurnin = TurninState.Gold;
            }
            if (ImGui.RadioButton("银星", HighestTurnin is TurninState.Silver))
            {
                HighestTurnin = TurninState.Silver;
            }
            if (ImGui.RadioButton("铜星", HighestTurnin is TurninState.Bronze))
            {
                HighestTurnin = TurninState.Bronze;
            }

            ImGui.Separator();

            if (ImGui.Button("应用"))
            {
                var amountApplied = 0;
                foreach (var mission in C.MissionConfig)
                {
                    if (CosmicHelper.SheetMissionDict.TryGetValue(mission.Key, out var sheetInfo))
                    {
                        if (ApplyToSpecicClass && !sheetInfo.Jobs.Contains((uint)SpecificClass))
                            continue;

                        if (sheetInfo.Attributes.HasFlag(MissionAttributes.Score_TimeRemaining))
                            continue;

                        if (C.MissionConfig.TryGetValue(mission.Key, out var config))
                        {
                            config.TurninGoal = HighestTurnin;
                        }
                        amountApplied += 1;
                    }
                }
                C.SaveDebounced();

                Notify.Success($"已应用设置到 {amountApplied} 个任务，专门为你做的，兄弟。");
                ImGui.CloseCurrentPopup();
            }


            ImGui.EndPopup();
        }
    }
}
