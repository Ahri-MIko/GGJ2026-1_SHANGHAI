using UnityEngine;
using System;
using System.Collections.Generic;

// --- 数据模型 ---

/// <summary>
/// 辅助类：用于绕过 JsonUtility 不支持二维数组 (int[][]) 的限制
/// </summary>
[Serializable]
public class IntArrayWrapper
{
    public int[] values;
}

[Serializable]
public class StageData
{
    public int BarId;
    public int BossAEvent;
    public int[] PlayerBeat;  // 类型：int[]
    public int[] PlayerEvent; // 类型：int[]
    public string Note;       // 类型：string
}

[Serializable]
public class EventData
{
    public int EventId;
    public string Note;        // 类型：string
    public string EventType;   // 类型：string
    public int[] SoundFile;    // 类型：int[]
    public IntArrayWrapper[] SoundBeat; // 类型：int[][] (伪装处理)
    public int Buff;           // 类型：int
    public int AVariable;      // 类型：int
    public int BVariable;      // 类型：int
    public int CVariable;      // 类型：int
}

[Serializable]
public class ConstantData
{
    public int ConstantId;
    public string ConstantName;
    public string Config;
}

// --- JsonUtility 包装容器 ---

[Serializable] public class StageList { public List<StageData> items; }
[Serializable] public class EventList { public List<EventData> items; }
[Serializable] public class ConstantList { public List<ConstantData> items; }

// --- 加载器主体 ---

public static class DataLoader
{
    // 辅助工具：包装 JSON 数组，使其符合 JsonUtility 的解析要求
    private static string WrapJson(string json) => "{\"items\":" + json + "}";

    /// <summary>
    /// 加载指定关卡的 Stage 数据 (例如传入 1 加载 Stage1.json)
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
    /// 加载通用的事件表 (Event.json)
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

    /// <summary>
    /// 加载通用的常量表 (Constant.json)
    /// </summary>
    public static List<ConstantData> LoadConstantData()
    {
        TextAsset asset = Resources.Load<TextAsset>("Constant");
        if (asset == null)
        {
            Debug.LogError("❌ [DataLoader] 找不到文件: Resources/Constant.json");
            return new List<ConstantData>();
        }
        return JsonUtility.FromJson<ConstantList>(WrapJson(asset.text)).items;
    }
}