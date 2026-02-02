using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 节拍模拟器 - 控制TimeBar根据BPM和节拍数循环移动
/// </summary>
public class BeatBar : MonoBehaviour
{
    [SerializeField] private int LevelID = 1;
    [Header("引用")]
    [Tooltip("左边界（Left空物体）")]
    [SerializeField] private RectTransform left;
    
    [Tooltip("右边界（Right空物体）")]
    [SerializeField] private RectTransform right;
    
    [Tooltip("移动的时间条（TimeBar）")]
    [SerializeField] private RectTransform timeBar;

    [Tooltip("锅盖管理器")]
    [SerializeField] private PotManager potManager;


    [Tooltip("每一拍的时间完美程度反馈")]
    [SerializeField] private TextMeshProUGUI eviation;

    [Header("节拍配置")]
    [Tooltip("每分钟节拍数")]
    [SerializeField] private float bpm = 120f;
    
    [Tooltip("Board容纳的节拍数")]
    [SerializeField] private int beatsPerBar = 4;

    [Header("玩家输入判定")]
    [Tooltip("Perfect判定阈值（毫秒）")]
    [SerializeField] private float perfectThresholdMs = 30f;
    
    [Tooltip("Great判定阈值（毫秒）")]
    [SerializeField] private float greatThresholdMs = 80f;
    
    [Tooltip("Good判定阈值（毫秒）")]
    [SerializeField] private float goodThresholdMs = 120f;
    
    [Header("音频设置")]
    [Tooltip("音频延迟补偿（毫秒）- 提前多少ms播放音效")]
    [SerializeField] private float audioLatencyMs = 250f;

    [Header("运行时信息")]
    [Tooltip("游戏开始前的延迟时间（秒）")]
    [SerializeField] private float startOffset = 2f;
    
    [Tooltip("是否自动开始")]
    [SerializeField] private bool autoStart = true;
    
    [Tooltip("当前是否运行中")]
    [SerializeField] private bool isPlaying = false;
    
    [Tooltip("是否处于准备阶段")]
    [SerializeField] private bool isInPreparation = false;

    [Tooltip("小节计数器")]
    [SerializeField]private RoundController roundController;
    private int roundIndex = 1;

    [Tooltip("QTE 触发按键序列（共3个）")]
    [SerializeField] private KeyCode[] validKeys = new KeyCode[3];

    [Tooltip("关卡数据管理器")]
    [SerializeField] private LevelBlackBoard levelBlackBoard;

    List<StageData> stages = new List<StageData>();
    List<EventData> events = new List<EventData>();

    // 私有变量
    private float distance;             // 左右边界的距离（世界坐标）
    private float startX;               // 起始X位置（世界坐标）
    private float endX;                 // 结束X位置（世界坐标）
    private float cycleDuration;        // 一个循环的持续时间（秒）
    private double cycleStartTime;      // 当前循环开始的DSP时间
    private double gameStartTime;       // 游戏真正开始的时间（包含offset）
    private double pauseTime;           // 暂停时的DSP时间
    private double pausedDuration;      // 总暂停时长
    private bool[] beatTriggered;       // 记录每个节拍是否已触发（避免重复触发）
    private bool[] beatJudged;          // 记录每个节拍是否已被判定（避免重复判定）
    private bool[] beatAudioScheduled;  // 记录每个节拍的音频是否已调度
    private bool[] beatAudioEnemy;      // 记录记录每个节拍的音频是否已调度(敌人的)
    private double beatInterval;        // 每个节拍的时间间隔（秒）
    private const double BEAT_TRIGGER_THRESHOLD = 0.001; // 节拍触发阈值（20毫秒容差）

    // 判定统计
    private int perfectCount = 0;
    private int greatCount = 0;
    private int goodCount = 0;
    private int missCount = 0;


    private void Start()
    {
        Initialize();

        // 注册事件
        EventCenter.Instance.AddEventListener(GameEvents.StopGame, OnStopGame);
        EventCenter.Instance.AddEventListener(GameEvents.ResumeGame, OnResumeGame);
        EventCenter.Instance.AddEventListener(GameEvents.Pause, TogglePause);
        EventCenter.Instance.AddEventListener(GameEvents.rRestart, RealRestart);


        if (autoStart)
        {
            StartGame();
        }
    }

    private void OnDestroy()
    {
        // 注销事件
        EventCenter.Instance.RemoveEventListener(GameEvents.StopGame, OnStopGame);
        EventCenter.Instance.RemoveEventListener(GameEvents.ResumeGame, OnResumeGame);
        EventCenter.Instance.RemoveEventListener(GameEvents.Pause, TogglePause);
        EventCenter.Instance.RemoveEventListener(GameEvents.rRestart, RealRestart);


    }
    private void Update()
    {
        /*// 测试功能：按ESC键暂停/继续
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // 按R键重新开始
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }*/


        // 检查是否还在准备阶段
        if (isInPreparation)
        {
            double currentTime = AudioSettings.dspTime;
            if (currentTime >= gameStartTime)
            {
                // 准备阶段结束，开始游戏
                isInPreparation = false;
                isPlaying = true;
                Debug.Log($"准备阶段结束，游戏正式开始！时间: {currentTime:F4}");
            }
            return; // 在准备阶段不执行其他逻辑
        }
        
        if (!isPlaying)
            return;

        MoveTimeBar();
        
        // 检测玩家输入 - 只在游戏正式开始后才接受输入
        CheckLetterInput();
        
        // 实时检查是否有错过的按键
        CheckMissedBeats();
    }

    /// <summary>
    /// 检测按键输入 - 只检测预设的有效按键
    /// </summary>
    private void CheckLetterInput()
    {
        if (Input.anyKeyDown && validKeys != null && validKeys.Length > 0)
        {
            // 遍历预设的有效按键
            foreach (KeyCode key in validKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    // 获取按下的字母
                    string letter = key.ToString();
                    HandlePlayerInput(letter);
                    return; // 只处理一次输入
                }
            }
        }
    }

    /// <summary>
    /// 事件处理：停止游戏
    /// </summary>
    private void OnStopGame()
    {
        Stop();
    }

    /// <summary>
    /// 事件处理：重新开始游戏
    /// </summary>
    private void OnResumeGame()
    {
        Restart();
    }

    /// <summary>
    /// 实时检查是否有错过的按键
    /// </summary>
    private void CheckMissedBeats()
    {
        if (levelBlackBoard == null)
            return;

        // 获取当前回合需要玩家按的节拍列表
        int[] playerBeats = levelBlackBoard.GetPlayerBeats(roundIndex);
        
        if (playerBeats == null || playerBeats.Length == 0)
            return;

        // 获取当前时间和经过时间
        double currentTime = AudioSettings.dspTime;
        double elapsedTime = currentTime - cycleStartTime;

        // 遍历需要按的节拍
        foreach (int beatIndex in playerBeats)
        {
            // 如果该节拍已经被判定过，跳过
            if (beatJudged[beatIndex])
                continue;

            // 计算该节拍的目标时间
            double targetBeatTime = beatIndex * beatInterval;
            
            // 计算超过目标时间多久了
            double timePassed = elapsedTime - targetBeatTime;
            
            // 如果超过了判定窗口（Good阈值），标记为Miss
            double goodThresholdSec = goodThresholdMs / 1000.0;
            if (timePassed > goodThresholdSec)
            {
                // 检查是否是特殊判定
                int eventId = 0;
                Category category = Category.Generic;
                if (levelBlackBoard != null)
                {
                    eventId = levelBlackBoard.GetPlayerEventByBeat(roundIndex, beatIndex);
                    if (eventId != 0)
                    {
                        category = Category.Specific;
                    }
                }

                string tx = "";
                if(category == Category.Specific)
                {
                    tx = levelBlackBoard.GetShowMessageByEventId(eventId);
                }
                // 标记为超时Miss
                JudgementResult missResult = new JudgementResult(JudgementLevel.Miss, beatIndex, goodThresholdMs / 1000.0, "", category, eventId, tx);
                ProcessJudgement(missResult);
            }
        }
    }

    #region 初始化阶段
    /// <summary>
    /// 初始化
    /// </summary>
    private void Initialize()
    {
        if (left == null || right == null || timeBar == null || roundController == null)
        {
            return;
        }

        // 使用世界坐标获取左右边界位置，避免scale影响
        startX = left.position.x;
        endX = right.position.x;

        // 计算距离（世界坐标）
        distance = Mathf.Abs(endX - startX);


        // 计算循环时间和速度
        CalculateSpeed();

        // 设置初始位置
        ResetPosition();
        roundController.SetRound(roundIndex);

        //重置锅盖
        UpdatePotDisplay();


    }

    /// <summary>
    /// 根据BPM和节拍数计算循环时间
    /// </summary>
    /// 
    #endregion

    #region TimeBar相关
    private void CalculateSpeed()
    {
        // 一个循环的持续时间（秒） = (节拍数 / BPM) * 60
        cycleDuration = (beatsPerBar / bpm) * 60f;
        
        // 计算每个节拍的时间间隔
        beatInterval = cycleDuration / beatsPerBar;
        
        // 初始化节拍触发数组和判定数组
        beatTriggered = new bool[beatsPerBar];
        beatJudged = new bool[beatsPerBar];
        beatAudioScheduled = new bool[beatsPerBar];
        beatAudioEnemy = new bool[beatsPerBar];
    }

    /// <summary>
    /// 移动TimeBar
    /// </summary>
    private void MoveTimeBar()
    {
        // 使用AudioSettings.dspTime获取精确时间
        double currentTime = AudioSettings.dspTime;
        double elapsedTime = currentTime - cycleStartTime;

        // 检查是否完成循环
        if (elapsedTime >= cycleDuration)
        {
            // 开始新的循环
            cycleStartTime = currentTime;
            elapsedTime = 0;
            ResetBeatTriggers(); // 重置节拍触发标记
            OnCycleComplete();
        }

        // 根据时间计算进度 (0-1)
        float progress = (float)(elapsedTime / cycleDuration);

        // 检测并触发节拍事件（只在分隔线位置：1, 2, 3...beatsPerBar-1）
        CheckAndTriggerBeats(elapsedTime);

        // 根据进度计算位置
        Vector3 worldPos = timeBar.position;
        worldPos.x = Mathf.Lerp(startX, endX, progress);

        // 应用新的世界位置
        timeBar.position = worldPos;
    }

    /// <summary>
    /// 检测并触发节拍事件
    /// </summary>
    private void CheckAndTriggerBeats(double elapsedTime)
    {
        // 音频延迟补偿（秒）
        double audioLatencySeconds = audioLatencyMs / 1000.0;
        
        // 检查所有节拍位置（0到beatsPerBar-1）
        for (int beatIndex = 0; beatIndex < beatsPerBar; beatIndex++)
        {
            // 计算这个节拍应该出现的精确时间
            double targetBeatTime = beatIndex * beatInterval;
            
            // 提前播放音效
            if (!beatAudioScheduled[beatIndex] && elapsedTime >= targetBeatTime - audioLatencySeconds)
            {
                if (levelBlackBoard != null)
                {
                    if (levelBlackBoard.isPlayerBeat(roundIndex, beatIndex))
                    {
                        beatAudioScheduled[beatIndex] = true;
                        AudioManager.Instance.PlaySound("Press");
                    }
                }
            }


            if (!beatAudioEnemy[beatIndex] && elapsedTime >= targetBeatTime - audioLatencySeconds)
            {
                // 检查是否是 BossBeat，如果是则触发 BossAction 事件
                if (levelBlackBoard != null && levelBlackBoard.IsBossBeat(roundIndex, beatIndex))
                {
                    int bossAEvent = levelBlackBoard.GetBossAEvent(roundIndex);
                    string bossMSG = levelBlackBoard.GetShowMessageByEventId(bossAEvent);
                    string soundName = levelBlackBoard.GetSoundFileByEventId(bossAEvent);
                    if (bossAEvent != -1 && bossAEvent != 0)
                    {
                        beatAudioEnemy[beatIndex] = true;
                        //敌人信息打印
                        EventCenter.Instance.EventTrigger(GameEvents.OnBossAction, bossMSG);
                        if (!string.IsNullOrEmpty(soundName))
                        {
                            //播放相关音效
                            AudioManager.Instance.PlaySound(soundName);
                        }
                        //AudioManager.Instance.PlaySound("Press");

                        Debug.Log($"触发 Boss 行动: Round {roundIndex}, Beat {beatIndex}, EventID {bossAEvent}");
                    }
                }
            }


            // 如果这个节拍已经触发过，跳过
            if (beatTriggered[beatIndex])
                continue;

            // 检查当前时间是否到达或超过目标时间（带容差）
            if (elapsedTime >= targetBeatTime - BEAT_TRIGGER_THRESHOLD)
            {
                // 标记为已触发
                beatTriggered[beatIndex] = true;

                // 触发节拍事件
                OnBeatHit(beatIndex, elapsedTime - targetBeatTime);
                
                // 当 beatIndex >= 3 时，更新 beatIndex - 2 的锅盖状态
                // 显示下一轮的状态
                if (beatIndex >= 3)
                {
                    int potToUpdate = beatIndex - 2;  // 要更新的锅的索引（1-7）
                    int nextRound = roundIndex + 1;   // 下一轮
                    UpdateSinglePot(potToUpdate, nextRound);
                }
            }
        }
    }

    /// <summary>
    /// 重置节拍触发标记
    /// </summary>
    private void ResetBeatTriggers()
    {
        if (beatTriggered != null)
        {
            for (int i = 0; i < beatTriggered.Length; i++)
            {
                beatTriggered[i] = false;
            }
        }
        
        if (beatJudged != null)
        {
            for (int i = 0; i < beatJudged.Length; i++)
            {
                beatJudged[i] = false;
            }
        }
        
        if (beatAudioScheduled != null)
        {
            for (int i = 0; i < beatAudioScheduled.Length; i++)
            {
                beatAudioScheduled[i] = false;
            }
        }

        if(beatAudioEnemy != null)
        {
            for( int i = 0;i < beatAudioEnemy.Length; i++)
            {
                beatAudioEnemy[i] = false;
            }
        }
    }

    /// <summary>
    /// 重置到起始位置
    /// </summary>
    private void ResetPosition()
    {
        if (timeBar != null)
        {
            Vector3 worldPos = timeBar.position;
            worldPos.x = startX;
            timeBar.position = worldPos;
            // 重置循环开始时间
            cycleStartTime = AudioSettings.dspTime;
            // 重置节拍触发标记
            ResetBeatTriggers();
        }
    }
    #endregion

    #region GameProcess
    /// <summary>
    /// 一个循环完成时的回调
    /// </summary>
    private void OnCycleComplete()
    {
        // 检查游戏是否结束
        if (levelBlackBoard != null)
        {
            int totalRounds = levelBlackBoard.GetTotalRounds();
            if (roundIndex >= totalRounds)
            {
                Stop(); // 结束关卡
                return;
            }
        }

        roundIndex++;
        roundController.SetRound(roundIndex);
        
        // 重置所有节拍状态（包括音频调度）
        ResetBeatTriggers();

        // 更新锅盖显示
        UpdatePotDisplay();
    }

    /// <summary>
    /// 更新锅盖显示
    /// </summary>
    private void UpdatePotDisplay()
    {
        if (levelBlackBoard == null || potManager == null)
        {
            return;
        }

        // 获取当前回合的 PlayerBeat 数组
        int[] playerBeats = levelBlackBoard.GetPlayerBeats(roundIndex);

        // 先隐藏所有锅盖（1-7）
        for (int i = 1; i <= 7; i++)
        {
            potManager.ShowPot(i);
        }

        // 如果有 PlayerBeat 数据，显示对应的锅盖
        if (playerBeats != null && playerBeats.Length > 0)
        {
            foreach (int beatIndex in playerBeats)
            {
                potManager.HidePot(beatIndex);
            }
        }
    }

    /// <summary>
    /// 更新单个锅的显示状态
    /// </summary>
    /// <param name="potIndex">锅的索引（1-7）</param>
    /// <param name="targetRound">要显示的目标轮次</param>
    private void UpdateSinglePot(int potIndex, int targetRound)
    {
        if (potManager == null || levelBlackBoard == null)
            return;
        
        // 先显示这个锅（默认有盖子）
        potManager.ShowPot(potIndex);
        
        // 获取目标轮次的 PlayerBeat 数据
        int[] targetBeats = levelBlackBoard.GetPlayerBeats(targetRound);
        
        // 如果目标轮次需要按这个锅，则隐藏锅盖
        if (targetBeats != null && targetBeats.Length > 0)
        {
            foreach (int beatIndex in targetBeats)
            {
                if (beatIndex == potIndex)
                {
                    potManager.HidePot(potIndex);
                    Debug.Log($"更新锅 {potIndex}: 显示轮次 {targetRound} 的状态（隐藏锅盖）");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 节拍命中回调 - 当TimeBar到达某个节拍点时触发
    /// </summary>
    /// <param name="beatIndex">节拍索引（1到beatsPerBar-1）</param>
    /// <param name="deviation">时间偏差（秒），正数表示晚了，负数表示早了</param>
    private void OnBeatHit(int beatIndex, double deviation)
    {
        // 创建节拍事件数据
        BeatHitData beatData = new BeatHitData(beatIndex, roundIndex);
        
        // 触发事件，通知其他系统节拍到达
        EventCenter.Instance.EventTrigger(GameEvents.OnBeatHit, beatData);
        
        // 转换为毫秒显示
        double deviationMs = deviation * 1000;
        Debug.Log($"节拍命中: Beat {beatIndex}/{beatsPerBar}, Round {roundIndex}, 偏差: {deviationMs:F2}ms, DSP时间: {AudioSettings.dspTime:F4}");
        

        
        // 显示箭头位置
        if (potManager != null)
        {
            // 判断节拍索引是否在1-7范围内
            if (beatIndex >= 1 && beatIndex <= 7)
            {
                // 显示箭头在对应锅盖位置
                potManager.ShowArrowInIndex(beatIndex);
            }
            else
            {
                // 显示箭头在默认位置
                potManager.ShowArrowAtDefaultPosition();
            }
        }
    }
    #endregion

    #region 玩家输入判定

    /// <summary>
    /// 处理玩家输入
    /// </summary>
    /// <param name="letter">玩家按下的字母</param>
    private void HandlePlayerInput(string letter)
    {
        // 获取当前时间
        double currentTime = AudioSettings.dspTime;
        double elapsedTime = currentTime - cycleStartTime;

        // 查找最近的可判定节拍
        int nearestBeat = -1;
        double nearestDeviation = double.MaxValue;

        // 检查所有节拍位置（0到beatsPerBar-1）
        for (int beatIndex = 0; beatIndex < beatsPerBar; beatIndex++)
        {
            // 如果这个节拍已被判定，跳过
            if (beatJudged[beatIndex])
                continue;

            // 计算这个节拍的目标时间
            double targetBeatTime = beatIndex * beatInterval;
            double deviation = elapsedTime - targetBeatTime;
            double absDeviation = System.Math.Abs(deviation);

            // 检查是否在可输入范围内（Good阈值）
            double goodThresholdSec = goodThresholdMs / 1000.0;
            if (absDeviation <= goodThresholdSec && absDeviation < System.Math.Abs(nearestDeviation))
            {
                nearestBeat = beatIndex;
                nearestDeviation = deviation;
            }
        }
        bool canPress = false;
        canPress = levelBlackBoard.isPlayerBeat(roundIndex, nearestBeat);

        // 如果找到了可判定的节拍
        if (nearestBeat != -1 && canPress)
        {
            // 判定等级
            JudgementLevel level = CalculateJudgementLevel(System.Math.Abs(nearestDeviation));
            
            // 判断是否是特殊判定
            int eventId = 0;
            Category category = Category.Generic;
            string tx = "";
            if (levelBlackBoard != null)
            {
                eventId = levelBlackBoard.GetPlayerEventByBeat(roundIndex, nearestBeat);
                if (eventId != 0)
                {
                    category = Category.Specific;
                    tx = levelBlackBoard.GetShowMessageByEventId(eventId);
                }
            }
            
            // 创建判定结果（包含按键信息）
            JudgementResult result = new JudgementResult(level, nearestBeat, nearestDeviation, letter, category, eventId, tx);
            
            // 处理判定
            ProcessJudgement(result);
        }
        else
        {
            Debug.Log($"输入字母 {letter} 太早或太晚，没有可判定的节拍");
        }
    }

    /// <summary>
    /// 根据偏差计算判定等级
    /// </summary>
    private JudgementLevel CalculateJudgementLevel(double absDeviationSec)
    {
        double absDeviationMs = absDeviationSec * 1000;

        if (absDeviationMs <= perfectThresholdMs)
            return JudgementLevel.Perfect;
        else if (absDeviationMs <= greatThresholdMs)
            return JudgementLevel.Great;
        else if (absDeviationMs <= goodThresholdMs)
            return JudgementLevel.Good;
        else
            return JudgementLevel.Miss;
    }

    /// <summary>
    /// 处理判定结果
    /// </summary>
    private void ProcessJudgement(JudgementResult result)
    {
        // 检查是否已经被判定过
        if (beatJudged[result.BeatIndex])
        {
            Debug.LogWarning($"节拍 {result.BeatIndex} 已经被判定过了，跳过重复判定！");
            return;
        }
        
        // 标记该节拍已被判定
        beatJudged[result.BeatIndex] = true;
        
        // 统计判定结果
        switch (result.Level)
        {
            case JudgementLevel.Perfect:
                perfectCount++;
                break;
            case JudgementLevel.Great:
                greatCount++;
                break;
            case JudgementLevel.Good:
                // Good 算进 Miss 里面
                missCount++;
                break;
            case JudgementLevel.Miss:
                missCount++;
                break;
        }
        
        // 更新偏差显示（Miss不更新）
        if (result.Level != JudgementLevel.Miss && eviation != null)
        {
            eviation.text = $"{result.DeviationMs:F1}ms ({result.Level})";
            Debug.Log($"更新偏差显示: {eviation.text}");
            
            // 根据判定等级设置颜色
            switch (result.Level)
            {
                case JudgementLevel.Perfect:
                    eviation.color = Color.yellow;
                    break;
                case JudgementLevel.Great:
                    eviation.color = Color.green;
                    break;
                case JudgementLevel.Good:
                    eviation.color = Color.blue;
                    break;
            }
        }
        else if (eviation == null)
        {
            Debug.LogWarning("eviation UI组件为null，无法更新偏差显示！");
        }




        // 根据判定等级和按键改变表情（只在成功判定时）
        if (result.Level == JudgementLevel.Perfect || result.Level == JudgementLevel.Great)
        {
            if (FaceManager.Instance != null && !string.IsNullOrEmpty(result.InputKey))
            {
                if (result.InputKey == "Q")
                {
                    FaceManager.Instance.ChangeFace(0);
                }
                else if (result.InputKey == "W")
                {
                    FaceManager.Instance.ChangeFace(1);
                }
                else if (result.InputKey == "E")
                {
                    FaceManager.Instance.ChangeFace(2);
                }
            }
        }

        Debug.Log($"触发判定事件: BeatIndex={result.BeatIndex}, Level={result.Level}, Key={result.InputKey}");
        
        // 触发判定事件
        EventCenter.Instance.EventTrigger(GameEvents.OnPlayerJudgement, result);
        
        // 日志输出
        string keyInfo = string.IsNullOrEmpty(result.InputKey) ? "Miss" : $"Key: {result.InputKey}";
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 开始游戏（包含offset延迟）
    /// </summary>
    public void StartGame()
    {
        AudioManager.Instance.PlayBGM("Level" + levelBlackBoard.CurrentLevelID +"_Stereo") ;//这里需要指定歌曲
        double currentTime = AudioSettings.dspTime;
        gameStartTime = currentTime + startOffset;
        cycleStartTime = gameStartTime;
        
        isInPreparation = true;
        isPlaying = false;
        
        // 重置节拍触发标记
        ResetBeatTriggers();
        
        Debug.Log($"游戏准备开始，当前时间: {currentTime:F4}, 延迟: {startOffset}秒, 将在 {gameStartTime:F4} 开始");
    }

    /// <summary>
    /// 开始播放（立即开始，无offset）
    /// </summary>
    public void Play()
    {
        isPlaying = true;
        isInPreparation = false;
        // 记录开始时间
        cycleStartTime = AudioSettings.dspTime;
        gameStartTime = cycleStartTime;
        // 重置节拍触发标记
        ResetBeatTriggers();
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void Pause()
    {
        if (!isPlaying && !isInPreparation)
            return;
        
        // 记录暂停时间
        pauseTime = AudioSettings.dspTime;
        
        isPlaying = false;
        isInPreparation = false;
        
        // 暂停BGM（如果AudioManager存在）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseBGM();
        }
        
        Debug.Log($"游戏暂停，暂停时间: {pauseTime:F4}");
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void Resume()
    {
        if (isPlaying)
            return;
        
        // 计算暂停的时长
        double currentTime = AudioSettings.dspTime;
        double thisPauseDuration = currentTime - pauseTime;
        pausedDuration += thisPauseDuration;
        
        // 调整时间轴
        cycleStartTime += thisPauseDuration;
        gameStartTime += thisPauseDuration;
        
        isPlaying = true;
        
        // 恢复BGM（如果AudioManager存在）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeBGM();
        }
        
        Debug.Log($"游戏恢复，暂停时长: {thisPauseDuration:F4}秒，累计暂停: {pausedDuration:F4}秒");
    }

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        if (isPlaying || isInPreparation)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    /// <summary>
    /// 停止并重置（用于结束关卡）
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        isInPreparation = false;
        pausedDuration = 0;
        
        // 停止BGM（如果AudioManager存在）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }
        
        // 重置位置
        ResetPosition();

        
        //EventCenter.Instance.EventTrigger(GameEvents.OnStageEnd,new StageEndData(perfectCount,greatCount,missCount));
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void Restart()
    {
        // 停止当前游戏
        Stop();
        
        // 重置回合数
        roundIndex = 1;
        if (roundController != null)
        {
            roundController.SetRound(roundIndex);
        }
        
        // 重置所有节拍状态
        ResetBeatTriggers();
        
        // 重置统计数据
        ResetStatistics();
        
        // 初始化偏差显示为等待状态
        if (eviation != null)
        {
            eviation.text = "Ready...";
            eviation.color = Color.white;
        }
        
        Debug.Log("游戏重新开始");
        
        // 重新开始游戏（包含offset）
        StartGame();
    }

    public void RealRestart()
    {
        
        isPlaying = false;
        isInPreparation = false;
        pausedDuration = 0;

        // 停止BGM（如果AudioManager存在）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }

        // 重置位置
        ResetPosition();

        // 重置回合数
        roundIndex = 1;
        if (roundController != null)
        {
            roundController.SetRound(roundIndex);
        }

        // 重置所有节拍状态
        ResetBeatTriggers();

        // 重置统计数据
        ResetStatistics();

        // 初始化偏差显示为等待状态
        if (eviation != null)
        {
            eviation.text = "Ready...";
            eviation.color = Color.white;
        }

        Debug.Log("游戏重新开始");

        // 重新开始游戏（包含offset）
        StartGame();
    }

    /// <summary>
    /// 设置BPM
    /// </summary>
    public void SetBPM(float newBpm)
    {
        bpm = Mathf.Max(1f, newBpm); // 确保BPM至少为1
        CalculateSpeed();
    }

    /// <summary>
    /// 设置节拍数
    /// </summary>
    public void SetBeatsPerBar(int beats)
    {
        beatsPerBar = Mathf.Max(1, beats); // 确保节拍数至少为1
        CalculateSpeed();
    }

    /// <summary>
    /// 获取当前进度（0-1）
    /// </summary>
    public float GetProgress()
    {
        if (timeBar == null)
            return 0f;

        float currentX = timeBar.position.x;
        return Mathf.InverseLerp(startX, endX, currentX);
    }

    /// <summary>
    /// 获取当前所在节拍（0到beatsPerBar-1）
    /// </summary>
    public int GetCurrentBeat()
    {
        float progress = GetProgress();
        return Mathf.FloorToInt(progress * beatsPerBar);
    }

    /// <summary>
    /// 获取剩余准备时间（秒）
    /// </summary>
    public float GetRemainingPreparationTime()
    {
        if (!isInPreparation)
            return 0f;
        
        double currentTime = AudioSettings.dspTime;
        return (float)System.Math.Max(0, gameStartTime - currentTime);
    }

    /// <summary>
    /// 是否在准备阶段
    /// </summary>
    public bool IsInPreparation()
    {
        return isInPreparation;
    }

    /// <summary>
    /// 是否已暂停
    /// </summary>
    public bool IsPaused()
    {
        return !isPlaying && !isInPreparation && pausedDuration > 0;
    }

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }
    
    /// <summary>
    /// 获取 Perfect 数量
    /// </summary>
    public int GetPerfectCount()
    {
        return perfectCount;
    }
    
    /// <summary>
    /// 获取 Great 数量
    /// </summary>
    public int GetGreatCount()
    {
        return greatCount;
    }
    
    /// <summary>
    /// 获取 Miss 数量（包含 Good 和 Miss）
    /// </summary>
    public int GetMissCount()
    {
        return missCount;
    }
    
    /// <summary>
    /// 重置统计数据
    /// </summary>
    public void ResetStatistics()
    {
        perfectCount = 0;
        greatCount = 0;
        goodCount = 0;
        missCount = 0;
        Debug.Log("统计数据已重置");
    }

    #endregion

    #region Inspector验证

    private void OnValidate()
    {
        // 确保BPM和节拍数为正数
        bpm = Mathf.Max(1f, bpm);
        beatsPerBar = Mathf.Max(1, beatsPerBar);

        // 如果在播放模式中修改了参数，重新计算速度
        if (Application.isPlaying && left != null && right != null && timeBar != null)
        {
            // 重新获取边界位置（世界坐标）
            startX = left.position.x;
            endX = right.position.x;
            distance = Mathf.Abs(endX - startX);
            
            CalculateSpeed();
        }
    }

    #endregion

    #region Gizmos绘制

    private void OnDrawGizmosSelected()
    {
        if (left == null || right == null)
            return;

        // 绘制起始和结束位置
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(left.position, 10f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(right.position, 10f);

        // 绘制连接线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(left.position, right.position);
    }

    #endregion
}
