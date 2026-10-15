using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Ui.MainUi.HelpFolder.Tips_Folder;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.HelpFolder
{
    internal class helpSelect_Tips
    {
        public enum Help_Selection
        {
            Welcome,
            ModeSelection,
            AgendaMode, 

        }

        private static Help_Selection selectedMode = Help_Selection.Welcome;

        private static string EnumString(Help_Selection tipSelected)
        {
            return tipSelected switch
            {
                Help_Selection.Welcome => "欢迎",
                Help_Selection.ModeSelection => "模式选择",
                Help_Selection.AgendaMode => "宇宙日程",
                _ => tipSelected.ToString()
            };
        }

        private static readonly Dictionary<Help_Selection, Action> TipViews = new()
        {
            [Help_Selection.Welcome] = () => Welcome.Draw(),
            [Help_Selection.ModeSelection] = () => ModeSelection.Draw(),
        };

        public static void Draw()
        {
            float spacing = 10f;
            float leftPanelWidth = 200f;
            float rightPanelWidth = ImGui.GetContentRegionAvail().X - leftPanelWidth - spacing;
            float childHeight = ImGui.GetContentRegionAvail().Y;

            if (ImGui.BeginChild("Tip Selector", new Vector2(leftPanelWidth, childHeight), true))
            {
                foreach (Help_Selection tip in Enum.GetValues<Help_Selection>())
                {
                    bool isSelected = tip == selectedMode;
                    if (ImGui.Selectable($"{EnumString(tip)}", isSelected))
                    {
                        selectedMode = tip;
                    }
                }
            }
            ImGui.EndChild();

            ImGui.SameLine(0, spacing);
            if (ImGui.BeginChild("DebugContent", new System.Numerics.Vector2(rightPanelWidth, childHeight), true))
            {
                if (TipViews.TryGetValue(selectedMode, out var drawAction))
                {
                    drawAction();
                }
                else
                {
                    ImGui.Text("未知提示视图");
                }
            }
            ImGui.EndChild();
        }

        private static void ScoreMax()
        {
            ImGui.TextWrapped("每颗星球都有一套被认为最适合刷取技巧点的“最优”任务。" +
                "\n某些任务比其他任务更值得刷取。天气/时间也会产生影响。以下是我按职业推荐的内容。");
            ImGui.TextWrapped("");
        }
    }
}
