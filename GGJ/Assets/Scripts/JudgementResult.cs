using UnityEngine;

/// <summary>
/// 判定等级
/// </summary>
public enum JudgementLevel
{
    Miss,       // 未命中
    Good,       // 良好
    Great,      // 很好
    Perfect     // 完美
}

/// <summary>
/// 判定结果数据
/// </summary>
public class JudgementResult
{
    public JudgementLevel Level;        // 判定等级
    public int BeatIndex;               // 节拍索引
    public double Deviation;            // 偏差时间（秒）
    public double DeviationMs;          // 偏差时间（毫秒）
    public string InputKey;             // 触发的按键
    public Category Category;           // 是否是特定语音（Specific）还是普通（Generic）
    
    public JudgementResult(JudgementLevel level, int beatIndex, double deviation, string inputKey, Category category = Category.Generic)
    {
        Level = level;
        BeatIndex = beatIndex;
        Deviation = deviation;
        DeviationMs = deviation * 1000;
        InputKey = inputKey;
        Category = category;
    }
}
