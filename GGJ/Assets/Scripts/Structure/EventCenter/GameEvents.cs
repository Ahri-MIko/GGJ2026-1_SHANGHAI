using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents
{
    /// <summary>
    /// 节拍事件 - 当TimeBar到达每个节拍点时触发
    /// 参数：BeatHitData - 包含节拍索引和回合数
    /// </summary>
    public const string OnBeatHit = "OnBeatHit";
    
    /// <summary>
    /// 玩家输入判定事件 - 当玩家输入被判定时触发
    /// 参数：JudgementResult - 判定结果
    /// </summary>
    public const string OnPlayerJudgement = "OnPlayerJudgement";


    /// <summary>
    /// 玩家输入后的判定事件-分为 Perfect-Great-good
    /// 参数：int 
    /// </summary>
    /// 
    public const string OnDoDMG = "OnDoDMG"; //对地方造成伤害
    public const string OnCure = "OnCure"; //自身回复生命值
    
    /// <summary>
    /// 播放语音事件
    /// 参数：string - 语音名称/ID
    /// </summary>
    public const string OnPlayVoice = "OnPlayVoice";
    
    /// <summary>
    /// 显示表情包事件
    /// 参数：string - 表情包类型（如 "PerfectReply", "NormalEmoji", "HurtChaos" 等）
    /// </summary>
    public const string OnShowEmoji = "OnShowEmoji";
}
