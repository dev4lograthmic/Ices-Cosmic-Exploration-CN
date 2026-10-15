using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings
{
    internal class Shop_Credits
    {
        private static string ItemSearch = string.Empty;
        private static ImGuiEx.RealtimeDragDrop<uint> MaterialDragDrop = new("MaterialShop", (id) => id.ToString());
        private static ImGuiEx.RealtimeDragDrop<uint> GearDragDrop = new("GearShop", (id) => id.ToString());

        public static unsafe void Draw()
        {
            float minContentWidth = 920 * ImGuiHelpers.GlobalScale;
            var availWidth = ImGui.GetContentRegionAvail().X;
            if (availWidth < minContentWidth)
                ImGui.SetNextWindowContentSize(new Vector2(minContentWidth, 0));

            using var scrollChild = ImRaii.Child("##shoppingTabScroll", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);
            if (!scrollChild.Success) return;

            bool BuyItems = C.BuyItems;

            if (ImGui.Checkbox("购买物品", ref BuyItems))
            {
                C.BuyItems = BuyItems;
                C.StopOnceHitCosmoCredits = false;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("这是您的个性化自定义购物清单，当您的宇宙信用点达到指定数量时将会进行购物消费。");
                ImGui.Text("以下是每个选项的功能说明：");
                ImGui.BulletText("目标库存：将购买物品直到您的物品栏中拥有设定数量的物品，这个数量不会在每次运行之间减少。\n" +
                                 "适用于像强心剂这类您希望总是持有一定数量的物品。");
                ImGui.BulletText("购买数量：将购买 X 个此物品，当您从商人处购买时，这个数量会减少，直到变成 0。\n" +
                                 "适用于一次性购买，或者您只需要特定数量的物品。");
                ImGui.BulletText("持续购买：在满足前 2 个条件（目标库存/购买数量）后，如果有足够的宇宙信用点，将持续购买此物品。\n" +
                                 "只能对 1 个物品进行设置，通常适用于那些您只是想把宇宙信用点花掉的物品。");
                ImGui.EndTooltip();
            }
            ImGui.NewLine();

            int buyAtAmount = C.CosmoBuyAtAmount;
            int CosmoKeepAmount = C.CosmoKeepAmount;

            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("开始购物阈值", ref buyAtAmount, 0, 30000))
            {
                C.CosmoBuyAtAmount = buyAtAmount;
                C.SaveDebounced();
            }

            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("保留宇宙信用点数量", ref CosmoKeepAmount, 0, buyAtAmount))
            {
                C.CosmoKeepAmount = CosmoKeepAmount;
                C.SaveDebounced();
            }

            CheckConfigState();
            if (Task_BuyCosmoItems.CanPurchaseAnyItem())
            {
                ImGui.Text("您现在可以购买列表中的宇宙信用点物品！");
            }
            else
            {
                ImGui.Text("您当前的宇宙信用点/物品不足以购买任何商品（好吧，这只是测试）");
            }

            if (ImGui.Button("添加材料/染剂/物品"))
            {
                ImGui.OpenPopup("CosmocreditMateriaPopup");
            }

            ImGui.SameLine();

            if (ImGui.Button("添加防具/房屋/坐骑"))
            {
                ImGui.OpenPopup("Cosmocredit_MountArmorPopup");
            }

            ImGui.SameLine();
            
            if (ImGui.Button("清空购物清单"))
            {
                C.CosmoShopping.Clear();
                C.CosmoShoppingOrder.Clear();
                C.CosmoShoppingOrder_Gear.Clear();

                C.Save();
            }

            DrawAddItemPopups();

            ImGui.Separator();
            ImGui.NewLine();

            DrawShoppingTable("防具/房屋/坐骑", Shop_Cosmocredits.Shop_MountsCards, C.CosmoShoppingOrder_Gear, GearDragDrop);

            // Draw separate tables for each shop type

            ImGui.NewLine();

            DrawShoppingTable("材料/染剂/物品", Shop_Cosmocredits.Shop_MateriaDye, C.CosmoShoppingOrder, MaterialDragDrop);
        }

        private static void DrawAddItemPopups()
        {
            ImGui.SetNextWindowSize(new Vector2(400, 0), ImGuiCond.Appearing);

            if (ImGui.BeginPopup("CosmocreditMateriaPopup"))
            {
                ImGui.SetNextItemWidth(380);
                ImGui.InputText("##Item Search", ref ItemSearch, 256);

                ImGui.Spacing();

                if (ImGui.BeginTable("Cosmo Materia Shop", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg, new Vector2(0, 250)))
                {
                    ImGui.TableSetupColumn("Icons", ImGuiTableColumnFlags.WidthFixed, 20);
                    ImGui.TableSetupColumn("Names", ImGuiTableColumnFlags.WidthStretch);

                    foreach (var item in Shop_Cosmocredits.Shop_MateriaDye)
                    {
                        DrawShopItemRow(item.Key, C.CosmoShoppingOrder);
                    }
                    ImGui.EndTable();
                }

                ImGui.EndPopup();
            }

            if (ImGui.BeginPopup("Cosmocredit_MountArmorPopup"))
            {
                ImGui.SetNextItemWidth(380);
                ImGui.InputText("##Item Search2", ref ItemSearch, 256);

                ImGui.Spacing();

                if (ImGui.BeginTable("Cosmo Gear Shop", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg, new Vector2(0, 250)))
                {
                    ImGui.TableSetupColumn("Icons", ImGuiTableColumnFlags.WidthFixed, 20);
                    ImGui.TableSetupColumn("Names", ImGuiTableColumnFlags.WidthStretch);

                    foreach (var item in Shop_Cosmocredits.Shop_MountsCards)
                    {
                        DrawShopItemRow(item.Key, C.CosmoShoppingOrder_Gear);
                    }
                    ImGui.EndTable();
                }

                ImGui.EndPopup();
            }
        }

        private static void DrawShopItemRow(uint id, List<uint> orderList)
        {
            if (Svc.Data.GetExcelSheet<Item>().TryGetRow(id, out var itemInfo))
            {
                var name = itemInfo.Name.ToString();

                if (!ItemSearch.IsNullOrWhitespace() && !name.ToLower().Contains(ItemSearch.ToLower()))
                {
                    return;
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.PushID(id);

                if (itemInfo.Icon is { } itemIcon && Svc.Texture.TryGetFromGameIcon((int)itemIcon, out var texture))
                {
                    ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(20, 20));
                }

                ImGui.TableNextColumn();
                ImGui.Text($"{itemInfo.Name}");

                if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    AddItemToList(id, orderList);
                    C.Save();
                }

                ImGui.PopID();
            }
        }

        private static void DrawShoppingTable(string tableName, Dictionary<uint, Shop_Cosmocredits.ItemInfo> shopData, List<uint> orderList, ImGuiEx.RealtimeDragDrop<uint> dragDrop)
        {
            if (orderList.Count == 0)
            {
                ImGui.TextDisabled($"{tableName} 购物清单中没有物品");
                return;
            }

            ImGui.Text($"{tableName}（{orderList.Count} 个物品）");

            dragDrop.Begin();

            if (ImGui.BeginTable($"Shopping_{tableName}", 10, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("顺序", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("名称");
                ImGui.TableSetupColumn("持有", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("价格", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("类型", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("已解锁", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("目标库存", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("购买数量", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("持续购买", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableHeadersRow();

                for (int i = 0; i < orderList.Count; i++)
                {
                    uint itemId = orderList[i];
                    DrawShoppingItemRow(itemId, shopData, i, orderList, dragDrop);
                }

                ImGui.EndTable();
            }

            dragDrop.End();
        }

        private static void DrawShoppingItemRow(uint itemId, Dictionary<uint, Shop_Cosmocredits.ItemInfo> shopData, int index, List<uint> orderList, ImGuiEx.RealtimeDragDrop<uint> dragDrop)
        {
            var setting = C.CosmoShopping[itemId];
            var itemInfo = Svc.Data.GetExcelSheet<Item>().GetRow(itemId);

            ImGui.TableNextRow();
            dragDrop.NextRow();
            dragDrop.SetRowColor(itemId);

            ImGui.PushID(itemId);

            // Drag/Drop Handle - MUCH SIMPLER NOW!
            ImGui.TableSetColumnIndex(0);
            dragDrop.DrawButtonDummy(itemId, orderList, index);

            // Name
            ImGui.TableNextColumn();
            if (itemInfo.Icon is { } itemIcon && Svc.Texture.TryGetFromGameIcon((int)itemIcon, out var texture))
            {
                ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                ImGui.SameLine();
            }
            ImGui.Text($"{itemInfo.Name}");

            // Have
            ImGui.TableNextColumn();
            PlayerHelper.GetItemCount(itemId, out var count);
            ImGui.Text($"{count}");

            // Cost
            ImGui.TableNextColumn();
            if (shopData.TryGetValue(itemId, out var shopInfo))
            {
                ImGui.Text($"{shopInfo.Cost:N0}");
            }

            // Kind
            ImGui.TableNextColumn();
            string kind = itemInfo.ItemUICategory.Value.Name.ToString();
            ImGui.Text(kind);

            // Unlocked (for consumable items like mounts, orchestrion rolls, cards, etc.)
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(UnlockState.IsItemUnlockable(itemInfo) ? UnlockState.IsItemUnlocked(itemInfo) ? "是" : "否" : "-");

            // Keep Amount
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(80);
            var keepAmount = setting.KeepAmount;
            if (ImGui.InputInt($"##keep_{itemId}", ref keepAmount))
            {
                setting.KeepAmount = Math.Max(0, keepAmount);
                C.SaveDebounced();
            }

            // Buy Amount
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(80);
            var buyAmount = setting.BuyAmount;
            if (ImGui.InputInt($"##buy_{itemId}", ref buyAmount))
            {
                setting.BuyAmount = Math.Max(0, buyAmount);
                C.SaveDebounced();
            }

            // Keep Buying
            ImGui.TableNextColumn();
            var keepBuying = setting.KeepBuying;
            if (ImGui.Checkbox($"##keepbuying_{itemId}", ref keepBuying))
            {
                foreach (var enabled in C.CosmoShopping)
                {
                    enabled.Value.KeepBuying = false;
                }

                setting.KeepBuying = keepBuying;
                C.Save();
            }

            // Remove Button
            ImGui.TableNextColumn();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"##remove_{itemId}"))
            {
                RemoveItem(itemId, orderList);
                C.Save();
            }

            ImGui.PopID();
        }

        private static void AddItemToList(uint itemId, List<uint> orderList)
        {
            if (C.CosmoShopping.ContainsKey(itemId))
                return;

            C.CosmoShopping[itemId] = new CosmoShoppingList();
            orderList.Add(itemId);
        }

        private static void RemoveItem(uint itemId, List<uint> orderList)
        {
            C.CosmoShopping.Remove(itemId);
            orderList.Remove(itemId);
        }

        public static void CheckConfigState()
        {
            if (C.CosmoShopping == null)
            {
                C.CosmoShopping = new();
                C.Save();
            }
            if (C.CosmoShoppingOrder == null)
            {
                C.CosmoShoppingOrder = new();
                C.Save();
            }
            if (C.CosmoShoppingOrder_Gear == null)
            {
                C.CosmoShoppingOrder_Gear = new();
                C.Save();
            }
        }
    }
}