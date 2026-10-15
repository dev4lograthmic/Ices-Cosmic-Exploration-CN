using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;
using ICE.Ui.MainUi.Settings;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;

namespace ICE.Ui.MainUi.ModeSelect_Modes
{
    internal class Mission_Setup
    {
        private static readonly Dictionary<string, uint> BattleJobs = new()
        {
            // Tanks
            { "Paladin", 19 },
            { "Warrior", 21 },
            { "Dark Knight", 32 },
            { "Gunbreaker", 37 },
    
            // Healers
            { "White Mage", 24 },
            { "Scholar", 28 },
            { "Astrologian", 33 },
            { "Sage", 40 },
    
            // Melee DPS
            { "Monk", 20 },
            { "Dragoon", 22 },
            { "Ninja", 30 },
            { "Samurai", 34 },
            { "Reaper", 39 },
            { "Viper", 41 },
    
            // Physical Ranged DPS
            { "Bard", 23 },
            { "Machinist", 31 },
            { "Dancer", 38 },
    
            // Magical Ranged DPS
            { "Black Mage", 25 },
            { "Summoner", 27 },
            { "Red Mage", 35 },
            { "Pictomancer", 42 }
        };

        public static CosmicTables.Mission_Table? MissionTable;
        private static List<CosmicHelper.MissionInfo> TableItems = [];
        private static int ItemCount = 0;
        private static string newListName = string.Empty;

        public static void Draw()
        {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 10).Push(ImGuiStyleVar.ChildBorderSize, 1);

            // Header at the top
            float scale = ImGuiHelpers.GlobalScale;

            using (var headerChild = ImRaii.Child("##modeSelect_StandardHeader", new Vector2(0, 45 * scale), true, ImGuiWindowFlags.NoScrollbar))
            {
                if (!headerChild.Success) return;

                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10 * scale);
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5 * scale);

                string modeType = string.Empty;
                FontAwesomeIcon modeIcon = FontAwesomeIcon.List;

                bool standard = C.SelectedMode == ModeSelect.Standard;
                bool relicMode = C.SelectedMode == ModeSelect.RelicMode;
                bool xpLeveling = C.SelectedMode == ModeSelect.LevelMode;
                bool goldMode = C.SelectedMode == ModeSelect.MissionGoldMode;
                bool agendaMode = C.SelectedMode == ModeSelect.AgendaMode;


                if (standard)
                    modeType = "标准模式";
                else if (relicMode)
                {
                    modeType = "宇宙工具研究数据刷取模式";
                    modeIcon = FontAwesomeIcon.ArrowUpRightDots;
                }
                else if (xpLeveling)
                {
                    modeType = "练级模式";
                    modeIcon = FontAwesomeIcon.Leaf;
                }
                else if (goldMode)
                {
                    modeType = "金星完成模式";
                    modeIcon = FontAwesomeIcon.Trophy;
                }
                else if (agendaMode)
                {
                    modeType = "日程模式";
                    modeIcon = FontAwesomeIcon.ClipboardList;
                }

                ImGuiEx.IconWithText(modeIcon, $"{modeType}");

                ImGui.SameLine(0, 10 * scale);

                // Adjust the Y position to center the button vertically with the text
                float textHeight = ImGui.GetTextLineHeight();
                float buttonHeight = ImGui.GetFrameHeight();
                float yOffset = (textHeight - buttonHeight) / 2f;
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, "模式选择"))
                {
                    ImGui.OpenPopup("Mode Select | Select Mode Window");
                }
                if (ImGui.BeginPopup("Mode Select | Select Mode Window"))
                {
                    MainWindow.ModeSelection();

                    ImGui.EndPopup();
                }

                uint currentJobId = (uint)Player.Job;
                bool usingSupportedJob = CosmicHelper.CrafterJobList.Contains(currentJobId) || CosmicHelper.GatheringJobList.Contains(currentJobId);

                bool AnyStop = C.StopOnceHitCosmicScore
                             | C.StopWhenLevel
                            || C.StopOnceHitCosmoCredits
                            || C.StopOnceHitLunarCredits
                            || C.StopOnceRelicFinished
                            || C.StopOnceStandardMissionsGolded;
                if (AnyStop)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();

                        ImGui.Text("看来你启用了以下某一项停止条件");
                        if (C.StopOnceHitCosmicScore)
                            ImGui.BulletText($"技巧点达到阈值时停止 [{C.CosmicScoreCap:N0}]");
                        if (C.StopWhenLevel)
                            ImGui.BulletText($"等级达到阈值时停止 [{C.TargetLevel:N0}]");
                        if (C.StopOnceHitCosmoCredits)
                            ImGui.BulletText($"宇宙信用点达到阈值时停止 [{C.CosmoCreditsCap:N0}]");
                        if (C.StopOnceHitLunarCredits)
                            ImGui.BulletText($"行星信用点达到阈值时停止 [{C.LunarCreditsCap:N0}]");
                        if (C.StopOnceRelicFinished)
                            ImGui.BulletText($"宇宙工具可报告时停止");
                        if (C.StopOnceStandardMissionsGolded)
                            ImGui.BulletText("所有标准任务达成金星评价时停止");

                        ImGui.Text("所以如果你停止后不清楚原因……可能就是这个");

                        ImGui.EndTooltip();
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                bool unsupportedArtisan = false; // xpLeveling && CosmicHelper.CrafterJobList.Contains((uint)Player.Job);
                bool unsupportedMoon = xpLeveling 
                    && CosmicMoonRegistry.TryGetMoon(Player.Territory.RowId, out var currentMoon)
                    && !CosmicMoonRegistry.HasLevelingContent(currentMoon);

                // Leveling on a hub requires QuickLevelList entries; gathering still needs route YAML per territory
                using (ImRaii.Disabled(SchedulerMain.State != IceState.Idle || !usingSupportedJob || unsupportedMoon))
                {
                    if (ImGui.Button("开始", new Vector2(150 * scale, 0)))
                    {
                        SchedulerMain.EnablePlugin();
                    }
                }

                if (unsupportedArtisan)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("嘿！使用此模式需要更新 Artisan，请至少更新到：");
                        ImGui.Text("4.0.4.29");
                        ImGui.EndTooltip();
                    }
                }
                else if (unsupportedMoon && CosmicMoonRegistry.TryGetMoon(Player.Territory.RowId, out var unsupportedHub))
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"嘿！{unsupportedHub.DisplayName} 暂不支持练级。");
                        var missing = new List<string>();
                        if (!CosmicMoonRegistry.HasLevelingContent(unsupportedHub))
                            missing.Add("QuickLevelList 任务");
                        if (!CosmicMoonContent.HasGatheringRoutes(unsupportedHub.TerritoryId))
                            missing.Add("采集路线");
                        if (missing.Count > 0)
                            ImGui.Text($"仍需：{string.Join("、", missing)}。");
                        ImGui.EndTooltip();
                    }
                }
                if (!P.AutoHook.UpdatedPlugin() && CosmicMoonRegistry.Auxesia.TerritoryId == Player.Territory.RowId)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"嘿！你当前的 AutoHook 版本暂不支持此地图");
                        ImGui.Text($"（目前）你需要使用测试版才能在此自动钓鱼");
                        ImGui.Text($"如果你仍尝试运行且它选中了钓鱼任务，还会再弹出一个警告……");
                        ImGui.EndTooltip();
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                using (ImRaii.Disabled(SchedulerMain.State == IceState.Idle))
                {
                    using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f)))
                    using (ImRaii.PushColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.3f, 0.3f, 1.0f)))
                    using (ImRaii.PushColor(ImGuiCol.ButtonActive, new Vector4(0.7f, 0.1f, 0.1f, 1.0f)))
                    {
                        if (ImGui.Button("停止", new Vector2(150 * scale, 0)))
                        {
                            SchedulerMain.DisablePlugin();
                        }
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                if (ImGui.Button("任务设置"))
                {
                    ImGui.OpenPopup("Mission Settings: Popup");
                }
                if (ImGui.BeginPopup("Mission Settings: Popup"))
                {
                    // TODO: Mission Settings
                    bool grindAllProvisionals = C.GrindAllProvisionals;
                    if (ImGui.Checkbox("允许执行所有临时性任务", ref grindAllProvisionals))
                    {
                        C.GrindAllProvisionals = grindAllProvisionals;
                        C.Save();
                    }
                    ImGuiEx.HelpMarker("启用后，将显示所有可刷取的 天气限定/时间限定/连续 任务。\n" +
                                       "这些任务会在当前选定职业的普通任务之外额外显示。\n" +
                                       "如果您只想专注于一个职业进行任务，请禁用此选项。");

                    bool allowCriticalsAllClass = C.GrindOffClassRedAlert;
                    if (ImGui.Checkbox("紧急探索任务：允许所有职业", ref allowCriticalsAllClass))
                    {
                        C.GrindOffClassRedAlert = allowCriticalsAllClass;
                        C.Save();
                    }
                    ImGuiEx.HelpMarker($"这将允许你为紧急探索任务刷取其它职业。\n" +
                        $"（例如你当前是刻木匠，但弹出了锻铁匠的紧急探索任务）");

                    bool removeGold = C.RemoveAfterGold;
                    if (ImGui.Checkbox("金星完成时移除任务", ref removeGold))
                    {
                        C.RemoveAfterGold = removeGold;
                        C.Save();
                    }
                    using (ImRaii.Disabled(!removeGold))
                    {
                        bool keepARanks = C.KeepARanks;
                        if (ImGui.Checkbox("保留“A 类任务”及以下任务", ref keepARanks))
                        {
                            C.KeepARanks = keepARanks;
                            C.Save();
                        }
                    }

                    ImGui.Checkbox("当前任务结束后停止", ref Mission_Settings.StopAfterCurrent);
                    bool relicTurnin = C.TurninRelic;
                    if (ImGui.Checkbox("宇宙工具可报告时提交##RelicTurnin_GeneralSetting", ref relicTurnin))
                    {
                        C.TurninRelic = relicTurnin;
                        C.Save();
                    }
                    ImGui.SameLine();
                    ImGui.TextDisabled("?");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("这是此功能的工作方式说明。如果我未来修改了这个功能，这个提示也会随之改变。\n" +
                                         "1：此功能将检查你的当前职业[不是菜单中选择的职业，是实际当前职业]提交宇宙工具。\n" +
                                         "2：此功能的优先级高于“宇宙工具可报告时停止”选项，如果两个都启用，它会选择提交宇宙工具而不是停止，并继续执行任务。\n" +
                                         "3：如果你当前是能工巧匠职业，报告后将前往设置好的制作返回点位置。\n" +
                                         "\t- 这是可选的，你可以自由关闭。我个人喜欢开着，方便我回到自己选定的安静区域。");
                    }

                    ImGui.Separator();
                    bool relic_AllowRedAlert = C.Relic_IncludeCriticals;
                    if (ImGui.Checkbox("宇宙工具模式：允许紧急探索任务", ref relic_AllowRedAlert))
                    {
                        C.Relic_IncludeCriticals = relic_AllowRedAlert;
                        C.Save();
                    }

                    bool OnlySelected = C.XPRelicOnlyEnabled;
                    if (ImGui.Checkbox("仅限启用的任务", ref OnlySelected))
                    {
                        C.XPRelicOnlyEnabled = OnlySelected;
                        C.Save();
                    }
                    if (ImGui.Button("宇宙工具职业切换设置"))
                    {
                        C.SelectedTab = WindowSelection.CharacterSettings;
                    }

                    if (ImGui.Button("保存当前任务为预设"))
                    {
                        ImGui.OpenPopup("Preset Save Editor");
                    }

                    if (ImGui.BeginPopup("Preset Save Editor"))
                    {
                        ImGui.InputText($"预设名称##PlaylistName", ref newListName);
                        using (ImRaii.Disabled(string.IsNullOrEmpty(newListName)))
                        {
                            if (ImGui.Button("保存"))
                            {
                                List<uint> new_Playlist = new();
                                foreach (var mission in C.MissionConfig.Where(x => x.Value.Enabled))
                                {
                                    new_Playlist.Add(mission.Key);
                                }
                                if (C.Mission_Playlist.ContainsKey(newListName))
                                {
                                    C.Mission_Playlist[newListName] = new_Playlist;
                                }
                                else
                                {
                                    C.Mission_Playlist.Add(newListName, new_Playlist);
                                }
                                C.Save();
                                ImGui.CloseCurrentPopup();
                            }
                        }

                        ImGui.EndPopup();
                    }

                    if (C.Mission_Playlist.Count > 0)
                    {
                        if (ImGui.Button("查看所有预设"))
                        {
                            ImGui.OpenPopup("Preset: List Viewer");
                        }

                        if (ImGui.BeginPopup("Preset: List Viewer"))
                        {
                            ImGui.Text($"加载任务预设");

                            if (ImGui.BeginTable($"Preset: TableViewer", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                            {
                                ImGui.TableSetupColumn("名称");
                                ImGui.TableSetupColumn("启用任务数量");

                                ImGui.TableHeadersRow();

                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.AlignTextToFramePadding();
                                ImGui.Text($"全部清空");
                                ImGui.SameLine();
                                if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUpRightFromSquare, $"FreshPreset_Button"))
                                {
                                    foreach (var mission in C.MissionConfig)
                                    {
                                        mission.Value.Enabled = false;
                                    }
                                    C.Save();
                                    ImGui.CloseCurrentPopup();
                                }

                                foreach (var item in C.Mission_Playlist)
                                {
                                    ImGui.TableNextRow();
                                    ImGui.TableSetColumnIndex(0);
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{item.Key}");
                                    ImGui.SameLine();
                                    if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUpRightFromSquare, $"{item.Key}_Button"))
                                    {
                                        foreach (var mission in C.MissionConfig)
                                        {
                                            if (item.Value.Contains(mission.Key))
                                                mission.Value.Enabled = true;
                                            else
                                                mission.Value.Enabled = false;
                                        }
                                        C.Save();
                                        ImGui.CloseCurrentPopup();
                                    }
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip("导入任务");
                                    }

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{item.Value.Count}");

                                    ImGui.TableNextColumn();
                                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"{item.Key}_Remove"))
                                    {
                                        C.Mission_Playlist.Remove(item);
                                        C.Save();
                                    }
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip("移除");
                                    }
                                }

                                ImGui.EndTable();
                            }

                            ImGui.EndPopup();
                        }
                    }


                ImGui.EndPopup();
                }
            }

            using (var bodyChild = ImRaii.Child("##modeSelect_Body", new Vector2(0, -1), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (!bodyChild.Success) return;

                float scrollbarSize = ImGui.GetStyle().ScrollbarSize;
                float buttonRowHeight = (ImGui.GetTextLineHeight() + 8 * scale + 4 * scale) + scrollbarSize;

                using (var missionButtons = ImRaii.Child("##tab_scroll", new Vector2(0, buttonRowHeight), false, ImGuiWindowFlags.HorizontalScrollbar))
                {
                    if (!missionButtons.Success)
                        return;

                    ImGui_Ice.DrawRankButton("紧急探索任务", MissionFilter.RedAlert, MissionTable);
                    ImGui_Ice.DrawRankButton("连续任务", MissionFilter.Sequence, MissionTable);
                    ImGui_Ice.DrawRankButton("天气限定任务", MissionFilter.Weather, MissionTable);
                    ImGui_Ice.DrawRankButton("时间限定任务", MissionFilter.Timed, MissionTable);
                    ImGui_Ice.DrawRankButton("超难任务", MissionFilter.Master, MissionTable);
                    ImGui_Ice.DrawRankButton("A 类任务", MissionFilter.ARank, MissionTable);
                    ImGui_Ice.DrawRankButton("B 类任务", MissionFilter.BRank, MissionTable);
                    ImGui_Ice.DrawRankButton("C 类任务", MissionFilter.CRank, MissionTable);
                    ImGui_Ice.DrawRankButton("D 类任务", MissionFilter.DRank, MissionTable);

                    ImGui_Ice.EndCategoryButtonRow();
                }

                var bottomSpace = ImGui.GetTextLineHeight() + 6f;
                bottomSpace += 12f; // prevent the tabs from creating a scrollbar

                Vector2 size = new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y - bottomSpace);
                if (ImGui.BeginChild("###MissionTableV3", size, false))
                {
                    try
                    {
                        if (MissionTable == null && CosmicHelper.SheetMissionDict.Count > 0)
                        {
                            foreach (var mission in CosmicHelper.SheetMissionDict)
                            {
                                CosmicHelper.MissionInfo missionDetails = new() { Id = mission.Key };
                                TableItems.Add(missionDetails);
                            }
                            ItemCount = TableItems.Count();
                            MissionTable = new(TableItems);
                        }
                        var filterActive = MissionTable.FilteredItems.Count != 0 && MissionTable.FilteredItems.Count != ItemCount;
                        var filterCount = filterActive ? $" (of {ItemCount})" : "";
                        var height = ImGui.GetFrameHeight();
                        MissionTable.Draw(height + 4f);
                    }
                    catch (Exception ex)
                    {
                        IceLogging.Error(ex.Message, "Drawing Mission Table");
                    }
                }
                ImGui.EndChild();
            }
        }
    }
}
