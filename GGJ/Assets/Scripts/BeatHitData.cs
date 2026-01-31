using UnityEngine;

/// <summary>
/// 节拍事件数据
/// </summary>
public class BeatHitData
{
    public int BeatIndex;       // 节拍索引
    public int Round;           // 回合数

    public BeatHitData(int beatIndex, int round)
    {
        BeatIndex = beatIndex;
        Round = round;
    }
}
