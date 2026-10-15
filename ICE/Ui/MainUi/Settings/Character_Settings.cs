using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using Pictomancy;
using System.Collections.Generic;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings
{
    internal class Character_Settings
    {
        public static void Draw()
        {
            // Register current character whenever this panel is open
            if (Player.Available)
            {
                var cid = (ulong)Player.CID;
                C.KnownCharacters[cid] = Player.Name;
            }

            if (ImGui.BeginTabBar("CharacterSettingsTabs"))
            {
                if (ImGui.BeginTabItem("全局"))
                {
                    if (ImGui.BeginChild("Global Character Settings"))
                    {
                        RepairSettings(null);
                        ImGui.Separator();
                        ArtisanSettingsV2(null);
                        ImGui.Separator();
                        MountSelection(null);
                        ImGui.Separator();
                        RelicJobSwap(null);
                    }
                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                foreach (var (cid, name) in C.KnownCharacters.ToList())
                {
                    bool tabOpen = true;
                    if (ImGui.BeginTabItem($"{name}##cid_{cid}", ref tabOpen))
                    {
                        if (!C.CharacterOverrides.ContainsKey(cid))
                        {
                            C.CharacterOverrides[cid] = new();
                            C.Save();
                        }

                        if (ImGui.BeginChild("Character Specific"))
                        {
                            var ov = C.CharacterOverrides[cid];
                            RepairSettings(ov);
                            ImGui.Separator();
                            ArtisanSettingsV2(ov);
                            ImGui.Separator();
                            MountSelection(ov);
                            ImGui.Separator();
                            RelicJobSwap(ov);

                        }
                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }

                    if (!tabOpen)
                    {
                        C.CharacterOverrides.Remove(cid);
                        C.KnownCharacters.Remove(cid);
                        C.Save();
                        break;
                    }
                }

                ImGui.EndTabBar();
            }
        }

        // -------------------------------------------------------------------------
        // Override helper — renders a small checkbox to toggle the override, then
        // the actual control (greyed out when not overriding).
        // drawControl receives the current effective value and should render the
        // ImGui control, write back into `ov` directly, and return true if changed.
        // -------------------------------------------------------------------------
        private static void OverrideField<T>(string id, T globalVal, T? overrideVal, Action<T?> setOverride, Action<T> drawControl) where T : struct
        {
            bool hasOverride = overrideVal.HasValue;
            if (ImGui.Checkbox($"##ov_{id}", ref hasOverride))
            {
                setOverride(hasOverride ? globalVal : null);
                C.Save();
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(hasOverride
                    ? "已覆盖 —— 点击以继承全局设置"
                    : "点击为该角色覆盖此设置");

            ImGui.SameLine();

            using (ImRaii.Disabled(!hasOverride))
                drawControl(overrideVal ?? globalVal);
        }

        // Reference-type variant (string, Global_Artisan)
        private static void OverrideFieldRef<T>(string id, T globalVal, T? overrideVal, Action<T?> setOverride, Action<T> drawControl) where T : class
        {
            bool hasOverride = overrideVal != null;
            if (ImGui.Checkbox($"##ov_{id}", ref hasOverride))
            {
                // For reference types, seed with a clone of global so the user starts
                // from the current global value rather than blank.
                setOverride(hasOverride ? CloneRef(globalVal) : null);
                C.Save();
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(hasOverride
                    ? "已覆盖 —— 点击以继承全局设置"
                    : "点击为该角色覆盖此设置");

            ImGui.SameLine();

            using (ImRaii.Disabled(!hasOverride))
                drawControl(overrideVal ?? globalVal);
        }

        // Shallow-clone helper for Global_Artisan seeding
        private static T CloneRef<T>(T source) where T : class
        {
            if (source is Global_Artisan a)
                return (new Global_Artisan
                {
                    SolverType = a.SolverType,
                    FoodId = a.FoodId,
                    FoodHQ = a.FoodHQ,
                    PotionId = a.PotionId,
                    PotionHQ = a.PotionHQ,
                    ManualId = a.ManualId,
                    SquadronManual = a.SquadronManual,
                } as T)!;
            return source;
        }

        // -------------------------------------------------------------------------
        // Repair Settings
        // -------------------------------------------------------------------------
        private static void RepairSettings(CharacterOverride? ov)
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Hammer, "修理设置");
            ImGui.Dummy(new Vector2(0, 5));

            if (ov == null)
            {
                // ── Global ──────────────────────────────────────────────────────
                bool repairAtVendor = C.RepairAtVendor;
                if (ImGui.Checkbox("NPC 修理工修理", ref repairAtVendor))
                { C.RepairAtVendor = repairAtVendor; C.Save(); }

                using (ImRaii.Disabled(repairAtVendor))
                {
                    bool selfRepairGather = C.SelfRepairGather;
                    if (ImGui.Checkbox("采集时自己修理", ref selfRepairGather))
                    { C.SelfRepairGather = selfRepairGather; C.Save(); }

                    bool selfRepairCrafter = C.SelfRepairCrafter;
                    if (ImGui.Checkbox("制作时自己修理", ref selfRepairCrafter))
                    { C.SelfRepairCrafter = selfRepairCrafter; C.Save(); }
                }

                float repairAmount = C.RepairPercent;
                ImGui.SetNextItemWidth(150);
                if (ImGui.SliderFloat("###Repair %", ref repairAmount, 0f, 99f, "%.0f%%"))
                {
                    if (C.RepairPercent != (int)repairAmount)
                    { C.RepairPercent = (int)repairAmount; C.SaveDebounced(); }
                }

                bool repairAll = C.RepairAllGear;
                if (ImGui.Checkbox("修理背包中所有装备", ref repairAll))
                { C.RepairAllGear = repairAll; C.Save(); }

                bool stopDarkMatter = C.Stop_DarkMatter;
                if (ImGui.Checkbox("暗物质低于此数量时停止", ref stopDarkMatter))
                { C.Stop_DarkMatter = stopDarkMatter; C.Save(); }

                int minDM = C.Minimum_DarkMatter;
                ImGui.SetNextItemWidth(200);
                if (ImGui.InputInt("最低 8 级暗物质数量", ref minDM, 1, 10))
                { C.Minimum_DarkMatter = minDM; C.SaveDebounced(); }
            }
            else
            {
                // ── Per-character override ───────────────────────────────────────
                OverrideField("RepairAtVendor", C.RepairAtVendor, ov.RepairAtVendor,
                    v => ov.RepairAtVendor = v,
                    current => {
                        bool v = current;
                        if (ImGui.Checkbox("NPC 修理工修理", ref v) && ov.RepairAtVendor.HasValue)
                        { ov.RepairAtVendor = v; C.Save(); }
                    });

                bool effectiveVendor = ov.RepairAtVendor ?? C.RepairAtVendor;
                using (ImRaii.Disabled(effectiveVendor))
                {
                    OverrideField("SelfRepairGather", C.SelfRepairGather, ov.SelfRepairGather,
                        v => ov.SelfRepairGather = v,
                        current => {
                            bool v = current;
                            if (ImGui.Checkbox("采集时自己修理", ref v) && ov.SelfRepairGather.HasValue)
                            { ov.SelfRepairGather = v; C.Save(); }
                        });

                    OverrideField("SelfRepairCrafter", C.SelfRepairCrafter, ov.SelfRepairCrafter,
                        v => ov.SelfRepairCrafter = v,
                        current => {
                            bool v = current;
                            if (ImGui.Checkbox("制作时自己修理", ref v) && ov.SelfRepairCrafter.HasValue)
                            { ov.SelfRepairCrafter = v; C.Save(); }
                        });
                }

                OverrideField("RepairPercent", C.RepairPercent, ov.RepairPercent,
                    v => ov.RepairPercent = v,
                    current => {
                        float f = current;
                        ImGui.SetNextItemWidth(150);
                        if (ImGui.SliderFloat("###Repair %", ref f, 0f, 99f, "%.0f%%") && ov.RepairPercent.HasValue)
                        { ov.RepairPercent = (int)f; C.SaveDebounced(); }
                    });

                OverrideField("RepairAllGear", C.RepairAllGear, ov.RepairAllGear,
                    v => ov.RepairAllGear = v,
                    current => {
                        bool v = current;
                        if (ImGui.Checkbox("修理背包中所有装备", ref v) && ov.RepairAllGear.HasValue)
                        { ov.RepairAllGear = v; C.Save(); }
                    });

                // Stop_DarkMatter is global-only — no per-character override
                using (ImRaii.Disabled(true))
                {
                    bool stopDM = C.Stop_DarkMatter;
                    ImGui.Checkbox("暗物质低于此数量时停止（全局）", ref stopDM);
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    ImGui.SetTooltip("此设置为全局设置，所有角色共享。\n请在“全局”标签页中编辑。");

                OverrideField("Minimum_DarkMatter", C.Minimum_DarkMatter, ov.Minimum_DarkMatter,
                    v => ov.Minimum_DarkMatter = v,
                    current => {
                        int v = current;
                        ImGui.SetNextItemWidth(200);
                        if (ImGui.InputInt("最低 8 级暗物质数量", ref v, 1, 10) && ov.Minimum_DarkMatter.HasValue)
                        { ov.Minimum_DarkMatter = v; C.SaveDebounced(); }
                    });
            }

            PlayerHelper.GetItemCount(Utils.DarkMatter_8Id, out var dmCount);
            ImGui.Text($"当前拥有：{dmCount:N0} 个 8 级暗物质");
        }

        // -------------------------------------------------------------------------
        // Artisan Settings
        // -------------------------------------------------------------------------
        private static void ArtisanSettingsV2(CharacterOverride? ov)
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Wrench, "全局 Artisan 设置");
            ImGui.Dummy(new Vector2(0, 5));

            // Resolve which artisan objects we're actually editing
            var craft_Standard = ov?.Artisan_GlobalStandard ?? C.Artisan_GlobalStandard;
            var craft_Expert = ov?.Artisan_GlobalExpert ?? C.Artisan_GlobalExpert;

            List<ArtisanCraftType> global_StandardModes = new()
            {
                ArtisanCraftType.Default,
                ArtisanCraftType.ProgressOnly,
                ArtisanCraftType.Standard,
                ArtisanCraftType.Raphael,
            };
            List<ArtisanCraftType> global_ExpertModes = new()
            {
                ArtisanCraftType.Default,
                ArtisanCraftType.Expert,
                ArtisanCraftType.Raphael,
            };

            // If we're in character mode and the override objects don't exist yet we
            // show an "Override Artisan settings" master toggle instead of the full table.
            if (ov != null)
            {
                bool hasStandardOverride = ov.Artisan_GlobalStandard != null;
                bool hasExpertOverride = ov.Artisan_GlobalExpert != null;

                if (ImGui.Checkbox("覆盖标准 Artisan 设置##ovStd", ref hasStandardOverride))
                {
                    ov.Artisan_GlobalStandard = hasStandardOverride ? CloneRef(C.Artisan_GlobalStandard) : null;
                    C.Save();
                }
                ImGui.SameLine();
                if (ImGui.Checkbox("覆盖专家 Artisan 设置##ovExp", ref hasExpertOverride))
                {
                    ov.Artisan_GlobalExpert = hasExpertOverride ? CloneRef(C.Artisan_GlobalExpert) : null;
                    C.Save();
                }
                ImGui.Dummy(new Vector2(0, 3));
            }

            #region Labels

            string GetSolverLabel(ArtisanCraftType type) => type switch
            {
                ArtisanCraftType.Default => "默认",
                ArtisanCraftType.Raphael => "Raphael 求解器",
                ArtisanCraftType.ProgressOnly => "仅进度求解器",
                ArtisanCraftType.Standard => "标准求解器",
                ArtisanCraftType.Expert => "专家配方求解器",
                _ => "未知"
            };

            string GetFoodLabel(uint foodId)
            {
                if (foodId == 0) return "默认";
                var item = ConsumableInfo.CrafterFood.FirstOrDefault(x => x.Id == foodId);
                PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                return BuildItemLabel(item.Name, nq, hq);
            }
            string GetPotionLabel(uint potionId)
            {
                if (potionId == 0) return "默认";
                var item = ConsumableInfo.Pots.FirstOrDefault(x => x.Id == potionId);
                PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                return BuildItemLabel(item.Name, nq, hq);
            }
            string GetManualLabel(uint manualId)
            {
                if (manualId == 0) return "默认";
                var item = ConsumableInfo.Manuals.FirstOrDefault(x => x.Id == manualId);
                PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                return BuildItemLabel(item.Name, nq, 0);
            }
            string GetSquadManualLabel(uint squadManualId)
            {
                if (squadManualId == 0) return "默认";
                var item = ConsumableInfo.SquadronManuals.FirstOrDefault(x => x.Id == squadManualId);
                PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                return BuildItemLabel(item.Name, nq, 0);
            }
            string BuildItemLabel(string name, int nqCount, int hqCount)
            {
                var parts = new List<string>();
                if (hqCount > 0) parts.Add($"{(char)0xE03C} {name} [x{hqCount}]");
                if (nqCount > 0) parts.Add($"{name} [x{nqCount}]");
                return string.Join(" / ", parts);
            }

            var std_FoodLabel = GetFoodLabel(craft_Standard.FoodId);
            var exp_FoodLabel = GetFoodLabel(craft_Expert.FoodId);
            var std_PotionLabel = GetPotionLabel(craft_Standard.PotionId);
            var exp_PotionLabel = GetPotionLabel(craft_Expert.PotionId);
            var std_ManualLabel = GetManualLabel(craft_Standard.ManualId);
            var exp_ManualLabel = GetManualLabel(craft_Expert.ManualId);
            var std_SquadLabel = GetSquadManualLabel(craft_Standard.SquadronManual);
            var exp_SquadLabel = GetSquadManualLabel(craft_Expert.SquadronManual);

            float std_ComboWidth = new[]
            {
                std_FoodLabel, std_PotionLabel, std_ManualLabel, std_SquadLabel,
                GetSolverLabel(craft_Standard.SolverType),
            }.Max(l => ImGui.CalcTextSize(l).X + ImGui.GetStyle().FramePadding.X * 2 + ImGui.GetStyle().ScrollbarSize + 10);

            float exp_ComboWidth = new[]
            {
                exp_FoodLabel, exp_PotionLabel, exp_ManualLabel, exp_SquadLabel,
                GetSolverLabel(craft_Expert.SolverType),
            }.Max(l => ImGui.CalcTextSize(l).X + ImGui.GetStyle().FramePadding.X * 2 + ImGui.GetStyle().ScrollbarSize + 10);

            #endregion

            // When in character mode and neither override is active, grey out the table
            bool tableDisabled = ov != null && ov.Artisan_GlobalStandard == null && ov.Artisan_GlobalExpert == null;
            using (ImRaii.Disabled(tableDisabled))
            {
                if (ImGui.BeginTable("ArtisanSettings", 3,
                    ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("");
                    ImGui.TableSetupColumn("标准制作设置");
                    ImGui.TableSetupColumn("专家制作设置");
                    ImGui.TableHeadersRow();

                    bool stdDisabled = ov != null && ov.Artisan_GlobalStandard == null;
                    bool expDisabled = ov != null && ov.Artisan_GlobalExpert == null;

                    void SaveArtisan()
                    {
                        if (ov == null)
                        {

                        }
                        C.Save();
                    }

                    #region Solver Row
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("求解器类型");

                    ImGui.TableNextColumn();
                    using (ImRaii.Disabled(stdDisabled))
                    {
                        ImGui.SetNextItemWidth(std_ComboWidth);
                        if (ImGui.BeginCombo("##StandardSolverType", GetSolverLabel(craft_Standard.SolverType)))
                        {
                            foreach (var type in global_StandardModes)
                            {
                                bool sel = craft_Standard.SolverType == type;
                                if (ImGui.Selectable(GetSolverLabel(type), sel))
                                { 
                                    craft_Standard.SolverType = type; 
                                    SaveArtisan(); 
                                }
                                if (sel) ImGui.SetItemDefaultFocus();
                            }
                            ImGui.EndCombo();
                        }
                    }

                    ImGui.TableNextColumn();
                    using (ImRaii.Disabled(expDisabled))
                    {
                        ImGui.SetNextItemWidth(exp_ComboWidth);
                        if (ImGui.BeginCombo("##ExpertSolverType", GetSolverLabel(craft_Expert.SolverType)))
                        {
                            foreach (var type in global_ExpertModes)
                            {
                                bool sel = craft_Expert.SolverType == type;
                                if (ImGui.Selectable(GetSolverLabel(type), sel))
                                { 
                                    craft_Expert.SolverType = type; 
                                    SaveArtisan(); 
                                }
                                if (sel) 
                                    ImGui.SetItemDefaultFocus();
                            }
                            ImGui.EndCombo();
                        }
                    }
                    #endregion

                    bool supportedArtisan = P.Artisan.UpdatedArtisan();
                    if (supportedArtisan)
                    {
                        #region Food Row
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("食物");

                        ImGui.TableNextColumn();
                        using (ImRaii.Disabled(stdDisabled))
                        {
                            ImGui.SetNextItemWidth(std_ComboWidth);
                            if (ImGui.BeginCombo("##StandardFood", std_FoodLabel))
                            {
                                if (ImGui.Selectable("默认", craft_Standard.FoodId == 0))
                                { 
                                    craft_Standard.FoodId = 0; 
                                    craft_Standard.FoodHQ = false; 
                                    SaveArtisan(); 
                                }
                                if (craft_Standard.FoodId == 0) 
                                    ImGui.SetItemDefaultFocus();
                                ImGui.Separator();
                                foreach (var item in ConsumableInfo.CrafterFood)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                    PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                                    if (nq == 0 && hq == 0) 
                                        continue;
                                    bool sel = craft_Standard.FoodId == item.Id;
                                    if (ImGui.Selectable(BuildItemLabel(item.Name, nq, hq) + $"###{item.Id}", sel))
                                    { 
                                        craft_Standard.FoodId = item.Id; 
                                        craft_Standard.FoodHQ = hq > 0; 
                                        SaveArtisan(); 
                                    }
                                    if (sel) 
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                        }

                        ImGui.TableNextColumn();
                        using (ImRaii.Disabled(expDisabled))
                        {
                            ImGui.SetNextItemWidth(exp_ComboWidth);
                            if (ImGui.BeginCombo("##ExpertFood", exp_FoodLabel))
                            {
                                if (ImGui.Selectable("默认", craft_Expert.FoodId == 0))
                                { 
                                    craft_Expert.FoodId = 0; 
                                    craft_Expert.FoodHQ = false; 
                                    SaveArtisan(); 
                                }
                                if (craft_Expert.FoodId == 0) 
                                    ImGui.SetItemDefaultFocus();
                                ImGui.Separator();
                                foreach (var item in ConsumableInfo.CrafterFood)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                    PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                                    if (nq == 0 && hq == 0) 
                                        continue;
                                    bool sel = craft_Expert.FoodId == item.Id;
                                    if (ImGui.Selectable(BuildItemLabel(item.Name, nq, hq) + $"###{item.Id}", sel))
                                    { 
                                        craft_Expert.FoodId = item.Id; 
                                        craft_Expert.FoodHQ = hq > 0; 
                                        SaveArtisan(); 
                                    }
                                    if (sel) 
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                        }
                        #endregion

                        #region Potion Row
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("爆发药");

                        ImGui.TableNextColumn();
                        using (ImRaii.Disabled(stdDisabled))
                        {
                            ImGui.SetNextItemWidth(std_ComboWidth);
                            if (ImGui.BeginCombo("##StandardPotion", std_PotionLabel))
                            {
                                if (ImGui.Selectable("默认", craft_Standard.PotionId == 0))
                                { 
                                    craft_Standard.PotionId = 0; 
                                    craft_Standard.PotionHQ = false; 
                                    SaveArtisan(); 
                                }
                                if (craft_Standard.PotionId == 0) 
                                    ImGui.SetItemDefaultFocus();
                                ImGui.Separator();
                                foreach (var item in ConsumableInfo.Pots)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                    PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                                    if (nq == 0 && hq == 0) 
                                        continue;
                                    bool sel = craft_Standard.PotionId == item.Id;
                                    if (ImGui.Selectable(BuildItemLabel(item.Name, nq, hq) + $"###{item.Id}", sel))
                                    { 
                                        craft_Standard.PotionId = item.Id; 
                                        craft_Standard.PotionHQ = hq > 0; 
                                        SaveArtisan(); 
                                    }
                                    if (sel) 
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                        }

                        ImGui.TableNextColumn();
                        using (ImRaii.Disabled(expDisabled))
                        {
                            ImGui.SetNextItemWidth(exp_ComboWidth);
                            if (ImGui.BeginCombo("##ExpertPotion", exp_PotionLabel))
                            {
                                if (ImGui.Selectable("默认", craft_Expert.PotionId == 0))
                                { 
                                    craft_Expert.PotionId = 0; 
                                    craft_Expert.PotionHQ = false; 
                                    SaveArtisan(); 
                                }
                                if (craft_Expert.PotionId == 0) 
                                    ImGui.SetItemDefaultFocus();
                                ImGui.Separator();
                                foreach (var item in ConsumableInfo.Pots)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                    PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                                    if (nq == 0 && hq == 0) 
                                        continue;
                                    bool sel = craft_Expert.PotionId == item.Id;
                                    if (ImGui.Selectable(BuildItemLabel(item.Name, nq, hq) + $"###{item.Id}", sel))
                                    { 
                                        craft_Expert.PotionId = item.Id; 
                                        craft_Expert.PotionHQ = hq > 0; 
                                        SaveArtisan(); 
                                    }
                                    if (sel) 
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                        }
                        #endregion

                        #region Manual Row
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("手册");

                        ImGui.TableNextColumn();
                        using (ImRaii.Disabled(stdDisabled))
                        {
                            ImGui.SetNextItemWidth(std_ComboWidth);
                            if (ImGui.BeginCombo("##StandardManual", std_ManualLabel))
                            {
                                if (ImGui.Selectable("默认", craft_Standard.ManualId == 0))
                                { 
                                    craft_Standard.ManualId = 0; 
                                    SaveArtisan(); 
                                }
                                if (craft_Standard.ManualId == 0) 
                                    ImGui.SetItemDefaultFocus();
                                ImGui.Separator();
                                foreach (var item in ConsumableInfo.Manuals)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                    if (nq == 0) 
                                        continue;
                                    bool sel = craft_Standard.ManualId == item.Id;
                                    if (ImGui.Selectable(BuildItemLabel(item.Name, nq, 0) + $"###{item.Id}", sel))
                                    { 
                                        craft_Standard.ManualId = item.Id; 
                                        SaveArtisan(); }
                                    if (sel) 
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                        }

                        ImGui.TableNextColumn();
                        using (ImRaii.Disabled(expDisabled))
                        {
                            ImGui.SetNextItemWidth(exp_ComboWidth);
                            if (ImGui.BeginCombo("##ExpertManual", exp_ManualLabel))
                            {
                                if (ImGui.Selectable("默认", craft_Expert.ManualId == 0))
                                { 
                                    craft_Expert.ManualId = 0; 
                                    SaveArtisan(); 
                                }
                                if (craft_Expert.ManualId == 0) 
                                    ImGui.SetItemDefaultFocus();
                                ImGui.Separator();
                                foreach (var item in ConsumableInfo.Manuals)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                    if (nq == 0) continue;
                                    bool sel = craft_Expert.ManualId == item.Id;
                                    if (ImGui.Selectable(BuildItemLabel(item.Name, nq, 0) + $"###{item.Id}", sel))
                                    { 
                                        craft_Expert.ManualId = item.Id; 
                                        SaveArtisan(); 
                                    }
                                    if (sel) 
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                        }
                        #endregion

                        #region Squadron Manual Row
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("中队手册");

                        ImGui.TableNextColumn();
                        using (ImRaii.Disabled(stdDisabled))
                        {
                            ImGui.SetNextItemWidth(std_ComboWidth);
                            if (ImGui.BeginCombo("##StandardSquadManual", std_SquadLabel))
                            {
                                if (ImGui.Selectable("默认", craft_Standard.SquadronManual == 0))
                                { 
                                    craft_Standard.SquadronManual = 0; 
                                    SaveArtisan(); 
                                }
                                if (craft_Standard.SquadronManual == 0) 
                                    ImGui.SetItemDefaultFocus();
                                ImGui.Separator();
                                foreach (var item in ConsumableInfo.SquadronManuals)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                    if (nq == 0) continue;
                                    bool sel = craft_Standard.SquadronManual == item.Id;
                                    if (ImGui.Selectable(BuildItemLabel(item.Name, nq, 0) + $"###{item.Id}", sel))
                                    { 
                                        craft_Standard.SquadronManual = item.Id; 
                                        SaveArtisan(); 
                                    }
                                    if (sel) 
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                        }

                        ImGui.TableNextColumn();
                        using (ImRaii.Disabled(expDisabled))
                        {
                            ImGui.SetNextItemWidth(exp_ComboWidth);
                            if (ImGui.BeginCombo("##ExpertSquadManual", exp_SquadLabel))
                            {
                                if (ImGui.Selectable("默认", craft_Expert.SquadronManual == 0))
                                { 
                                    craft_Expert.SquadronManual = 0; 
                                    SaveArtisan(); 
                                }
                                if (craft_Expert.SquadronManual == 0) 
                                    ImGui.SetItemDefaultFocus();

                                ImGui.Separator();
                                foreach (var item in ConsumableInfo.SquadronManuals)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                    if (nq == 0) continue;
                                    bool sel = craft_Expert.SquadronManual == item.Id;
                                    if (ImGui.Selectable(BuildItemLabel(item.Name, nq, 0) + $"###{item.Id}", sel))
                                    { 
                                        craft_Expert.SquadronManual = item.Id; 
                                        SaveArtisan(); 
                                    }
                                    if (sel) 
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                        }
                        #endregion
                    }

                    ImGui.EndTable();
                }
            }
        }

        // -------------------------------------------------------------------------
        // Mount Selection
        // -------------------------------------------------------------------------
        private static bool _visualizeRadius = false;
        private static bool _visualizeDismountRadius = false;
        private static Dictionary<uint, string> _availableMounts = new();
        private static string _mountSearchText = "";
        private static int _mountDisplayOffset = 0;
        private static int _mountItemsPerPage = 10;

        private static unsafe void MountSelection(CharacterOverride? ov)
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Feather, "坐骑设置");
            ImGui.Dummy(new Vector2(0, 5));

            // Resolve effective values for display
            uint effectiveMountId = ov?.MountId ?? C.MountId;
            string effectiveMountName = ov?.MountName ?? C.MountName;
            bool effectiveMountOutside = C.UseMountOutsideMission;
            bool effectiveMountInMission = C.UseMountInMission;
            float effectiveMinMountRange = C.MountRadius;
            float effectiveDismountRange = C.DismountRadius;

            // Mount picker button
            if (ov == null)
            {
                if (ImGui.Button("选择坐骑"))
                    OpenMountPopup();
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"坐骑: {C.MountName}");
            }
            else
            {
                bool hasMountOverride = ov.MountId.HasValue;
                if (ImGui.Checkbox("##ovMount", ref hasMountOverride))
                {
                    if (hasMountOverride) { ov.MountId = C.MountId; ov.MountName = C.MountName; }
                    else { ov.MountId = null; ov.MountName = null; }
                    C.Save();
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(hasMountOverride
                        ? "已覆盖 —— 点击以继承全局设置"
                        : "点击为该角色覆盖坐骑");
                ImGui.SameLine();

                using (ImRaii.Disabled(!hasMountOverride))
                {
                    if (ImGui.Button("选择坐骑##charMount"))
                        OpenMountPopup();
                    ImGui.SameLine();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"坐骑: {effectiveMountName}");
                }
            }

            // Shared popup — writes to ov if present, else to C directly
            if (ImGui.BeginPopup("Mount Options"))
            {
                ImGui.InputText("搜索", ref _mountSearchText, 100);

                var filtered = _availableMounts
                    .Where(kvp => string.IsNullOrEmpty(_mountSearchText) ||
                                  kvp.Value.Contains(_mountSearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                int total = filtered.Count;
                int maxOffset = Math.Max(0, total - _mountItemsPerPage);
                _mountDisplayOffset = Math.Min(_mountDisplayOffset, maxOffset);

                foreach (var mount in filtered.Skip(_mountDisplayOffset).Take(_mountItemsPerPage))
                {
                    if (ImGui.Selectable($"{mount.Value}##{mount.Key}"))
                    {
                        if (ov != null && ov.MountId.HasValue)
                        { ov.MountId = mount.Key; ov.MountName = mount.Value; }
                        else if (ov == null)
                        { C.MountId = mount.Key; C.MountName = mount.Value; }
                        C.Save();
                        ImGui.CloseCurrentPopup();
                    }
                }

                ImGui.Separator();
                if (ImGui.Button("上页") && _mountDisplayOffset > 0)
                    _mountDisplayOffset = Math.Max(0, _mountDisplayOffset - _mountItemsPerPage);
                ImGui.SameLine();
                ImGui.Text($"{_mountDisplayOffset + 1}-{Math.Min(_mountDisplayOffset + _mountItemsPerPage, total)} / {total}");
                ImGui.SameLine();
                if (ImGui.Button("下页") && _mountDisplayOffset < maxOffset)
                    _mountDisplayOffset = Math.Min(maxOffset, _mountDisplayOffset + _mountItemsPerPage);

                ImGui.EndPopup();
            }

            // Bool / float fields
            if (ov == null)
            {
                bool mountOutside = C.UseMountOutsideMission;
                if (ImGui.Checkbox("任务外使用坐骑", ref mountOutside))
                { C.UseMountOutsideMission = mountOutside; C.Save(); }

                bool mountInMission = C.UseMountInMission;
                if (ImGui.Checkbox("任务内使用坐骑", ref mountInMission))
                { C.UseMountInMission = mountInMission; C.Save(); }

                float minRange = C.MountRadius;
                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("使用坐骑的最小范围", ref minRange, 1))
                { C.MountRadius = minRange; C.Save(); }
                ImGui.SameLine();
                ImGui.Checkbox("可视化使用坐骑半径范围", ref _visualizeRadius);

                float dismount = C.DismountRadius;
                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("下坐骑目标范围", ref dismount, 1))
                { C.DismountRadius = dismount; C.Save(); }
                ImGui.SameLine();
                ImGui.Checkbox("可视化下坐骑半径范围", ref _visualizeDismountRadius);
            }
            else
            {
                // These four are global-only — always read/write C regardless of character tab.
                // Show them as editable but with a tooltip clarifying they affect all characters.
                bool mountOutside = C.UseMountOutsideMission;
                if (ImGui.Checkbox("任务外使用坐骑##global", ref mountOutside))
                { 
                    C.UseMountOutsideMission = mountOutside; 
                    C.Save(); 
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("全局设置 —— 适用于所有角色。");

                bool mountInMission = C.UseMountInMission;
                if (ImGui.Checkbox("任务内使用坐骑##global", ref mountInMission))
                { C.UseMountInMission = mountInMission; C.Save(); }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("全局设置 —— 适用于所有角色。");

                float minRange = C.MountRadius;
                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("使用坐骑的最小范围##global", ref minRange, 1))
                { 
                    C.MountRadius = minRange; 
                    C.Save(); 
                }
                if (ImGui.IsItemHovered()) 
                    ImGui.SetTooltip("全局设置 —— 适用于所有角色。");
                ImGui.SameLine();
                ImGui.Checkbox("可视化使用坐骑半径范围", ref _visualizeRadius);

                float dismount = C.DismountRadius;
                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("下坐骑目标范围##global", ref dismount, 1))
                { 
                    C.DismountRadius = dismount; 
                    C.Save(); 
                }
                if (ImGui.IsItemHovered()) 
                    ImGui.SetTooltip("全局设置 —— 适用于所有角色。");
                ImGui.SameLine();
                ImGui.Checkbox("可视化下坐骑半径范围", ref _visualizeDismountRadius);
            }

            // Pictomancy visualisation (always uses resolved values)
            using (var drawList = PctService.Draw())
            {
                if (drawList == null) return;
                var playerPos = Player.Position;
                if (_visualizeRadius)
                    PctService.VfxRenderer.AddCircle("Mount_Radius Circle", playerPos,
                        C.MountRadius, Utils.FromUintABGR(2616716297));
                if (_visualizeDismountRadius)
                    PctService.VfxRenderer.AddCircle("Dismount_Radius Circle", playerPos,
                        C.DismountRadius, Utils.FromUintABGR(2601121571));
            }
        }

        private static unsafe void OpenMountPopup()
        {
            _availableMounts.Clear();
            _availableMounts[0] = "随机坐骑";
            var mountSheet = Svc.Data.GetExcelSheet<Mount>();
            foreach (var mountItem in mountSheet)
            {
                if (!PlayerState.Instance()->IsMountUnlocked(mountItem.RowId)) continue;
                string name = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(mountItem.Singular.ToString().ToLower());
                _availableMounts[mountItem.RowId] = name;
            }
            _mountSearchText = "";
            _mountDisplayOffset = 0;
            ImGui.OpenPopup("Mount Options");
        }

        // -------------------------------------------------------------------------
        // Relic Job Swap
        // -------------------------------------------------------------------------
        private static void RelicJobSwap(CharacterOverride? ov)
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Toolbox, "职业切换");
            ImGui.Dummy(new(0, 5));
            if (ov == null)
            {
                bool swapJobs = C.Relic_SwapJob;
                if (ImGui.Checkbox("提交宇宙工具时切换职业", ref swapJobs))
                { 
                    C.Relic_SwapJob = swapJobs; 
                    C.Save(); 
                }

                string currentJobName = BattleJobs.FirstOrDefault(x => x.Value == C.Relic_BattleJob).Key ?? "无";
                ImGui.SetNextItemWidth(200);
                if (ImGui.BeginCombo("战斗职业##relicJob", currentJobName))
                {
                    foreach (var (jobName, jobId) in BattleJobs)
                    {
                        bool sel = C.Relic_BattleJob == jobId;
                        if (ImGui.Selectable(jobName, sel))
                        { 
                            C.Relic_BattleJob = jobId; 
                            C.Save(); 
                        }
                        if (sel) 
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }

                bool useStylist = C.Relic_Stylist;
                if (ImGui.Checkbox("使用 Stylist 插件重新装备工具", ref useStylist))
                {
                    C.Relic_Stylist = useStylist;
                    C.Save();
                }
            }
            else
            {
                OverrideField("Relic_SwapJob", C.Relic_SwapJob, ov.Relic_SwapJob,
                    v => ov.Relic_SwapJob = v,
                    current => {
                        bool v = current;
                        if (ImGui.Checkbox("提交宇宙工具时切换职业", ref v) && ov.Relic_SwapJob.HasValue)
                        { 
                            ov.Relic_SwapJob = v; 
                            C.Save(); 
                        }
                    });

                OverrideField("Relic_BattleJob", C.Relic_BattleJob, ov.Relic_BattleJob,
                    v => ov.Relic_BattleJob = v,
                    current => {
                        string currentJobName = BattleJobs.FirstOrDefault(x => x.Value == current).Key ?? "无";
                        ImGui.SetNextItemWidth(200);
                        if (ImGui.BeginCombo("战斗职业##relicJobOv", currentJobName))
                        {
                            foreach (var (jobName, jobId) in BattleJobs)
                            {
                                bool sel = current == jobId;
                                if (ImGui.Selectable(jobName, sel) && ov.Relic_BattleJob.HasValue)
                                { 
                                    ov.Relic_BattleJob = jobId; 
                                    C.Save(); 
                                }
                                if (sel) ImGui.SetItemDefaultFocus();
                            }
                            ImGui.EndCombo();
                        }
                    });

                OverrideField("Relic_Stylist", C.Relic_Stylist, ov.Relic_Stylist,
                    v => ov.Relic_Stylist = v,
                    current =>
                    {
                        bool v = current;
                        if (ImGui.Checkbox("使用 Stylist 插件重新装备工具", ref v) && ov.Relic_Stylist.HasValue)
                        {
                            ov.Relic_Stylist = v;
                            C.Save();
                        }
                    });
            }
        }

        // -------------------------------------------------------------------------
        // Battle job lookup table
        // -------------------------------------------------------------------------
        private static readonly Dictionary<string, uint> BattleJobs = new()
        {
            // Tanks
            { "骑士", 19 }, { "战士", 21 }, { "暗黑骑士", 32 }, { "绝枪战士", 37 },
            // Healers
            { "白魔法师", 24 }, { "学者", 28 }, { "占星术士", 33 }, { "贤者", 40 },
            // Melee DPS
            { "武僧", 20 }, { "龙骑士", 22 }, { "忍者", 30 }, { "武士", 34 },
            { "钐镰客", 39 }, { "蝰蛇剑士", 41 },
            // Physical Ranged DPS
            { "吟游诗人", 23 }, { "机工士", 31 }, { "舞者", 38 },
            // Magical Ranged DPS
            { "黑魔法师", 25 }, { "召唤师", 27 }, { "赤魔法师", 35 }, { "绘灵法师", 42 },
        };
    }
}
