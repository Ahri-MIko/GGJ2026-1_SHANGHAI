using System.Collections.Generic;
using UnityEngine;

public class LevelBlackBoard : MonoBehaviour
{

    public static int CurrentLevelID = 1;


    public List<StageData> stages = new List<StageData>();
    public List<EventData> events = new List<EventData>();
    public List<ConstantData> constants = new List<ConstantData>();


    public int debugLevelID;

    private void Awake()
    {
       
        debugLevelID = CurrentLevelID;

        // Load data into member variables (not local variables!)
        stages = DataLoader.LoadStageData(LevelBlackBoard.CurrentLevelID);
        events = DataLoader.LoadEventData();
        constants = DataLoader.LoadConstantData();
    }



    // Check if enemy scream sound should play at given beat
    // Returns sound file name if found, null otherwise
    public string isEnemyScream(int sectionIndex, int beatIndex)
    {
        sectionIndex--;
        if (sectionIndex < 0 || sectionIndex >= stages.Count)
        {
            Debug.LogWarning($"[LevelBlackBoard] Invalid sectionIndex: {sectionIndex}");
            return null;
        }

        StageData currentStage = stages[sectionIndex];
        int bossEventId = currentStage.BossAEvent;
        
        EventData bossEvent = events.Find(e => e.EventId == bossEventId);
        
        if (bossEvent == null)
        {
            Debug.LogWarning($"[LevelBlackBoard] Event not found for EventId: {bossEventId}");
            return null;
        }

        if (bossEvent.SoundBeat == null || bossEvent.SoundFiles == null)
        {
            Debug.LogWarning($"[LevelBlackBoard] EventId {bossEventId} has null SoundBeat or SoundFiles");
            return null;
        }

        for (int i = 0; i < bossEvent.SoundBeat.Length; i++)
        {
            if (bossEvent.SoundBeat[i] == beatIndex)
            {
                if (i < bossEvent.SoundFiles.Length)
                {
                    string soundFile = bossEvent.SoundFiles[i];
                    Debug.Log($"[LevelBlackBoard] Enemy sound found: Section={sectionIndex}, Beat={beatIndex}, Sound={soundFile}");
                    return soundFile;
                }
                else
                {
                    Debug.LogWarning($"[LevelBlackBoard] SoundFiles index out of range: i={i}, Length={bossEvent.SoundFiles.Length}");
                    return null;
                }
            }
        }

        return null;
    }
}