using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Utilities.Cosmic_Helper;

public static unsafe partial class CosmicHelper
{
    public static string PlaylistOptionString(PlaylistOptions option)
    {
        if (CosmicMoonRegistry.TryGetMoonForMaxRelicOption(option, out var moon))
            return $"{moon.DisplayName} 宇宙工具满级 [等级 {moon.MaxRelicStage}]";

        return option switch
        {
            PlaylistOptions.None => "无",
            PlaylistOptions.SelectedRelicLv => "指定宇宙工具等级",
            PlaylistOptions.CreditAmount => "宇宙信用点数量",
            PlaylistOptions.PlanetAmount => "行星信用点数量",
            PlaylistOptions.DronebitAmount => "无人机晶片数量",
            PlaylistOptions.ClassLevel => "职业等级",
            PlaylistOptions.ClassScore => "职业技巧点",
            PlaylistOptions.GoldClassMissions => "全部任务达成金星",
            PlaylistOptions.ToolMaxExp => "宇宙工具满级经验",
            PlaylistOptions.MasteryScore => "熟练点",
            _ => "???"
        };
    }
}
