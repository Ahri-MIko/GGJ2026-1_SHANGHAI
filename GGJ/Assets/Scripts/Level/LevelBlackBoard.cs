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
        // 方便在编辑器里手动改值调试
        debugLevelID = CurrentLevelID;

        //加载当前stage数据
        List<StageData> stages = DataLoader.LoadStageData(LevelBlackBoard.CurrentLevelID);
        //加载关卡数据
        List<EventData> events = DataLoader.LoadEventData();
        //加载静态数据
        List<ConstantData> constants = DataLoader.LoadConstantData();
    }
}