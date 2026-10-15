using Dalamud.Interface;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ICE.Ui.MainUi.Settings
{
    internal class Shop_Tokens
    {
        public static void Draw()
        {
            var enableBooklet = C.BookletBuy_Enable;
            var bookletAmount = C.BookletBuy_Amount;

            var enableMountBuy = C.PlanetMount_Enable;
            var mountBuyAmount = C.PlanetMount_Amount;

            if (ImGui.Checkbox("兑换探索计划证书", ref enableBooklet))
            {
                C.BookletBuy_Enable = enableBooklet;
                C.SaveDebounced();
            }
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt("兑换探索计划证书 @", ref bookletAmount))
            {
                if (bookletAmount > 99)
                {
                    C.BookletBuy_Amount = bookletAmount;
                    C.SaveDebounced();
                }
            }

            if (ImGui.Checkbox("兑换坐骑", ref enableMountBuy))
            {
                C.PlanetMount_Enable = enableMountBuy;
                C.SaveDebounced();
            }
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt("兑换坐骑 @", ref mountBuyAmount))
            {
                if (mountBuyAmount > 59)
                {
                    C.PlanetMount_Amount = mountBuyAmount;
                    C.SaveDebounced();
                }
            }

            if (ImGui.BeginTable("Mount Token Info", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("行星");
                ImGui.TableSetupColumn("票据");
                ImGui.TableSetupColumn("探索计划证书");
                ImGui.TableSetupColumn("坐骑");
                ImGui.TableSetupColumn("已解锁");

                ImGui.TableHeadersRow();

                foreach (var entry in CosmicMoonRegistry.TokenIds)
                {
                    var planet = CosmicMoonRegistry.ByTerritoryId[entry.Key];
                    var planetIcon = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), planet.IconResource).GetWrapOrEmpty();

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui_Ice.ImageButtonWithText(planetIcon, planet.DisplayName, planet.DisplayName, new(24, 24));

                    ImGui.TableNextColumn();
                    if (ExcelHelper.ItemSheet.TryGetRow(entry.Value.tokenId, out var tokenSheet))
                    {
                        if (Svc.Texture.TryGetFromGameIcon((int)tokenSheet.Icon, out var icon) && PlayerHelper.GetItemCount(entry.Value.tokenId, out var count))
                        {
                            ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), $"{count:N0}", $"{tokenSheet.Name}", new(24));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text($"物品 ID: {entry.Value.tokenId}");
                                ImGui.Text($"名称: {tokenSheet.Name}");
                                ImGui.EndTooltip();
                            }
                        }
                    }

                    ImGui.TableNextColumn();
                    if (ExcelHelper.ItemSheet.TryGetRow(entry.Value.bookletId, out var bookletSheet))
                    {
                        if (Svc.Texture.TryGetFromGameIcon((int)bookletSheet.Icon, out var icon) && PlayerHelper.GetItemCount(entry.Value.bookletId, out var count))
                        {
                            ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), $"{count:N0}", $"{bookletSheet.Name}", new(24));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text($"物品 ID: {entry.Value.bookletId}");
                                ImGui.Text($"名称: {bookletSheet.Name}");
                                ImGui.EndTooltip();
                            }
                        }
                    }

                    ImGui.TableNextColumn();
                    if (ExcelHelper.ItemSheet.TryGetRow(entry.Value.mountId, out var mountSheet))
                    {
                        if (Svc.Texture.TryGetFromGameIcon((int)mountSheet.Icon, out var icon) && PlayerHelper.GetItemCount(entry.Value.mountId, out var count))
                        {
                            ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), $"{count:N0}", $"{mountSheet.Name}", new(24));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text($"物品 ID: {entry.Value.mountId}");
                                ImGui.Text($"名称: {mountSheet.Name}");
                                ImGui.EndTooltip();
                            }
                        }
                    }

                    ImGui.TableNextColumn();
                    bool unlocked = UnlockState.IsItemUnlocked(mountSheet);
                    var fontIcon = unlocked ? FontAwesomeIcon.Check : FontAwesomeIcon.Times;
                    var color = unlocked ? EColor.Green : EColor.Red;
                    ImGuiEx.Icon(color, fontIcon);
                }

                ImGui.EndTable();
            }

        }
    }
}
