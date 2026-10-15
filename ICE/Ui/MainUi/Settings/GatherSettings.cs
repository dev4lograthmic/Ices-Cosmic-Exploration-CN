using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.ImGuiTools;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings
{
    internal class GatherSettings
    {
        public enum MissionKinds
        {
            LimitedNodes,
            GatherX,
            TimeAttack,
            Chain_Scoring,
            Boon_Scoring,
            Chain_Boon,
            DualClass,
            GreaterReach_GatherX,
            GreaterReach_Boon,
            GreaterReach_Chain,
            GreaterReach_Boon_Chain,
        }

        private static string newProfileName = "";
        private static string[] MissionTypes = 
        [
            "限量采集点", 
            "采集 X 个物品", 
            "时间竞速", 
            "连锁冲分", 
            "恩惠冲分", 
            "连锁 + 恩惠冲分", 
            "双职业",
            "远达 [采集 X]",
            "远达 [恩惠]",
            "远达 [连锁]",
            "远达 [恩惠 + 连锁]",
        ];
        private static readonly string[] RankLabels = ["全部任务", "D 级及以上", "C 级及以上", "B 级及以上", "A 级及以上", "EX 级及以上", "仅 EX+"];
        private static MissionKinds _selectedMission = MissionKinds.LimitedNodes;

        private static readonly string PROFILE_PREFIX = "IceGatherProfile_";

        public static string ExportGatherProfile(int profileId)
        {
            if (!C.GatherProfiles.TryGetValue(profileId, out var profile))
                return string.Empty;

            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            var bytes = Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);

            return PROFILE_PREFIX + base64;
        }
        public static bool ImportGatherProfile(string importString, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Check for and remove the prefix
                if (!importString.StartsWith(PROFILE_PREFIX))
                {
                    errorMessage = "无效的导入字符串：缺少前缀";
                    return false;
                }

                var base64String = importString.Substring(PROFILE_PREFIX.Length);

                var bytes = Convert.FromBase64String(base64String);
                var json = Encoding.UTF8.GetString(bytes);

                var profile = JsonSerializer.Deserialize<GatherProfile>(json);
                if (profile == null)
                {
                    errorMessage = "反序列化配置失败";
                    return false;
                }

                // Get the next available ID
                int nextId = C.GatherProfiles.Keys.Count > 0
                    ? C.GatherProfiles.Keys.Max() + 1
                    : 0;

                profile.Id = nextId;
                C.GatherProfiles[nextId] = profile;

                // Save the configuration
                C.Save();

                return true;
            }
            catch (FormatException)
            {
                errorMessage = "无效的导入字符串：不是合法的 base64 格式";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"导入失败: {ex.Message}";
                return false;
            }
        }
        public static bool InitialSetupProfile(string importString, MissionKinds type, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                if (!importString.StartsWith(PROFILE_PREFIX))
                {
                    errorMessage = "无效的导入字符串：缺少前缀";
                    return false;
                }

                var base64String = importString.Substring(PROFILE_PREFIX.Length);
                var bytes = Convert.FromBase64String(base64String);
                var json = Encoding.UTF8.GetString(bytes);

                var profile = JsonSerializer.Deserialize<GatherProfile>(json);
                if (profile == null)
                {
                    errorMessage = "反序列化配置失败";
                    return false;
                }

                int nextId = C.GatherProfiles.Keys.Count > 0
                    ? C.GatherProfiles.Keys.Max() + 1
                    : 0;

                profile.Id = nextId;
                C.GatherProfiles[nextId] = profile;

                foreach (var mission in C.MissionConfig)
                {
                    if (!CosmicHelper.SheetMissionDict.TryGetValue(mission.Key, out var missionDict))
                        continue;

                    var attrs = missionDict.Attributes;
                    if (!attrs.HasFlag(MissionAttributes.Gather))
                        continue;
                    if (attrs.HasFlag(MissionAttributes.Collectables) || attrs.HasFlag(MissionAttributes.ReducedItems))
                        continue;

                    if (GetMissionKind(attrs) == type)
                        mission.Value.GProfileId = nextId;
                }

                return true;
            }
            catch (FormatException)
            {
                errorMessage = "无效的导入字符串：不是合法的 base64 格式";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"导入失败: {ex.Message}";
                return false;
            }
        }
        private static Dictionary<uint, string> Foods = new();

        public static void Draw()
        {
            int maxGp = 1200;

            bool SelfSpiritbondGather = C.SelfSpiritbondGather;
            if (ImGui.Checkbox("采集时精制魔晶石", ref SelfSpiritbondGather))
            {
                if (C.SelfSpiritbondGather != SelfSpiritbondGather)
                {
                    C.SelfSpiritbondGather = SelfSpiritbondGather;
                    C.Save();
                }
            }
            ImGuiEx.HelpMarker("启用此选项后，Pandora 的强心剂功能将不会被自动暂停。");

            bool AutoCordial = C.AutoCordial;
            if (ImGui.Checkbox("自动强心剂", ref AutoCordial))
            {
                C.AutoCordial = AutoCordial;
                C.Save();
            }
            ImGuiEx.HelpMarker("仅在 ICE 运行时生效，手动模式无效\n" +
                               "在宇宙探索地图中将暂停 Pandora 插件的自动强心剂功能");
            if (ImGui.CollapsingHeader("强心剂设置"))
            {
                int cordialMinRank = C.CordialMinRank;
                ImGui.SetNextItemWidth(150);
                if (ImGui.Combo("使用强心剂的最低任务等级", ref cordialMinRank, RankLabels, RankLabels.Length))
                {
                    C.CordialMinRank = cordialMinRank;
                    C.Save();
                }

                bool InverseCordialPrio = C.inverseCordialPrio;
                bool PreventOvercap = C.PreventOvercap;
                int CordialMinGp = C.CordialMinGp;

                if (ImGui.Checkbox("反转优先级（轻型 -> 普通 -> 高级）", ref InverseCordialPrio))
                {
                    C.inverseCordialPrio = InverseCordialPrio;
                    C.Save();
                }
                if (ImGui.Checkbox("防止溢出", ref PreventOvercap))
                {
                    C.PreventOvercap = PreventOvercap;
                    C.Save();
                }
                ImGui.SetNextItemWidth(200);
                if (ImGui.SliderInt("GP 低于设定值时使用强心剂", ref CordialMinGp, 0, maxGp))
                {
                    C.CordialMinGp = CordialMinGp;
                    C.SaveDebounced();
                }
                ImGui.SameLine();
                ImGuiEx.HelpMarker("此值为在使用强心剂之前，最低所需的 GP（采集力）。\n" +
                                   "如果设置为 0，即使启用了此功能也不会使用强心剂（因为……您的 GP（采集力）永远不会为 0）");
            }

            if (ImGui.CollapsingHeader("食物设置"))
            {
                int foodMinRank = C.FoodMinRank;
                ImGui.SetNextItemWidth(150);
                if (ImGui.Combo("使用食物的最低任务等级", ref foodMinRank, RankLabels, RankLabels.Length))
                {
                    C.FoodMinRank = foodMinRank;
                    C.Save();
                }

                bool useFood = C.UseGatheringFood;
                if (ImGui.Checkbox("在采集任务中使用食物", ref useFood))
                {
                    C.UseGatheringFood = useFood;
                    C.Save();
                }

                if (ImGui.Button("选择采集食物"))
                {
                    foreach (var item in ConsumableInfo.GatherFood)
                    {
                        if (PlayerHelper.GetItemCount(item.Id, out var count) && count > 0)
                            Foods[item.Id] = item.Name;
                    }

                    ImGui.OpenPopup("Food Selection");
                }
                ImGui.SameLine();
                if (C.GatheringFood == 0)
                {
                    ImGui.Text("未选择食物");
                }
                else
                {
                    var itemName = Svc.Data.GetExcelSheet<Item>().Where(x => x.RowId == C.GatheringFood).FirstOrDefault().Name.ToString();
                    ImGui.Text($"{itemName}");
                }

                if (ImGui.BeginPopup("Food Selection"))
                {
                    if (ImGui.BeginTable("Food Item Selection", 2, ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("食物");
                        ImGui.TableSetupColumn("数量");

                        // First Column, pretty much giving an option for "None" if they want none
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (ImGui.Selectable("不使用采集食物"))
                        {
                            C.GatheringFood = 0;
                            C.Save();
                            ImGui.CloseCurrentPopup();
                        }

                        foreach (var item in Foods)
                        {
                            ImGui.TableNextRow();
                            ImGui.PushID(item.Key);

                            ImGui.TableSetColumnIndex(0);
                            if (ImGui.Selectable($"{item.Value}"))
                            {
                                C.GatheringFood = item.Key;
                                C.Save();

                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.TableNextColumn();
                            PlayerHelper.GetItemCount(item.Key, out var count);
                            if (ImGui.Selectable($"x {count}"))
                            {
                                C.GatheringFood = item.Key;
                                C.Save();

                                ImGui.CloseCurrentPopup();
                            }
                        }

                        ImGui.EndTable();
                    }

                    ImGui.EndPopup();
                }
            }

            bool selfGather = C.Gather_NoNav;
            if (ImGui.Checkbox("禁用采集点之间的寻路", ref selfGather))
            {
                C.Gather_NoNav = selfGather;
                C.SaveDebounced();
            }
            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "在任务进行中禁用采集点之间的寻路\n" +
                "但仍允许在任务之间自动使用技能/采集动作/精炼\n" +
                "这是非常初期的测试版，可能会有问题\n" +
                "我以克苏鲁之名起誓，如果你启用了它然后来问“为什么没用”" +
                "你就会被暗影领域放逐");

            ImGui.Separator();

            if (ImGui.BeginTable("Gathering Profile Settings", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("配置选择");
                ImGui.TableSetupColumn("采集设置");

                // 1st Row, technically only really used for the gather profile name creator
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.SetNextItemWidth(200);
                ImGui.InputText("新配置名称", ref newProfileName, 64);
                using (ImRaii.Disabled(newProfileName == ""))
                {
                    if (ImGui.Button("添加配置") && !string.IsNullOrWhiteSpace(newProfileName))
                    {
                        var newId = C.GatherProfiles.Keys.Max() + 1;
                        C.GatherProfiles[newId] = new()
                        {
                            Name = newProfileName,
                        };
                        C.Save();
                        newProfileName = "";
                    }
                }

                // 2nd Row, Actually profile selector
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                #region Profile Selection

                ImGui.Text("采集配置");

                bool canDelete = C.GatherProfiles.Count > 1 && C.SelectedGatherIndex != 0;
                using (ImRaii.Disabled(!canDelete))
                {
                    if (ImGui.Button("删除选中的配置"))
                    {
                        int deletedId = C.SelectedGatherIndex;

                        // Don't allow deleting the default profile
                        if (deletedId == 0)
                        {
                            return;
                        }

                        // Remove the profile
                        C.GatherProfiles.Remove(deletedId);

                        // Update all missions using this GatherSettingId
                        foreach (var mission in C.MissionConfig)
                        {
                            if (mission.Value.GProfileId == deletedId)
                            {
                                mission.Value.GProfileId = 0; // fallback to default
                            }
                        }

                        // Clamp the selected index and save
                        C.SelectedGatherIndex = 0;
                        C.Save();
                    }
                }

                if (ImGui.BeginChild("GatherProfileChild", new Vector2(300, ImGui.GetTextLineHeightWithSpacing() * 5 + 10), true))
                {
                    foreach (var profile in C.GatherProfiles)
                    {
                        var id = profile.Key;
                        bool isSelected = C.SelectedGatherIndex == id;
                        if (ImGui.Selectable($"{profile.Value.Name}##{profile.Value.Name}_{id}", isSelected))
                        {
                            C.SelectedGatherIndex = id;
                            C.Save();
                        }

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndChild();

                if (!C.GatherProfiles.TryGetValue(C.SelectedGatherIndex, out var entry))
                {
                    // We've somehow gotten a variable that is outside the normal index, so going to just reset it back to 0
                    C.SelectedGatherIndex = 0;
                    C.SaveDebounced();
                }

                var missionIndex = (int)_selectedMission;
                if (ImGui.Combo("任务类型", ref missionIndex, MissionTypes, MissionTypes.Length))
                    _selectedMission = (MissionKinds)missionIndex;
                if (ImGui.Button("应用到任务类型"))
                {
                    foreach (var mission in C.MissionConfig)
                    {
                        if (!CosmicHelper.SheetMissionDict.TryGetValue(mission.Key, out var missionDict))
                            continue;

                        var attrs = missionDict.Attributes;
                        if (!attrs.HasFlag(MissionAttributes.Gather))
                            continue;
                        if (attrs.HasFlag(MissionAttributes.Collectables) || attrs.HasFlag(MissionAttributes.ReducedItems))
                            continue;

                        if (GetMissionKind(attrs) == _selectedMission)
                            mission.Value.GProfileId = entry.Id;
                    }

                    C.Save();
                }

                #endregion

                #region Profile Editor

                ImGui.TableNextColumn();
                #region Minimum GP + Dual Class Info

                int minGP = entry.MinimumGp;
                ImGui.SetNextItemWidth(100);
                if (ImGui.SliderInt("开始任务所需最低 GP", ref minGP, -1, maxGp))
                {
                    entry.MinimumGp = minGP;
                    C.SaveDebounced();
                }

                ImGui.Text("双职业任务的制作数量去哪了？");
                ImGui.SameLine();
                ImGui.Dummy(new(5, 0));
                ImGui.SameLine();
                ImGui.TextDisabled("?");
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetNextWindowSize(new(400.0f, 0.0f)); // Fixed width, auto height
                    ImGui.BeginTooltip();

                    ImGui.TextWrapped("简答：现在已经内置了\n" +
                     "详解：说实话，这个系统本身就很繁琐。而且随着 SE 决定第二个星球不再有双职业制作任务，我觉得直接把它绑到评价系统里会更好。实际上你只需要：\n" +
                     "金星：3 个物品\n" +
                     "银星：2 个物品\n" +
                     "铜星：1 个物品\n" +
                     "就能达到门槛。即使第一次没达成，它也会继续采集。再加上这样一来，我就不用为了……4 个任务去管理钓鱼配置了？在我看来有点小小的多余。\n" +
                     "所以现在的运作方式是：选择一项汇报选项（金星/任意 都一样），它会采集到所需数量 -> 完成后自动汇报任务。\n" +
                     "现在你们谁都不能再让它去做 27 个物品了。停下！它说的是制作 (╯°Д°)╯︵/(.□ . \\)");
                    ImGui.EndTooltip();
                }

                #endregion

                #region Boon Increase 2

                if (ImGui.CollapsingHeader("沃土 / 富矿的馈赠 II"))
                {
                    string buffName = "BoonIncrease2";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "额外采集奖励发生率提升30%";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }

                    ImGui.PopID();
                }

                #endregion

                #region Boon Increase 1

                if (ImGui.CollapsingHeader("沃土 / 富矿的馈赠 I"))
                {
                    string buffName = "BoonIncrease1";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "额外采集奖励发生率提升10%";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.PopID();
                }

                #endregion

                #region Nophica's / Nald'thal's Tidings

                if (ImGui.CollapsingHeader("诺菲卡 / 纳尔札尔 福音"))
                {
                    string buffName = "Tidings";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "额外采集奖励发生时的获得数增加1个";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.PopID();
                }

                #endregion

                #region Blessed / Kings Yield II

                if (ImGui.CollapsingHeader("天赐收成 / 莫非王土 II"))
                {
                    string buffName = "YieldII";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minUsableDurability = entry.GatherBuffs.Buffs[buffName].MinUsableDurability;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "令获得数增加2个\n" +
                                        "只在采集点满耐久时使用";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最少采集点采集次数", ref minUsableDurability, 0, 8))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinUsableDurability = minUsableDurability;
                        C.SaveDebounced();
                    }
                    ImGui_Ice.IconWithTooltip(Dalamud.Interface.FontAwesomeIcon.InfoCircle,
                        "采集点至少需要剩余多少耐久才会启用此技能？\n" +
                        "主要用于可以连锁刷新耐久度的任务");

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.PopID();
                }

                #endregion

                #region Blessed / Kings Yield I

                if (ImGui.CollapsingHeader("天赐收成 / 莫非王土 I"))
                {
                    string buffName = "YieldI";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minUsableDurability = entry.GatherBuffs.Buffs[buffName].MinUsableDurability;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "令获得数增加1个\n" +
                                        "只在采集点满耐久时使用";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最少采集点采集次数", ref minUsableDurability, 0, 8))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinUsableDurability = minUsableDurability;
                        C.SaveDebounced();
                    }
                    ImGui_Ice.IconWithTooltip(Dalamud.Interface.FontAwesomeIcon.InfoCircle,
                        "采集点至少需要剩余多少耐久才会启用此技能？\n" +
                        "主要用于可以连锁刷新耐久度的任务");

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.PopID();
                }

                #endregion

                #region Bonus Integrity

                if (ImGui.CollapsingHeader("农夫之智 / 石工之理"))
                {
                    string buffName = "BonusIntegrity";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minUsableDurability = entry.GatherBuffs.Buffs[buffName].MinUsableDurability;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "恢复1次采集次数\n" +
                                        "50%几率附加理智同兴预备状态";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最少采集点采集次数", ref minUsableDurability, 0, 8))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinUsableDurability = minUsableDurability;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.PopID();
                }

                #endregion

                #region Bountiful Yield II

                if (ImGui.CollapsingHeader("高产 II / 丰收 II"))
                {
                    string buffName = "BountifulYieldII";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "令获得数增加2个\n" +
                                        "只在采集点满耐久时使用";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.Text("最低需求采集物品数量");
                    ImGui.SameLine();
                    int minItems = entry.GatherBuffs.BountifulMinItem;
                    if (ImGui.DragInt("##MinItemsGather", ref minItems, 1, 2, 4))
                    {
                        entry.GatherBuffs.BountifulMinItem = minItems;
                        C.SaveDebounced();
                    }

                    ImGui.PopID();
                }

                #endregion

                #region Field Mastery (Gather Chance)

                #region Field Mastery III

                if (ImGui.CollapsingHeader("环境探知III | 敏锐视野III"))
                {
                    string buffName = "FieldMasteryIII";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "令获得率提高50%\n" +
                                        "请注意：您可以同时启用多个同类技能选项，但最终只会应用那个最接近 " +
                                        "100% 获得率、同时最节省 GP 的方案。";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.PopID();
                }

                #endregion

                #region Field Mastery II

                if (ImGui.CollapsingHeader("环境探知II | 敏锐视野II"))
                {
                    string buffName = "FieldMasteryII";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "令获得率提高15%\n" +
                                        "请注意：您可以同时启用多个同类技能选项，但最终只会应用那个最接近 " +
                                        "100% 获得率、同时最节省 GP 的方案。";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.PopID();
                }

                #endregion

                #region Field Mastery I

                if (ImGui.CollapsingHeader("环境探知 | 敏锐视野"))
                {
                    string buffName = "FieldMasteryI";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "令获得率提高5%\n" +
                                        "请注意：您可以同时启用多个同类技能选项，但最终只会应用那个最接近 " +
                                        "100% 获得率、同时最节省 GP 的方案。";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.PopID();
                }

                #endregion

                #region Field Mastery [Temp]

                if (ImGui.CollapsingHeader("植被专精 | 明晰视野 [临时]"))
                {
                    string buffName = "FieldMasteryTemp";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = "令获得率提高15%\n" +
                                        "备注：此技能可与 环境探知/敏锐视野 一同使用，但仅生效于单次采集";

                    ImGui.Text($"技能详情: ");
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox("启用", ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("使用所需最低 GP", ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt("最大使用次数", ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker("设置为 -1 时允许无限使用\n" +
                                       "设置为 1 -> X 时，设定为每次任务的最大使用次数上限");

                    ImGui.PopID();
                }

                #endregion

                #endregion

                #endregion

                ImGui.EndTable();
            }

            ImGui.Separator();
            if (ImGui.Button("复制选中的配置"))
            {
                string export = ExportGatherProfile(C.SelectedGatherIndex);
                ImGui.SetClipboardText(export);
            }

            if (ImGui.Button("导入配置"))
            {
                string importProfile = ImGui.GetClipboardText();
                string errorMessage = "";
                ImportGatherProfile(importProfile, out errorMessage);
                if (errorMessage != "")
                {
                    IceLogging.Error(errorMessage);
                }
                C.Save();
            }

            ImGui.Dummy(new Vector2(0, 10));

            ImGui.Separator();

            ImGui.Dummy(new Vector2(0, 10));

            using (ImRaii.Disabled(!ImGui.IsKeyDown(ImGuiKey.LeftShift)))
            {
                if (ImGui.Button("覆盖为推荐采集配置"))
                {
                    SetupAllProfiles();

                    C.Save();
                }
            }
            ImGuiEx.HelpMarker("请注意：\n" +
                               "这将清除您当前的所有采集配置，并将我的推荐配置应用到每个任务类型。\n" +
                               "对大多数人来说这没有问题，这个功能主要是为那些不确定该如何设置的人准备的。" +
                               "如果您接受此操作，请按住左 Shift 键并点击以应用。");

            using (ImRaii.Disabled(!ImGui.IsKeyDown(ImGuiKey.LeftShift)))
            {
                if (ImGui.Button("重置钓鱼预设"))
                {
                    ResetAllFisherProfiles();
                }
            }
            ImGuiEx.HelpMarker("将所有钓鱼预设重置为默认的内置设置\n" +
                "按住左 Shift 以允许应用");
        }

        private static MissionKinds GetMissionKind(MissionAttributes attrs)
        {
            bool limited = attrs.HasFlag(MissionAttributes.Limited);
            bool timed = attrs.HasFlag(MissionAttributes.Score_TimeRemaining);
            bool chain = attrs.HasFlag(MissionAttributes.Score_Chain);
            bool boon = attrs.HasFlag(MissionAttributes.Score_Boon);
            bool collects = attrs.HasFlag(MissionAttributes.Collectables);
            bool reduced = attrs.HasFlag(MissionAttributes.ReducedItems);
            bool grGatherX = attrs.HasFlag(MissionAttributes.GreaterReach_GatherX);
            bool grBoon = attrs.HasFlag(MissionAttributes.GreaterReach_Boon);
            bool grChain = attrs.HasFlag(MissionAttributes.GreaterReach_Chain);
            bool grBoonCh = attrs.HasFlag(MissionAttributes.GreaterReach_Boon_Chain);

            if (grBoonCh) return MissionKinds.GreaterReach_Boon_Chain;
            if (grChain) return MissionKinds.GreaterReach_Chain;
            if (grBoon) return MissionKinds.GreaterReach_Boon;
            if (grGatherX) return MissionKinds.GreaterReach_GatherX;
            if (limited) return MissionKinds.LimitedNodes;
            if (timed) return MissionKinds.TimeAttack;
            if (chain && boon) return MissionKinds.Chain_Boon;
            if (chain) return MissionKinds.Chain_Scoring;
            if (boon) return MissionKinds.Boon_Scoring;

            // DualClass uses craftMission flag rather than attrs pattern
            if (attrs.HasFlag(MissionAttributes.Craft)) return MissionKinds.DualClass;

            return MissionKinds.GatherX; // fallback: plain gather
        }

        public static void SetupAllProfiles()
        {
            foreach (var profile in C.GatherProfiles)
            {
                if (profile.Key == 0)
                    continue;
                else
                {
                    C.GatherProfiles.Remove(profile.Key);
                    foreach (var mission in C.MissionConfig)
                    {
                        if (mission.Value.GProfileId == profile.Key)
                        {
                            mission.Value.GProfileId = 0; // fallback to default
                        }
                    }
                }
            }

            string timedMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IlRpbWVkIE1pc3Npb25zIiwiTWluaW11bUdwIjoxMDAsIkR1YWxDbGFzc0NyYWZ0QW1vdW50IjoxLCJHYXRoZXJCdWZmcyI6eyJCdWZmcyI6eyJCb29uSW5jcmVhc2UyIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiQm9vbkluY3JlYXNlMSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";
            string limitedMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkxpbWl0ZWQgTm9kZXMiLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIlRpZGluZ3MiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyMDAsIk1heFVzZSI6LTF9LCJZaWVsZElJIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjo1MDAsIk1heFVzZSI6LTF9LCJZaWVsZEkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo0MDAsIk1heFVzZSI6LTF9LCJCb3VudGlmdWxZaWVsZElJIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJCb251c0ludGVncml0eSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjMwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Q2hhbmNlIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjowLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5VGVtcCI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9fSwiQm91bnRpZnVsTWluSXRlbSI6NH19";
            string chainedMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkNoYWluZWQiLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJCb29uSW5jcmVhc2UxIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9LCJUaWRpbmdzIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjAwLCJNYXhVc2UiOi0xfSwiWWllbGRJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJCb251c0ludGVncml0eSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";
            string DualClass = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkR1YWwgQ2xhc3MiLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjIsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";
            string boonMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkJvb24iLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIlRpZGluZ3MiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyMDAsIk1heFVzZSI6LTF9LCJZaWVsZElJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAwLCJNYXhVc2UiOi0xfSwiWWllbGRJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NDAwLCJNYXhVc2UiOi0xfSwiQm91bnRpZnVsWWllbGRJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjozMDAsIk1heFVzZSI6LTF9LCJCb251c0ludGVncml0eUNoYW5jZSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjI1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5VGVtcCI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfX0sIkJvdW50aWZ1bE1pbkl0ZW0iOjR9fQ==";
            string ChainBoonMission = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkNoYWluZWQgXHUwMDJCIEJvb24iLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MDAsIk1heFVzZSI6LTF9LCJZaWVsZEkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo0MDAsIk1heFVzZSI6LTF9LCJCb3VudGlmdWxZaWVsZElJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjMwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Q2hhbmNlIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjowLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlUZW1wIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9fSwiQm91bnRpZnVsTWluSXRlbSI6NH19";
            string GatherXAmount = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkdhdGhlciBYIEFtb3VudCIsIk1pbmltdW1HcCI6LTEsIkR1YWxDbGFzc0NyYWZ0QW1vdW50IjoxLCJHYXRoZXJCdWZmcyI6eyJCdWZmcyI6eyJCb29uSW5jcmVhc2UyIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiQm9vbkluY3JlYXNlMSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";

            string GreaterReach_GatherX = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkdyZWF0ZXIgUmVhY2ggW0dhdGhlciBYXSIsIk1pbmltdW1HcCI6LTEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJCb29uSW5jcmVhc2UxIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJUaWRpbmdzIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiWWllbGRJSSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6NTAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiWWllbGRJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NDAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiQm91bnRpZnVsWWllbGRJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjozMDAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJCb251c0ludGVncml0eUNoYW5jZSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkZpZWxkTWFzdGVyeUlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjI1MCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkZpZWxkTWFzdGVyeUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiRmllbGRNYXN0ZXJ5SSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiRmllbGRNYXN0ZXJ5VGVtcCI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfX19fQ==";
            string GreaterReach_Boon = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkdyZWF0ZXIgUmVhY2ggW0Jvb25dIiwiTWluaW11bUdwIjotMSwiR2F0aGVyQnVmZnMiOnsiQnVmZnMiOnsiQm9vbkluY3JlYXNlMiI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiQm9vbkluY3JlYXNlMSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJUaWRpbmdzIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiWWllbGRJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJCb251c0ludGVncml0eSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH19fX0=";
            string GreaterReach_Chain = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkdyZWF0ZXIgUmVhY2ggW0NoYWluXSIsIk1pbmltdW1HcCI6LTEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJCb29uSW5jcmVhc2UxIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJUaWRpbmdzIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiWWllbGRJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJCb251c0ludGVncml0eSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH19fX0=";
            string GreaterReach_BoonCh = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkdyZWF0ZXIgUmVhY2ggW0Jvb24gXHUwMDJCIENoYWluXSIsIk1pbmltdW1HcCI6LTEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIllpZWxkSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MDAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJZaWVsZEkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo0MDAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJCb3VudGlmdWxZaWVsZElJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiQm9udXNJbnRlZ3JpdHkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjMwMCwiTWF4VXNlIjotMSwiTWluVXNhYmxlRHVyYWJpbGl0eSI6MH0sIkJvbnVzSW50ZWdyaXR5Q2hhbmNlIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjowLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiRmllbGRNYXN0ZXJ5SUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjUwLCJNYXhVc2UiOi0xLCJNaW5Vc2FibGVEdXJhYmlsaXR5IjowfSwiRmllbGRNYXN0ZXJ5SUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJGaWVsZE1hc3RlcnlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9LCJGaWVsZE1hc3RlcnlUZW1wIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTEsIk1pblVzYWJsZUR1cmFiaWxpdHkiOjB9fX19";

            GatherSettings.InitialSetupProfile(timedMissions, MissionKinds.TimeAttack, out var _);
            GatherSettings.InitialSetupProfile(limitedMissions, MissionKinds.LimitedNodes, out var _);
            GatherSettings.InitialSetupProfile(chainedMissions, MissionKinds.Chain_Scoring, out var _);
            GatherSettings.InitialSetupProfile(boonMissions, MissionKinds.Boon_Scoring, out var _);
            GatherSettings.InitialSetupProfile(ChainBoonMission, MissionKinds.Chain_Boon, out var _);
            GatherSettings.InitialSetupProfile(DualClass, MissionKinds.DualClass, out var _);
            GatherSettings.InitialSetupProfile(GatherXAmount, MissionKinds.GatherX, out var _);
            GatherSettings.InitialSetupProfile(GreaterReach_GatherX, MissionKinds.GreaterReach_GatherX, out var _);
            GatherSettings.InitialSetupProfile(GreaterReach_Chain, MissionKinds.GreaterReach_Chain, out var _);
            GatherSettings.InitialSetupProfile(GreaterReach_Boon, MissionKinds.GreaterReach_Boon, out var _);
            GatherSettings.InitialSetupProfile(GreaterReach_BoonCh, MissionKinds.GreaterReach_Boon_Chain, out var _);

        }

        private static void ResetAllFisherProfiles()
        {
            IceLogging.Verbose("User has selected to reset all fishing presets, respecting request", "Gathering Settings");
            foreach (var config in C.MissionConfig)
            {
                config.Value.Use_BuildinPreset = true;
                config.Value.AutoHookPresetName = string.Empty;
            }
            C.SaveDebounced();
        }
    }
}