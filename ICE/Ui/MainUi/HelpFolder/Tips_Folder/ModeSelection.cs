using Dalamud.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.HelpFolder.Tips_Folder
{
    internal class ModeSelection
    {
        public static void Draw()
        {
            ImGui.TextWrapped("目前（撰写本文时）共有 5 种模式，各自承担的功能略有不同。");
            ImGui.TextWrapped("取决于你的需求/目标，它们各自承担不同的功能。");

            if (ImGui.BeginTabBar("Mode Selection Info"))
            {
                if (ImGui.BeginTabItem("标准模式"))
                {
                    StandardMode();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("宇宙工具刷取"))
                {
                    RelicGrind();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("练级模式"))
                {
                    LevelingMode();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("金星完成刷取"))
                {
                    GoldCompletion();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("日程模式"))
                {
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private static void StandardMode()
        {
            ImGui.Dummy(new(0, 5));
            ImGuiEx.IconWithText(FontAwesomeIcon.List, "标准模式");
            ImGui.TextWrapped(
                "最直接的模式。标准模式只运行你为当前职业启用的任务 —— " +
                "你选定要做什么，剩下的它来处理。");
            ImGui.TextWrapped("这让你完全掌控要运行哪些任务，非常适合：");
            ImGui.BulletText("技巧点刷取 —— 选择特定的高价值任务");
            ImGui.BulletText("经验刷取");
            ImGui.BulletText("信用点 / 行星信用点 / 票据刷取");
            ImGui.TextWrapped(
                "基础任务（D→A 级）只会取用你起始的职业。" +
                "临时性任务（天气、限时、连续）和紧急探索会在任务之间被挑取 —— " +
                "启用多职业设置即可为这些任务切换职业。");
        }

        private static void RelicGrind()
        {
            ImGui.Dummy(new(0, 5));
            ImGuiEx.IconWithText(FontAwesomeIcon.ArrowUpRightDots, "宇宙工具刷取");

            ImGui.TextWrapped(
                "此模式旨在以最少的人工干预自动选择用于推进宇宙工具的任务。" +
                "它会扫描所有可用任务，评估每个任务提供的研究数据，并挑选" +
                "对当前等级收益最高的那个。");
            ImGui.TextWrapped(
                "如果你的等级已经需要更高一档的任务，但还没解锁" +
                "（例如 D 级已完成但 C 级尚未解锁），请务必在开始前手动解锁。");
            ImGui.TextWrapped("注意：此模式不会替你切换星球。每颗星球的研究数据还有上限：");
            ImGui.BulletText("憧憬湾   —— 最高 IV 级");
            ImGui.BulletText("法恩娜行星 —— 最高 V 级");
            ImGui.BulletText("俄匊斯行星   —— 最高 VI 级");
            ImGui.BulletText("奥克塞西亚行星 —— 最高 VII 级");
            ImGui.TextWrapped("所以请确保你在正确的星球上，以满足你需要的研究数据" +
                "并完成你的宇宙工具。");
        }

        public static void GoldCompletion()
        {
            ImGui.Dummy(new(0, 5));
            ImGuiEx.IconWithText(FontAwesomeIcon.Trophy, "金星完成");

            ImGui.TextWrapped(
                "一种非常直接的模式，目标是让你尚未达成金星的每一个任务都达成金星评价。" +
                "无需逐个选择要做的任务，它会根据" +
                "你当前设置的优先级自动挑选任务。");
            ImGui.TextWrapped("请注意：它没有其它内部逻辑。它无法判断某个任务是否因为" +
                "属性不达标、没有食物等原因而无法完成。它只是最直接的“自动挑选它能做的任务”。" +
                "如果你想掌控“哪些任务你能做”，请选择标准模式并挑选需要完成的任务。");
            ImGui.TextWrapped("此模式也会尊重“是否刷非本职业临时任务”的相关设置");

        }

        public static void LevelingMode()
        {
            ImGui.Dummy(new(0, 5));
            ImGuiEx.IconWithText(FontAwesomeIcon.Leaf, "练级模式");

            ImGui.TextWrapped("此模式旨在挑选既提供最多经验、又能最快完成的任务。" +
                "这些任务都以铜星评价完成（即最快完成任务的方式），" +
                "因为经验不会随汇报评价而变化。也就是说，如果某个任务给 125% 经验，它就一直给这么多。");

            ImGui.Dummy(new(0, 5));
            ImGui.Text("能工巧匠");
            ImGui.BulletText("优先选择制作进展需求最低的任务");
            ImGui.BulletText("品质对这些任务没有影响");
            ImGui.BulletText("如果你还没有解锁收藏品，需要在 50 级时前往摩杜纳解锁");
            ImGui.BulletText("如果你像我一样想省一点，可以在以下等级再更新装备：");
            ImGui.BulletText("10 级 -> 35 -> 52 -> 80");
            ImGui.BulletText("这是我实测够用的节点；如果你想更快，中间当然可以更频繁地更新装备。");

            ImGui.Dummy(new(0, 5));
            ImGui.Text("大地使者");
            ImGui.BulletText("稍微繁琐一些，也绝不是最快的，但到目前为止最安全。");
            ImGui.BulletText("捕鱼人的配置已内置在插件中，园艺工/采矿工会自动设置配置以便尽快汇报任务");
            ImGui.BulletText("这里比能工巧匠更需要频繁更新装备：50 级前约每 5-7 级一次，之后约每 3 级一次");
        }

        public static void CosmicAgenda()
        {
            ImGui.Dummy(new(0, 5));
            ImGuiEx.IconWithText(FontAwesomeIcon.ClipboardList, "宇宙日程 | 日程模式");

            ImGui.TextWrapped("此模式把（大多数）其它模式整合成一个播放列表，让你设置好就可以放着不管。" +
                "它的目的是让你安排做事的时间和顺序。");
            ImGui.TextWrapped("包括但不限于：");
            ImGui.BulletText("把选定职业练到指定等级");
            ImGui.BulletText("把特定职业的技巧点刷到 50 万");
            ImGui.BulletText("完成所有职业的宇宙工具");
            ImGui.TextWrapped("你可以为每项指定使用的模式，它会持续尝试该职业直到目标完成。" +
                "它会尊重你当前启用的所有设置，只是让你更灵活地选择要做什么。");
        }
    }
}
