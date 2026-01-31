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
        Debug.Log($"收到判定事件: {result.Level}, 偏差: {result.DeviationMs:F2}ms, 按键: {result.InputKey}");
        
        // 在这里添加你的判定响应逻辑
        // 例如：
        switch (result.Level)
        {
            case JudgementLevel.Perfect:
                // 播放Perfect音效，加分等
                break;
            case JudgementLevel.Great:
                // 播放Great音效，加分等
                break;
            case JudgementLevel.Good:
                // 播放Good音效，加分等
                break;
            case JudgementLevel.Miss:
                // 播放Miss音效，扣分等
                break;
        }
    }
}
