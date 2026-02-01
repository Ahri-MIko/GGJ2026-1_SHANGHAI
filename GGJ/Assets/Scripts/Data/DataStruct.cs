using UnityEngine;
using System;
using System.Collections.Generic;

// --- 数据模型 ---

[Serializable]
public class StageData
{
    public int BarId;
    public int BossBeat;      // 对应 Python 中新增的 BossBeat
    public int BossAEvent;
    public int[] PlayerBeat;
    public int[] PlayerEvent;
    public string Note;
}

[Serializable]
public class EventData
{
    public string EventId;
    public string Note;
    public string ShowMessage;
    public int EventType;
    public string SoundFile;  // 新增：对应 Python 中的 SoundFile 属性
}

// --- JsonUtility 包装容器 ---

[Serializable] public class StageList { public List<StageData> items; }
[Serializable] public class EventList { public List<EventData> items; }

// --- 加载器主体 ---

public static class DataLoader
{
    // 辅助工具：包装 JSON 数组，使其符合 JsonUtility 的解析要求
    private static string WrapJson(string json) => "{\"items\":" + json + "}";

    /// <summary>
    /// 加载指定关卡的 Stage 数据 (例如传入 1 加载 Resources/Stage1.json)
    /// </summary>
    public static List<StageData> LoadStageData(int stageId)
    {
        string fileName = "Stage" + stageId;
        TextAsset asset = Resources.Load<TextAsset>(fileName);
        if (asset == null)
        {
            Debug.LogError($"❌ [DataLoader] 找不到文件: Resources/{fileName}.json");
            return new List<StageData>();
        }
        return JsonUtility.FromJson<StageList>(WrapJson(asset.text)).items;
    }

    /// <summary>
    /// 加载通用的事件表 (Resources/Event.json)
    /// </summary>
    public static List<EventData> LoadEventData()
    {
        TextAsset asset = Resources.Load<TextAsset>("Event");
        if (asset == null)
        {
            Debug.LogError("❌ [DataLoader] 找不到文件: Resources/Event.json");
            return new List<EventData>();
        }
        return JsonUtility.FromJson<EventList>(WrapJson(asset.text)).items;
    }
}