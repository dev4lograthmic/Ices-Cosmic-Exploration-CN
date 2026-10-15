using Dalamud.Interface;
using ICE.Utilities.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.Settings
{
    internal class Shop_Dronebit
    {
        public static void Draw()
        {
            if (ImGui.Button("运行无人机搜索"))
            {
                SchedulerMain.State = IceState.ArtifactSearch;
            }

            if (ImGui.Button("停止"))
            {
                SchedulerMain.DisablePlugin();
            }

            bool buyDrones = C.Cosmodrone_Buy;
            if (ImGui.Checkbox("购买无人机", ref buyDrones))
            {
                C.Cosmodrone_Buy = buyDrones;
                C.Save();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, 
                "想要购买无人机吗？如果需要，请启用此选项"
                );

            int drone_buyAtAmount = C.Cosmodrone_BuyAt;
            ImGui.SetNextItemWidth(200);
            if (ImGui.SliderInt("开始购买阈值", ref drone_buyAtAmount, 200, 5000))
            {
                drone_buyAtAmount = (int)Math.Round(drone_buyAtAmount / 200.0) * 200;
                C.Cosmodrone_BuyAt = drone_buyAtAmount;
                C.SaveDebounced();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, 
                "无人机晶片达到多少数量时从商店购买？\n" +
                "以 200 为递增单位，最大 5000"
                );

            int maxCrateAmount = C.Cosmodrone_MaxKeep;
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt("最大持有", ref maxCrateAmount))
            {
                if (maxCrateAmount < 0)
                    maxCrateAmount = 0;
                C.Cosmodrone_MaxKeep = maxCrateAmount;
                C.SaveDebounced();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "你想持有的无人机数量上限是多少？\n" +
                "0 = 持续购买\n" +
                "任何大于 0 的值将作为硬上限，达到此值时停止购买"
                );

            bool runDroneFinder = C.Cosmodrone_Run;
            if (ImGui.Checkbox("自动无人机探索", ref runDroneFinder))
            {
                C.Cosmodrone_Run = runDroneFinder;
                C.Save();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "要运行自动无人机搜索吗？如果需要，请启用此选项\n" +
                "请注意：千万不要放着无人看管！此功能仍在大幅度完善中");
        }
    }
}
