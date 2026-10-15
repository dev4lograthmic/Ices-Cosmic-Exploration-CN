using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.HelpFolder.Tips_Folder
{
    internal class Welcome
    {
        public static void Draw()
        {
            ImGui.TextWrapped($"欢迎！这大概是我目前做过的最复杂的插件了。");
            ImGui.TextWrapped($"此插件专为宇宙探索设计，内容也相当庞大。所以我会尽量把所有不同的技巧/提示都讲一遍。");

            ImGui.Dummy(new(0, 5));

            ImGui.TextWrapped("侧边栏有几个不同的分页，会*尽力*解答你可能遇到的任何问题。");
            ImGui.TextWrapped("请务必查看“插件需求”页面，了解每个功能分别需要什么");
            ImGui.TextWrapped("或者直接阅读对应的分页。应该能解答你不少疑问");
        }
    }
}
