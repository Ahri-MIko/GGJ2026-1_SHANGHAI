using UnityEngine;

/// <summary>
/// 关卡结束数据
/// </summary>
public class StageEndData
{
    public int PerfectCount;    // Perfect 次数
    public int GreatCount;      // Great 次数
    public int MissCount;       // Miss 次数（包含 Good）

    public StageEndData(int perfectCount, int greatCount, int missCount)
    {
        PerfectCount = perfectCount;
        GreatCount = greatCount;
        MissCount = missCount;
    }

    /// <summary>
    /// 获取总判定次数
    /// </summary>
    public int GetTotalCount()
    {
        return PerfectCount + GreatCount + MissCount;
    }

    /// <summary>
    /// 获取准确率（Perfect + Great）/ 总数
    /// </summary>
    public float GetAccuracy()
    {
        int total = GetTotalCount();
        if (total == 0) return 0f;
        return (float)(PerfectCount + GreatCount) / total * 100f;
    }

    /// <summary>
    /// 获取 Perfect 率
    /// </summary>
    public float GetPerfectRate()
    {
        int total = GetTotalCount();
        if (total == 0) return 0f;
        return (float)PerfectCount / total * 100f;
    }
}
