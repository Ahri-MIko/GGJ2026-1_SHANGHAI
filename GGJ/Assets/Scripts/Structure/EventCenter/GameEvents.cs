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
}
