using Dalamud.Interface;
using ECommons.GameHelpers;
using ICE.Ui.Debug_Tabs.Debug_Ui;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.ImGuiTools;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings.Settings_Table
{
    internal class TravelSettings
    {
        private static FishingDebug _fishingDebug = null;

        public static unsafe void Draw()
        {
            if (_fishingDebug == null)
            {
                _fishingDebug = new FishingDebug();
            }

            PathfindingSettings();

            Separator();
            StuckSettings();

            Separator();
            CraftingLocations();

            Separator();
            FishingLocations();
        }

        private static void Separator()
        {
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
        }

        private static void PathfindingSettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Route, "寻路");
            ImGui.Dummy(new Vector2(0, 5));

            bool stellarSprint = C.MoonSprint;
            if (ImGui.Checkbox("自动使用宇宙冲刺", ref stellarSprint))
            {
                C.MoonSprint = stellarSprint;
                C.Save();
            }

            bool closestNode = C.ClosestNodeSelection;
            if (ImGui.Checkbox("优先最近的采集点", ref closestNode))
            {
                C.ClosestNodeSelection = closestNode;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("始终导航到最近的可交互采集点，而不是按固定路线顺序前进。\n对于重视速度的限时 EX+ 任务很有用。");
            }

            bool randomize = C.RandomizeWaypoints;
            if (ImGui.Checkbox("随机化路径点位置", ref randomize))
            {
                C.RandomizeWaypoints = randomize;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("为导航目标添加小幅随机偏移，使角色不会总是走完全相同的路线");
            }
            if (randomize)
            {
                ImGui.SameLine();
                float radius = C.RandomizeWaypointsRadius;
                ImGui.SetNextItemWidth(100);
                if (ImGui.SliderFloat("随机半径 (yalms)", ref radius, 0.5f, 1.0f, "%.1f"))
                {
                    C.RandomizeWaypointsRadius = radius;
                    C.SaveDebounced();
                }
                bool showDebug = C.RandomizeWaypointsDebug;
                if (ImGui.Checkbox("显示随机位置调试目标", ref showDebug))
                {
                    C.RandomizeWaypointsDebug = showDebug;
                    C.Save();
                }
            }

            int GatherFanRandom = C.GatherFanSectionSize;
            ImGui.SetNextItemWidth(200);
            if (ImGui.SliderInt("采集扇形选择", ref GatherFanRandom, 0, 360))
            {
                C.GatherFanSectionSize = GatherFanRandom;
                C.SaveDebounced();
            }
            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "这会调整从扇形中心点随机选取的范围。\n" +
                "360 = 整个扇形都可被选取\n" +
                "其它数值则会在该扇形范围内选取（如果可用）", false);

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


            bool useHubReturn = C.UseHubReturn;
            if (ImGui.Checkbox("使用基地返回", ref useHubReturn))
            {
                C.UseHubReturn = useHubReturn;
                C.Save();
            }
            ImGui.SameLine();
            bool useAethernet = C.UseAethernet;
            if (ImGui.Checkbox("使用以太之光网络", ref useAethernet))
            {
                C.UseAethernet = useAethernet;
                C.Save();
            }

            bool useRedAlertNpc = C.UseRedAlertNpc;
            if (ImGui.Checkbox("使用紧急探索 NPC 进行移动", ref useRedAlertNpc))
            {
                C.UseRedAlertNpc = useRedAlertNpc;
                C.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled("测试版，可能无法正常工作");

            bool avoidStellarReturn = C.AvoidStellarReturn;
            if (ImGui.Checkbox("寻路时避免使用宇宙返回", ref avoidStellarReturn))
            {
                C.AvoidStellarReturn = avoidStellarReturn;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("启用后，寻路将不会使用宇宙返回来前往采集点。\n这同时适用于基地返回和基地 + 以太之光网络的移动方式。");
            }
            if (C.AvoidStellarReturn)
            {
                ImGui.SameLine();
                bool exceptHub = C.AvoidStellarReturnExceptHub;
                if (ImGui.Checkbox("基地活动除外", ref exceptHub))
                {
                    C.AvoidStellarReturnExceptHub = exceptHub;
                    C.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("启用后，宇宙返回仍会用于返回基地，\n以进行信用点购买、抽奖、无人机晶片和修理等活动。");
                }
            }

            var minHubReturnDistance = C.HubReturn_Distance;
            ImGui.SetNextItemWidth(200);
            if (ImGui.DragFloat("使用基地返回前的距离 (yalms)", ref minHubReturnDistance))
            {
                C.HubReturn_Distance = minHubReturnDistance;
                C.SaveDebounced();
            }

            bool DisableRedAlertPathing = C.DisablePathfindingToRedAlert;
            if (ImGui.Checkbox("禁用前往紧急探索的寻路", ref DisableRedAlertPathing))
            {
                C.DisablePathfindingToRedAlert = DisableRedAlertPathing;
                C.Save();
            }

            bool DisableHubActivies_RE = C.DisableHub_Critical;
            if (ImGui.Checkbox("紧急探索激活时不执行基地活动", ref DisableHubActivies_RE))
            {
                C.DisableHub_Critical = DisableHubActivies_RE;
                C.Save();
            }

            bool delayAether = C.Delay_Aethernet;
            if (ImGui.Checkbox("为以太之光 / NPC 移动添加延迟", ref delayAether))
            {
                C.Delay_Aethernet = delayAether;
                C.Save();
            }
            ImGuiEx.HelpMarker("在与以太之光 / 紧急探索 NPC 交互前添加随机延迟。\n" +
                "延迟会出现在交互前，以及与菜单交互之间的一小段时间。");
        }
        private static void StuckSettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.ExclamationTriangle, "卡住检测");
            ImGui.Dummy(new Vector2(0, 5));

            bool unstuckEnabled = C.JumpIfStuck_V2 || C.RetargetIfStuck;
            if (ImGui.Checkbox("寻路移动中卡住时：", ref unstuckEnabled))
            {
                if (unstuckEnabled)
                    C.JumpIfStuck_V2 = true;
                else
                {
                    C.JumpIfStuck_V2 = false;
                    C.RetargetIfStuck = false;
                }
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.HelpMarker(
                "当寻路移动卡住超过设定延迟时：\n" +
                "- 跳跃：尝试跳过障碍物\n" +
                "- 重新寻路：停止并重新寻路到目标（若启用则重新随机）");
            if (!unstuckEnabled) ImGui.BeginDisabled();
            if (ImGui.RadioButton("跳跃", C.JumpIfStuck_V2 && !C.RetargetIfStuck))
            {
                C.JumpIfStuck_V2 = true;
                C.RetargetIfStuck = false;
                C.Save();
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("重新寻路", C.RetargetIfStuck))
            {
                C.RetargetIfStuck = true;
                C.JumpIfStuck_V2 = false;
                C.Save();
            }
            ImGui.SameLine();
            ImGui.Text("延迟");
            ImGui.SameLine();
            int stuckDelay = C.StuckDelayMs;
            ImGui.SetNextItemWidth(100);
            if (ImGui.SliderInt("卡住毫秒###StuckDelay", ref stuckDelay, 500, 3000))
            {
                if (C.StuckDelayMs != stuckDelay)
                {
                    C.StuckDelayMs = stuckDelay;
                    C.SaveDebounced();
                }
            }
            if (!unstuckEnabled) ImGui.EndDisabled();
        }
        private static void CraftingLocations()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.MapPin, "制作返回点");
            ImGui.Dummy(new Vector2(0, 5));

            bool usePersonalLocations = C.PersonalReturnSpot;
            if (ImGui.Checkbox("使用自定义返回点", ref usePersonalLocations))
            {
                C.PersonalReturnSpot = usePersonalLocations;
                C.Save();
            }
            if (usePersonalLocations)
            {
                var territory = Player.Territory.RowId;
                var location = Player.Position;
                ImGui.SameLine();
                if (C.CrafterLocations.TryGetValue(territory, out var moonLoc))
                {
                    if (ImGui.Button("设置为当前位置"))
                    {
                        C.CrafterLocations[territory] = location;
                        C.Save();
                    }
                    ImGui.SameLine();
                    ImGui.Text($"({moonLoc.X:N1}, {moonLoc.Y:N1}, {moonLoc.Z:N1})");
                }
                else
                {
                    if (ImGui.Button("添加位置"))
                    {
                        C.CrafterLocations[territory] = Player.Position;
                        C.Save();
                    }
                    ImGui.SameLine();
                    ImGui.Text("当前未设置位置");
                }
            }
        }
        private static void FishingLocations()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Fish, "自定义钓点");
            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "如果你不想使用插件内置的随机钓点，可以用这种方式保存自己的位置。\n" +
                "你并非必须使用它，符合以下情况时会使用随机钓点：\n" +
                "1：已保存某个位置\n" +
                "2：已保存某个随机点", false);
            ImGui.Dummy(new Vector2(0, 5));

            var currentTerritory = Player.Territory.RowId;

            if (GatheringUtil.MoonFishingLocations.TryGetValue(currentTerritory, out var fishingHoles))
            {
                ImGui.Text($"地图: {Player.Territory.Value.PlaceName.Value.Name}");
                ImGui.Checkbox("显示钓点射线检测", ref _fishingDebug.ShowFishRay);
                if (Player.Object is { } player && _fishingDebug.ShowFishRay)
                {
                    _fishingDebug.Draw();
                }

                ImGui.Separator();

                foreach (var hole in fishingHoles.Keys)
                {
                    // Find existing entry for this zone + map coord, or creating a new one if one doesn't exist
                    var entry = C.Personal_FishLocation.FirstOrDefault(f => f.ZoneId == currentTerritory && f.MapCoords == hole);

                    if (entry == null)
                    {
                        entry = new FishingLocations
                        {
                            ZoneId = currentTerritory,
                            X = hole.X,
                            Y = hole.Y,
                            WorldPosition = null
                        };
                        C.Personal_FishLocation.Add(entry);
                        C.SaveDebounced();
                    }

                    ImGui.PushID($"{hole}_Flag");

                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Flag, $"  X: {hole.X:N2} Y: {hole.Y:N2}"))
                    {
                        var mission = CosmicHelper.SheetMissionDict.Where(x => x.Value.MapPosition == hole).FirstOrDefault();
                        Utils.SetGatheringRing(mission.Value.TerritoryId, (int)hole.X, (int)hole.Y, mission.Value.Radius, $"{hole.X:N2} {hole.Y:N2}");
                    }
                    ImGui.SameLine();

                    string currentPos = entry.WorldPosition == null ? "新增" : $"移除";

                    if (ImGui.Button($"{currentPos}"))
                    {
                        entry.WorldPosition = entry.WorldPosition == null ? Player.Position : null;
                        C.Save();
                    }

                    if (entry.WorldPosition != null)
                    {
                        ImGui.SameLine();
                        ImGui.Text($"{entry.WorldPosition.Value:N2}");
                    }

                    ImGui.PopID();
                }
            }
            else
            {
                ImGui.Text($"当前地图在数据表中没有已存储的钓点。（可能需要添加？）");
            }
        }
    }
}
