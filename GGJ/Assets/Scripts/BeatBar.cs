using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 节拍模拟器 - 控制TimeBar根据BPM和节拍数循环移动
/// </summary>
public class BeatBar : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("左边界（Left空物体）")]
    [SerializeField] private RectTransform left;
    
    [Tooltip("右边界（Right空物体）")]
    [SerializeField] private RectTransform right;
    
    [Tooltip("移动的时间条（TimeBar）")]
    [SerializeField] private RectTransform timeBar;

    [Tooltip("每一拍的时间完美程度反馈")]
    [SerializeField] private TextMeshProUGUI eviation;

    [Header("节拍配置")]
    [Tooltip("每分钟节拍数")]
    [SerializeField] private float bpm = 120f;
    
    [Tooltip("Board容纳的节拍数")]
    [SerializeField] private int beatsPerBar = 4;

    [Header("节拍分隔线")]
    [Tooltip("节拍分隔线Prefab")]
    [SerializeField] private GameObject beatLinePrefab;
    
    [Tooltip("是否显示节拍分隔线")]
    [SerializeField] private bool showBeatLines = true;

    [Header("玩家输入判定")]
    [Tooltip("Perfect判定阈值（毫秒）")]
    [SerializeField] private float perfectThresholdMs = 30f;
    
    [Tooltip("Great判定阈值（毫秒）")]
    [SerializeField] private float greatThresholdMs = 80f;
    
    [Tooltip("Good判定阈值（毫秒）")]
    [SerializeField] private float goodThresholdMs = 120f;
    

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


    List<StageData> stages = new List<StageData>();
    List<EventData> events = new List<EventData>();
    List<ConstantData> constants = new List<ConstantData>();

    // 私有变量
    private float distance;             // 左右边界的距离（世界坐标）
    private float startX;               // 起始X位置（世界坐标）
    private float endX;                 // 结束X位置（世界坐标）
    private float cycleDuration;        // 一个循环的持续时间（秒）
    private double cycleStartTime;      // 当前循环开始的DSP时间
    private double gameStartTime;       // 游戏真正开始的时间（包含offset）
    private double pauseTime;           // 暂停时的DSP时间
    private double pausedDuration;      // 总暂停时长
    private GameObject beatLinesContainer;  // 分隔线容器
    private bool[] beatTriggered;       // 记录每个节拍是否已触发（避免重复触发）
    private bool[] beatJudged;          // 记录每个节拍是否已被判定（避免重复判定）
    private double beatInterval;        // 每个节拍的时间间隔（秒）
    private const double BEAT_TRIGGER_THRESHOLD = 0.02; // 节拍触发阈值（20毫秒容差）
    private IntervalBar[] intervalBars; // 每个节拍对应的IntervalBar组件


    private void Start()
    {
        Initialize();
        
        if (autoStart)
        {
            StartGame();
        }
    }

    private void Update()
    {
        // 测试功能：按ESC键暂停/继续
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // 按R键重新开始
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }


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

    #region 初始化阶段
    /// <summary>
    /// 初始化
    /// </summary>
    private void Initialize()
    {
        //加载当前stage数据
        List<StageData> stages = DataLoader.LoadStageData(LevelBlackBoard.CurrentLevelID);
        List<EventData> events = DataLoader.LoadEventData();
        List<ConstantData> constants = DataLoader.LoadConstantData();
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
        
        // 生成节拍分隔线
        if (showBeatLines && beatLinePrefab != null)
        {
            GenerateBeatLines();
        }
        
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
        // 只检查分隔线位置（1到beatsPerBar-1）
        for (int beatIndex = 1; beatIndex < beatsPerBar; beatIndex++)
        {
            // 如果这个节拍已经触发过，跳过
            if (beatTriggered[beatIndex])
                continue;

            // 计算这个节拍应该出现的精确时间
            double targetBeatTime = beatIndex * beatInterval;

            // 检查当前时间是否到达或超过目标时间（带容差）
            if (elapsedTime >= targetBeatTime - BEAT_TRIGGER_THRESHOLD)
            {
                // 标记为已触发
                beatTriggered[beatIndex] = true;
                
                // 触发节拍事件
                OnBeatHit(beatIndex, elapsedTime - targetBeatTime);
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
        roundIndex++;
        roundController.SetRound(roundIndex);
        
        // 重置所有IntervalBar的显示
        ResetIntervalBars();
    }

    /// <summary>
    /// 重置所有IntervalBar的显示
    /// </summary>
    private void ResetIntervalBars()
    {
        if (intervalBars != null)
        {
            for (int i = 0; i < intervalBars.Length; i++)
            {
                if (intervalBars[i] != null)
                {
                    intervalBars[i].SetCapital("");
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
        
        // 检查是否超时Miss
        CheckBeatMiss(beatIndex);
    }

    /// <summary>
    /// 检查节拍是否Miss（超过可输入范围还未被判定）
    /// </summary>
    private void CheckBeatMiss(int beatIndex)
    {
        if (beatIndex > 0 && beatIndex < beatsPerBar)
        {
            // 检查上一个节拍是否已判定
            int previousBeat = beatIndex - 1;
            if (previousBeat > 0 && !beatJudged[previousBeat])
            {
                // 上一个节拍超时未判定，标记为Miss
                JudgementResult missResult = new JudgementResult(JudgementLevel.Miss, previousBeat, goodThresholdMs / 1000.0, "");
                ProcessJudgement(missResult);
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

        // 只检查分隔线位置（1到beatsPerBar-1）
        for (int beatIndex = 1; beatIndex < beatsPerBar; beatIndex++)
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

        // 如果找到了可判定的节拍
        if (nearestBeat != -1)
        {
            // 调用对应IntervalBar的SetCapital函数
            if (intervalBars != null && nearestBeat < intervalBars.Length && intervalBars[nearestBeat] != null)
            {
                intervalBars[nearestBeat].SetCapital(letter);
                Debug.Log($"设置IntervalBar[{nearestBeat}]的字母为: {letter}");
            }
            else
            {
                Debug.LogWarning($"无法设置IntervalBar[{nearestBeat}]: intervalBars={intervalBars != null}, 长度={intervalBars?.Length}, 组件={intervalBars?[nearestBeat] != null}");
            }
            
            // 判定等级
            JudgementLevel level = CalculateJudgementLevel(System.Math.Abs(nearestDeviation));
            
            // 创建判定结果（包含按键信息）
            JudgementResult result = new JudgementResult(level, nearestBeat, nearestDeviation, letter);
            
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
        // 标记该节拍已被判定
        beatJudged[result.BeatIndex] = true;

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
        AudioManager.Instance.PlayBGM("Level1");
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
    /// 停止并重置
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        isInPreparation = false;
        pausedDuration = 0;
        ResetPosition();
        
        // 停止BGM（如果AudioManager存在）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }
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
        
        // 重置所有IntervalBar显示
        ResetIntervalBars();
        
        // 重置所有节拍状态
        ResetBeatTriggers();
        
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

    #endregion

    #region 节拍分隔线

    /// <summary>
    /// 生成节拍分隔线
    /// </summary>
    private void GenerateBeatLines()
    {
        // 清除旧的分隔线
        ClearBeatLines();
        
        // 确保intervalBars数组已初始化
        if (intervalBars == null || intervalBars.Length != beatsPerBar)
        {
            intervalBars = new IntervalBar[beatsPerBar];
        }

        // 创建容器
        beatLinesContainer = new GameObject("BeatLines");
        beatLinesContainer.transform.SetParent(transform);

        // 获取TimeBar的Y轴位置
        float yPos = timeBar.position.y;

        // 在整数拍位置生成分隔线：1, 2, 3...
        for (int i = 1; i < beatsPerBar; i++)
        {
            float progress = (float)i / beatsPerBar;
            CreateBeatLine(progress, yPos, i);
        }

        Debug.Log($"生成了{beatsPerBar - 1}条节拍分隔线");
    }

    /// <summary>
    /// 创建单个节拍分隔线
    /// </summary>
    private void CreateBeatLine(float progress, float yPos, int beatIndex)
    {
        // 实例化分隔线
        GameObject line = Instantiate(beatLinePrefab, beatLinesContainer.transform);
        
        // 计算X位置
        float xPos = Mathf.Lerp(startX, endX, progress);
        
        // 设置位置
        line.transform.position = new Vector3(xPos, yPos, line.transform.position.z);
        
        // 设置名称
        line.name = $"BeatLine_{beatIndex}";
        
        // 获取并保存IntervalBar组件
        IntervalBar intervalBar = line.GetComponent<IntervalBar>();
        if (intervalBar != null)
        {
            intervalBars[beatIndex] = intervalBar;
        }
        else
        {
            Debug.LogWarning($"BeatLine_{beatIndex} 没有找到 IntervalBar 组件！");
        }
    }

    /// <summary>
    /// 清除节拍分隔线
    /// </summary>
    private void ClearBeatLines()
    {
        if (beatLinesContainer != null)
        {
            if (Application.isPlaying)
            {
                Destroy(beatLinesContainer);
            }
            else
            {
                DestroyImmediate(beatLinesContainer);
            }
            beatLinesContainer = null;
        }
        
        // 清除IntervalBar引用
        if (intervalBars != null)
        {
            for (int i = 0; i < intervalBars.Length; i++)
            {
                intervalBars[i] = null;
            }
        }
    }

    /// <summary>
    /// 重新生成节拍分隔线（可在运行时调用）
    /// </summary>
    public void RegenerateBeatLines()
    {
        if (showBeatLines && beatLinePrefab != null && left != null && right != null && timeBar != null)
        {
            GenerateBeatLines();
        }
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
            
            // 如果显示分隔线，重新生成
            if (showBeatLines && beatLinePrefab != null)
            {
                RegenerateBeatLines();
            }
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
