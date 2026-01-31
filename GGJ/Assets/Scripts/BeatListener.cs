using UnityEngine;

/// <summary>
/// 节拍监听器示例 - 展示如何监听节拍事件和玩家判定事件
/// </summary>
public class BeatListener : MonoBehaviour
{
    private void Start()
    {
        // 注册节拍事件监听
        EventCenter.Instance.AddEventListener<BeatHitData>(GameEvents.OnBeatHit, OnBeatReceived);
        
        // 注册玩家判定事件监听
        EventCenter.Instance.AddEventListener<JudgementResult>(GameEvents.OnPlayerJudgement, OnPlayerJudgement);
    }

    private void OnDestroy()
    {
        // 注销节拍事件监听
        EventCenter.Instance.RemoveEventListener<BeatHitData>(GameEvents.OnBeatHit, OnBeatReceived);
        
        // 注销玩家判定事件监听
        EventCenter.Instance.RemoveEventListener<JudgementResult>(GameEvents.OnPlayerJudgement, OnPlayerJudgement);
    }

    /// <summary>
    /// 接收到节拍事件的回调
    /// </summary>
    /// <param name="beatData">节拍数据（包含节拍索引和回合数）</param>
    private void OnBeatReceived(BeatHitData beatData)
    {
        //需要节拍触发时间
    }

    /// <summary>
    /// 接收到玩家判定事件的回调
    /// </summary>
    /// <param name="result">判定结果</param>
    private void OnPlayerJudgement(JudgementResult result)
    {
        string keyCode = result.InputKey;
        
        // 显示UI反馈
        switch (result.Level)
        {
            case JudgementLevel.Perfect:
                UIManager.Instance.ShowHitFeedback(2);
                break;
            case JudgementLevel.Great:
                UIManager.Instance.ShowHitFeedback(1);
                break;
            case JudgementLevel.Good:
                UIManager.Instance.ShowHitFeedback(0);
                break;
            case JudgementLevel.Miss:
                UIManager.Instance.ShowHitFeedback(0);
                break;
        }
        
        // 根据按键确定 ActionType
        ActionType currentAction = GetActionTypeFromKey(keyCode);
        
        // 将判定等级映射到质量
        Quality quality = MapJudgementToQuality(result.Level);
        
        // 触发语音和表情包事件
        TriggerVoiceAndEmoji(currentAction, quality, result.Category);
        
        // 从配置中获取效果
        if (GameConfig.Data.TryGetValue(currentAction, out var categoryDict))
        {
            if (categoryDict.TryGetValue(result.Category, out var qualityDict))
            {
                if (qualityDict.TryGetValue(quality, out Effect effect))
                {
                    // 触发伤害事件
                    if (effect.EnemyDamage > 0)
                    {
                        EventCenter.Instance.EventTrigger(GameEvents.OnDoDMG, effect.EnemyDamage);
                        Debug.Log($"对敌方造成 {effect.EnemyDamage} 点伤害 (按键: {keyCode}, 行为: {currentAction})");
                    }
                    
                    // 触发治疗/受伤事件
                    if (effect.SelfDamage != 0)
                    {
                        if (effect.SelfDamage < 0)
                        {
                            // 负数表示回复
                            int healAmount = -effect.SelfDamage;
                            EventCenter.Instance.EventTrigger(GameEvents.OnCure, healAmount);
                            Debug.Log($"自身回复 {healAmount} 点生命值 (按键: {keyCode}, 行为: {currentAction})");
                        }
                        else
                        {
                            // 正数表示受伤
                            EventCenter.Instance.EventTrigger(GameEvents.OnCure, -effect.SelfDamage);
                            Debug.Log($"自身受到 {effect.SelfDamage} 点伤害 (按键: {keyCode}, 行为: {currentAction})");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 根据按键获取对应的 ActionType
    /// Q - 技术回怼
    /// W - 已读乱回
    /// E - 沉默吃饭
    /// </summary>
    private ActionType GetActionTypeFromKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return (ActionType)0; // 默认返回第一个枚举值
        }
        
        string upperKey = key.ToUpper();
        switch (upperKey)
        {
            case "Q":
                return (ActionType)0; // 技术回怼
            case "W":
                return (ActionType)1; // 已读乱回
            case "E":
                return (ActionType)2; // 沉默吃饭
            default:
                return (ActionType)0; // 默认技术回怼
        }
    }
    
    /// <summary>
    /// 将判定等级映射到质量
    /// </summary>
    private Quality MapJudgementToQuality(JudgementLevel level)
    {
        switch (level)
        {
            case JudgementLevel.Perfect:
                return Quality.Perfect;
            case JudgementLevel.Great:
            case JudgementLevel.Good:
                return Quality.Normal;
            case JudgementLevel.Miss:
                return Quality.Fail;
            default:
                return Quality.Fail;
        }
    }
    
    /// <summary>
    /// 触发语音和表情包事件
    /// </summary>
    private void TriggerVoiceAndEmoji(ActionType action, Quality quality, Category category)
    {
        string voiceId = "";
        string emojiType = "";
        
        // 根据行为类型、质量和类别确定语音和表情包
        switch (action)
        {
            case ActionType _ when (int)action == 0: // Q - 技术回怼
                if (category == Category.Specific)
                {
                    // 针对具体语音
                    switch (quality)
                    {
                        case Quality.Perfect:
                            voiceId = "Q_Specific_Perfect";
                            emojiType = "PerfectReply";
                            break;
                        case Quality.Normal:
                            voiceId = "Q_Specific_Normal";
                            emojiType = "NormalReply";
                            break;
                        case Quality.Fail:
                            voiceId = "Q_Specific_Fail";
                            emojiType = "HurtReply";
                            break;
                    }
                }
                else // Generic
                {
                    // 普通表情包
                    switch (quality)
                    {
                        case Quality.Perfect:
                            voiceId = "Q_Generic_Perfect";
                            emojiType = "PerfectEmoji";
                            break;
                        case Quality.Normal:
                            voiceId = "Q_Generic_Normal";
                            emojiType = "NormalEmoji";
                            break;
                        case Quality.Fail:
                            voiceId = "Q_Generic_Fail";
                            emojiType = "HurtEmoji";
                            break;
                    }
                }
                break;
                
            case ActionType _ when (int)action == 1: // W - 已读乱回
                if (category == Category.Specific)
                {
                    switch (quality)
                    {
                        case Quality.Perfect:
                            voiceId = "W_Specific_Perfect";
                            emojiType = "PerfectChaos";
                            break;
                        case Quality.Normal:
                            voiceId = "W_Specific_Normal";
                            emojiType = "NormalChaos";
                            break;
                        case Quality.Fail:
                            voiceId = "W_Specific_Fail";
                            emojiType = "HurtChaos";
                            break;
                    }
                }
                else
                {
                    switch (quality)
                    {
                        case Quality.Perfect:
                            voiceId = "W_Generic_Perfect";
                            emojiType = "PerfectChaosEmoji";
                            break;
                        case Quality.Normal:
                            voiceId = "W_Generic_Normal";
                            emojiType = "NormalChaosEmoji";
                            break;
                        case Quality.Fail:
                            voiceId = "W_Generic_Fail";
                            emojiType = "HurtChaosEmoji";
                            break;
                    }
                }
                break;
                
            case ActionType _ when (int)action == 2: // E - 沉默吃饭
                if (category == Category.Specific)
                {
                    switch (quality)
                    {
                        case Quality.Perfect:
                            voiceId = "E_Specific_Perfect";
                            emojiType = "PerfectEat";
                            break;
                        case Quality.Normal:
                            voiceId = "E_Specific_Normal";
                            emojiType = "NormalEat";
                            break;
                        case Quality.Fail:
                            voiceId = "E_Specific_Fail";
                            emojiType = "HurtEat";
                            break;
                    }
                }
                else
                {
                    switch (quality)
                    {
                        case Quality.Perfect:
                            voiceId = "E_Generic_Perfect";
                            emojiType = "PerfectEatEmoji";
                            break;
                        case Quality.Normal:
                            voiceId = "E_Generic_Normal";
                            emojiType = "NormalEatEmoji";
                            break;
                        case Quality.Fail:
                            voiceId = "E_Generic_Fail";
                            emojiType = "HurtEatEmoji";
                            break;
                    }
                }
                break;
        }
        
        // 触发语音事件
        if (!string.IsNullOrEmpty(voiceId))
        {
            EventCenter.Instance.EventTrigger(GameEvents.OnPlayVoice, voiceId);
            Debug.Log($"播放语音: {voiceId}");
        }
        
        // 触发表情包事件
        if (!string.IsNullOrEmpty(emojiType))
        {
            EventCenter.Instance.EventTrigger(GameEvents.OnShowEmoji, emojiType);
            Debug.Log($"显示表情包: {emojiType}");
        }
    }
}
