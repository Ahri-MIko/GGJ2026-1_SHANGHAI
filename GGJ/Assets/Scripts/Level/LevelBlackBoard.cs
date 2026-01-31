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
        stages = DataLoader.LoadStageData(LevelBlackBoard.CurrentLevelID);
        events = DataLoader.LoadEventData();
        constants = DataLoader.LoadConstantData();

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
}