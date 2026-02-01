using System.Collections.Generic;
using UnityEngine;

public class LevelBlackBoard : MonoBehaviour
{

    public static int CurrentLevelID = 1;


    public List<StageData> stages = new List<StageData>();
    public List<EventData> events = new List<EventData>();


    public int debugLevelID;

    private void Awake()
    {
       
        debugLevelID = CurrentLevelID;
        stages = DataLoader.LoadStageData(LevelBlackBoard.CurrentLevelID);
        events = DataLoader.LoadEventData();

        /*// Adjust all PlayerBeat values by subtracting 1
        if (stages != null)
        {
            foreach (var stage in stages)
            {
                if (stage.PlayerBeat != null && stage.PlayerBeat.Length > 0)
                {
                    for (int i = 0; i < stage.PlayerBeat.Length; i++)
                    {
                        stage.PlayerBeat[i] -= 1;
                    }
                }
            }
        }*/
    }


    public bool isPlayerBeat(int sectionIndex, int beatIndex)
    {
        sectionIndex--;
        if (sectionIndex < 0 || sectionIndex >= stages.Count)
        {
            return false;
        }
        StageData currentStage = stages[sectionIndex];
        
        if (currentStage.PlayerBeat == null || currentStage.PlayerBeat.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < currentStage.PlayerBeat.Length; i++)
        {
            if (currentStage.PlayerBeat[i] == beatIndex)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取指定回合的 PlayerBeat 数组
    /// </summary>
    /// <param name="roundIndex">回合索引（从1开始）</param>
    /// <returns>PlayerBeat 数组，如果没有则返回 null</returns>
    public int[] GetPlayerBeats(int roundIndex)
    {
        int sectionIndex = roundIndex - 1;
        
        if (sectionIndex < 0 || sectionIndex >= stages.Count)
        {
            return null;
        }
        
        StageData currentStage = stages[sectionIndex];
        
        if (currentStage.PlayerBeat == null || currentStage.PlayerBeat.Length == 0)
        {
            return null;
        }
        
        return currentStage.PlayerBeat;
    }

    /// <summary>
    /// 获取指定回合的 PlayerEvent 数组
    /// </summary>
    /// <param name="roundIndex">回合索引（从1开始）</param>
    /// <returns>PlayerEvent 数组，如果没有则返回 null</returns>
    public int[] GetPlayerEvents(int roundIndex)
    {
        int sectionIndex = roundIndex - 1;
        
        if (sectionIndex < 0 || sectionIndex >= stages.Count)
        {
            return null;
        }
        
        StageData currentStage = stages[sectionIndex];
        
        if (currentStage.PlayerEvent == null || currentStage.PlayerEvent.Length == 0)
        {
            return null;
        }
        
        return currentStage.PlayerEvent;
    }

    /// <summary>
    /// 根据回合和节拍索引获取对应的 PlayerEvent ID
    /// </summary>
    /// <param name="roundIndex">回合索引（从1开始）</param>
    /// <param name="beatIndex">节拍索引</param>
    /// <returns>对应的 PlayerEvent ID，如果没有或不是特殊判定则返回 0</returns>
    public int GetPlayerEventByBeat(int roundIndex, int beatIndex)
    {
        int[] playerBeats = GetPlayerBeats(roundIndex);
        int[] playerEvents = GetPlayerEvents(roundIndex);
        
        // 检查数组是否有效
        if (playerBeats == null || playerEvents == null)
        {
            return 0;
        }
        
        // 检查数组长度是否匹配
        if (playerBeats.Length != playerEvents.Length)
        {
            Debug.LogWarning($"警告: 回合 {roundIndex} 的 PlayerBeat 和 PlayerEvent 数组长度不匹配！");
            return 0;
        }
        
        // 查找对应的节拍索引
        for (int i = 0; i < playerBeats.Length; i++)
        {
            if (playerBeats[i] == beatIndex)
            {
                // 返回对应的 Event ID（如果是0或空则表示普通判定）
                return playerEvents[i];
            }
        }
        
        return 0;
    }

    /// <summary>
    /// 获取当前关卡的小节总数
    /// </summary>
    /// <returns>小节数量</returns>
    public int GetTotalRounds()
    {
        if (stages == null)
        {
            return 0;
        }
        
        return stages.Count;
    }

    /// <summary>
    /// 获取指定回合的 BossBeat（Boss 节拍索引）
    /// </summary>
    /// <param name="roundIndex">回合索引（从1开始）</param>
    /// <returns>BossBeat 索引，如果没有则返回 -1</returns>
    public int GetBossBeat(int roundIndex)
    {
        int sectionIndex = roundIndex - 1;
        
        if (sectionIndex < 0 || sectionIndex >= stages.Count)
        {
            return -1;
        }
        
        StageData currentStage = stages[sectionIndex];
        return currentStage.BossBeat;
    }

    /// <summary>
    /// 获取指定回合的 BossAEvent（Boss 事件 ID）
    /// </summary>
    /// <param name="roundIndex">回合索引（从1开始）</param>
    /// <returns>BossAEvent ID，如果没有则返回 -1</returns>
    public int GetBossAEvent(int roundIndex)
    {
        int sectionIndex = roundIndex - 1;
        
        if (sectionIndex < 0 || sectionIndex >= stages.Count)
        {
            return -1;
        }
        
        StageData currentStage = stages[sectionIndex];
        return currentStage.BossAEvent;
    }

    /// <summary>
    /// 检查指定回合和节拍索引是否是 BossBeat
    /// </summary>
    /// <param name="roundIndex">回合索引（从1开始）</param>
    /// <param name="beatIndex">节拍索引</param>
    /// <returns>如果是 BossBeat 返回 true</returns>
    public bool IsBossBeat(int roundIndex, int beatIndex)
    {
        int bossBeat = GetBossBeat(roundIndex);
        return bossBeat != -1 && bossBeat == beatIndex;
    }

    /// <summary>
    /// 根据事件ID查找并返回对应的 ShowMessage
    /// </summary>
    /// <param name="eventId">事件ID（字符串或数字形式）</param>
    /// <returns>对应的 ShowMessage，如果未找到则返回 null</returns>
    public string GetShowMessageByEventId(string eventId)
    {
        if (events == null || events.Count == 0)
        {
            Debug.LogWarning("事件表为空！");
            return null;
        }

        foreach (EventData eventData in events)
        {
            if (eventData.EventId == eventId)
            {
                return eventData.ShowMessage;
            }
        }

        Debug.LogWarning($"未找到事件ID: {eventId}");
        return null;
    }

    /// <summary>
    /// 根据事件ID（int）查找并返回对应的 ShowMessage
    /// </summary>
    /// <param name="eventId">事件ID（整数形式）</param>
    /// <returns>对应的 ShowMessage，如果未找到则返回 null</returns>
    public string GetShowMessageByEventId(int eventId)
    {
        return GetShowMessageByEventId(eventId.ToString());
    }

    /// <summary>
    /// 根据事件ID查找并返回完整的 EventData
    /// </summary>
    /// <param name="eventId">事件ID（字符串形式）</param>
    /// <returns>对应的 EventData，如果未找到则返回 null</returns>
    public EventData GetEventDataById(string eventId)
    {
        if (events == null || events.Count == 0)
        {
            Debug.LogWarning("事件表为空！");
            return null;
        }

        foreach (EventData eventData in events)
        {
            if (eventData.EventId == eventId)
            {
                return eventData;
            }
        }

        Debug.LogWarning($"未找到事件ID: {eventId}");
        return null;
    }

    /// <summary>
    /// 根据事件ID（int）查找并返回完整的 EventData
    /// </summary>
    /// <param name="eventId">事件ID（整数形式）</param>
    /// <returns>对应的 EventData，如果未找到则返回 null</returns>
    public EventData GetEventDataById(int eventId)
    {
        return GetEventDataById(eventId.ToString());
    }

    /// <summary>
    /// 根据事件ID查找并返回对应的 SoundFile 属性
    /// </summary>
    /// <param name="eventId">事件ID（字符串形式）</param>
    /// <returns>对应的音频文件名，如果未找到或为空则返回空字符串</returns>
    public string GetSoundFileByEventId(string eventId)
    {
        // 直接利用你现有的 GetEventDataById 函数进行查找
        EventData eventData = GetEventDataById(eventId);

        if (eventData != null && !string.IsNullOrEmpty(eventData.SoundFile))
        {
            return eventData.SoundFile;
        }

        return "";
    }

    /// <summary>
    /// 根据事件ID（int）查找并返回对应的 SoundFile 属性
    /// </summary>
    /// <param name="eventId">事件ID（整数形式）</param>
    /// <returns>对应的音频文件名，如果未找到或为空则返回空字符串</returns>
    public string GetSoundFileByEventId(int eventId)
    {
        return GetSoundFileByEventId(eventId.ToString());
    }
}