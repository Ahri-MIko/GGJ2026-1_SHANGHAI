using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections.Generic;
using static EmojiDataStruct;


public class GameplayPanel : BasePanel
{
 
    public static GameplayPanel Instance;
    [Header("Pause")]
    public Button btnPause;

    [Header("Battle Stats")]
    // 直接在这里配置两个容器
    //public HealthUnit bossStats = new HealthUnit();
    //public HealthUnit playerStats = new HealthUnit();

    [Header("Face Bars (HP)")]
    public Image bossHpFill;
    public Image bossHpRedBar;
    public Image playerFaceFill;
    public TextMeshProUGUI bossFaceText; // 显示数值，如 "面子: 80%"
    

    
    /*
    public Image[] maskIcons; // 拖入 Q, W, E, R 的图标 Image
    public Sprite[] activeSprites; // 激活状态的图（发光）
    public Sprite[] inactiveSprites; // 灰暗状态的图
    public Transform activeMaskIndicator; // 一个框框，指示当前选中的是谁
    */
    [SerializeField] private Image bossPortrait;
    [SerializeField] private Image playerPortrait;

    [Header("Feedback")]
    [SerializeField] private Transform feedbackSpawnPoint; // 飘字生成点
    [SerializeField] private GameObject feedbackPrefab;    // 飘字 Prefab

    [Header("Dialogue")]
    [SerializeField] private TextMeshProUGUI bossDialogueText; // Boss气泡文字
    [SerializeField] private CanvasGroup bossDialogueGroup;    // 用来控制气泡淡入淡出
    [SerializeField] private TextMeshProUGUI playerRetortText; // 玩家回怼文字
    [SerializeField] private CanvasGroup playerDialogueGroup;

    [Header("Danmaku System")]
    public GameObject danmakuPrefab;      // 拖入 Danmaku_Prefab
    public Transform danmakuContainer;    // 拖入一个全屏透明 Panel 作为父物体

    [Header("Settings")]
    public float minSpeed = 3f; // 最快3秒飞过
    public float maxSpeed = 6f; // 最慢6秒飞过

    // 弹幕生成的上下边界 (Y轴)，防止弹幕飞到UI外面或者挡住重要信息
    public float topY = 400f;
    public float bottomY = -200f;

    [Header("Emoji System")]
    public GameObject emojiPrefab;        // 拖入 Emoji_Prefab
    public Transform emojiContainer;      // 建议新建一个透明 Panel 专门放表情
    public List<EmojiData> emojiLibrary;  // 在这里配置所有的表情包

    [Header("=== 资源配置 (美术相关) ===")]
    public Color colorPerfect = Color.red;
    public Color colorGood = Color.yellow;
    public Color colorMiss = Color.black;

    CameraShake cameraShaker;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }
    private void Start()
    {
        btnPause.onClick.AddListener(OnClickPause);
        cameraShaker = Camera.main.GetComponent<CameraShake>();

        EventCenter.Instance.AddEventListener<int>(GameEvents.OnDoDMG, DoDmg);
        EventCenter.Instance.AddEventListener<int>(GameEvents.OnCure, BeDmged);
        EventCenter.Instance.AddEventListener<string>(GameEvents.OnShowEmoji, ShowEmoji);
    }

    

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<int>(GameEvents.OnDoDMG,DoDmg);
        EventCenter.Instance.RemoveEventListener<int>(GameEvents.OnCure, BeDmged);
        EventCenter.Instance.RemoveEventListener<string>(GameEvents.OnShowEmoji, ShowEmoji);
    }
    private void OnClickPause()
    {
        // 1. 播放按钮点击音效 (如果有 AudioManager)
        // AudioManager.Instance.Play("UI_Click");

        // 2. 按钮点击反馈 (变小一下)
        btnPause.transform.DOPunchScale(Vector3.one * -0.1f, 0.1f).OnComplete(() =>
        {
            // 3. 通知 UIManager 切换状态
            // 注意：这里我们只负责通知 UI 变化，具体的“开始游戏逻辑”交给 Manager 协调
            UIManager.Instance.TogglePausePanel();
        });
    }

    private void DoDmg(int blood)
    {
        Debug.Log("通知对UI更新00");
        Debug.Log($"收到伤害: {blood}");
        UIManager.Instance.bossStats.Modify(-blood/1f);
        
        //UpdateBossHP(blood/100f);
    }

    private void BeDmged(int blood)
    {
        UIManager.Instance.playerStats.Modify(blood / 1f);
        //UpdatePlayerHP(blood / 100f);
    }
    #region 1.血条机制
    /// <summary>
    /// 更新 Boss 面子条
    /// </summary>
    /// <param name="percent">0.0 到 1.0 的百分比</param>
    public void UpdateBossHP(float percent)
    {
        // 1. 前景条立刻变（或快速变）
        //bossHpFill.DOFillAmount(percent, 0.2f);
        Debug.Log("通知对UI更新04");

        // 2. 背景缓冲条延迟跟随 (传统的打击感血条)
        bossHpRedBar.DOFillAmount(percent, 0.5f).SetDelay(0.2f).SetEase(Ease.OutCirc);

        // 3. 立绘受击震动
        bossPortrait.transform.DOShakePosition(0.3f, 10f);
        bossPortrait.DOColor(Color.red, 0.1f).OnComplete(() => bossPortrait.DOColor(Color.white, 0.1f));

        //4.文本改变
        float hp = percent * 100;
        bossFaceText.text = hp.ToString();

        //5.镜头摇晃
        OnCameraShake(0.2f, 0.5f);

}
public void OnCameraShake(float duration,float magnitude)
    {
        if(cameraShaker != null)
        {
            StartCoroutine(cameraShaker.Shake(duration, magnitude));
        }
        
    }

/// <summary>
/// 更新玩家 面子条
/// </summary>
/// <param name="percent">0.0 到 1.0</param>
public void UpdatePlayerHP(float percent)
    {
        
        playerFaceFill.DOFillAmount(percent, 0.3f);

        //抖动
        playerPortrait.transform.DOShakePosition(0.3f, 10f);
        playerPortrait.DOColor(Color.yellow, 0.1f).OnComplete(() => bossPortrait.DOColor(Color.white, 0.1f));

        // 如果血量危急，开始闪烁
        if (percent < 0.2f)
        {
            // 简单的心跳效果
            playerFaceFill.transform.DOScale(1.1f, 0.5f).SetLoops(2, LoopType.Yoyo);
        }
    }


    #endregion

    
    #region 2. 判定反馈与飘字

    // 1. 公开方法：增加了 Vector3? customPos 参数
    // 'Vector3?' 表示这个参数可以是 null
    public void ShowHitFeedback(int type, Vector3? customPos = null)
    {
        string text = "";
        Color color = Color.white;
        float punchScale = 1f;

        switch (type)
        {
            case 0:
                text = "失误！";
                color = colorMiss;
                punchScale = 1.0f;
                break;
            case 1:
                text = "一般";
                color = colorGood;
                punchScale = 1.2f;
                break;
            case 2:
                text = "完美！";
                color = colorPerfect;
                punchScale = 1.5f;
                break;
        }

        // 逻辑判断：如果传入了自定义位置，就用传入的；否则用默认的挂点位置
        Vector3 finalPos = customPos.HasValue ? customPos.Value : feedbackSpawnPoint.position;

        CreateFloatingText(text, color, punchScale, finalPos);
    }

    // 2. 私有方法：增加了 pos 参数
    private void CreateFloatingText(string content, Color color, float scaleMult, Vector3 pos)
    {
        // 注意：这里 Instantiate 的父物体依然设为 feedbackSpawnPoint.parent
        // 是为了保证它在 Hierarchy 里整齐，且由 Canvas 渲染
        GameObject obj = Instantiate(feedbackPrefab, feedbackSpawnPoint.parent);

        TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();

        tmp.text = content;
        tmp.color = color;
        obj.transform.localScale = Vector3.one * scaleMult;

        // 【关键修改 1】：直接设置世界坐标
        obj.transform.position = pos;

       
        obj.transform.DOLocalMoveY(100f, 0.8f).SetRelative(true).SetEase(Ease.OutCirc);

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg) cg.DOFade(0f, 0.5f).SetDelay(0.3f);

        Destroy(obj, 1.0f);
    }
    #endregion


    #region 3. 对话与剧情

    /// <summary>
    /// Boss 说话（显示气泡）
    /// </summary>
    /// <param name="content">台词内容</param>
    /// <param name="duration">显示几秒</param>
    public void ShowBossDialogue(string content, float duration = 2f)
    {
        bossDialogueText.text = content;

        // 弹入动画
        bossDialogueGroup.alpha = 1f;
        bossDialogueGroup.transform.localScale = Vector3.zero;
        bossDialogueGroup.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBounce);

        // 自动消失（如果需要的话，或者由逻辑层手动调用 Hide）
        CancelInvoke(nameof(HideBossDialogue));
        Invoke(nameof(HideBossDialogue), duration);
    }

    private void HideBossDialogue()
    {
        bossDialogueGroup.DOFade(0f, 0.3f);
    }

    /// <summary>
    /// 玩家回怼（文字弹幕效果）
    /// </summary>
    public void ShowPlayerRetort(string content,float duration = 2f)
    {
        playerDialogueGroup.alpha = 1f;
        playerDialogueGroup.transform.localScale = Vector3.zero;
        playerDialogueGroup.transform.DOScale(1f, 0.2f).SetEase(Ease.InCubic);

        playerRetortText.text = content;
        /*
        // 像漫画一样具有冲击力地出现
        playerRetortText.transform.localScale = Vector3.one * 2f;
        playerRetortText.alpha = 0f;

        playerRetortText.transform.DOScale(1f, 0.2f).SetEase(Ease.InCubic);
        playerRetortText.DOFade(1f, 0.1f);

        // 1秒后淡出
        playerRetortText.DOFade(0f, 0.5f).SetDelay(1.0f);
        */
        // 自动消失（如果需要的话，或者由逻辑层手动调用 Hide）
        CancelInvoke(nameof(HidePlayerDialogue));
        Invoke(nameof(HidePlayerDialogue), duration);
    }

    private void HidePlayerDialogue()
    {
        playerDialogueGroup.DOFade(0f, 0.3f);
    }
    #endregion

    #region 4.弹幕对话
    /// <summary>
    /// Boss 发射弹幕攻击
    /// </summary>
    public void SpawnBossDanmaku(string text)
    {
        // 1. 随机高度
        float randomY = UnityEngine.Random.Range(bottomY, topY);

        // 2. 随机速度
        float duration = UnityEngine.Random.Range(minSpeed, maxSpeed);

        // 3. 生成并发射
        CreateDanmaku(text, Color.white, duration, randomY);
    }

    /// <summary>
    /// 玩家回怼弹幕 (通常更快、更显眼)
    /// </summary>
    public void SpawnPlayerDanmaku(string text)
    {
        // 玩家的弹幕可以固定在某个高度，或者用特殊的金色
        float fixedY = 200f; // 屏幕正中间
        float fastSpeed = 2.0f; // 快速反击

        CreateDanmaku(text, Color.yellow, fastSpeed, fixedY);
    }

    // 内部通用生成方法
    private void CreateDanmaku(string content, Color color, float duration, float yPos)
    {
        GameObject obj = Instantiate(danmakuPrefab, danmakuContainer);
        DanmakuBullet bullet = obj.GetComponent<DanmakuBullet>();

        if (bullet != null)
        {
            bullet.Fire(content, color, duration, yPos);
        }
    }
    #endregion

    #region 5.表情包系统

    private void ShowEmoji(string name)
    {
        //PerfectReply 完美回怼
        //NormalReply 普通表情包
        //HurtReply 受伤表情包
        //PerfectChaos 完美乱回
        //PerfectEat 完美吃饭
        //None 无
        SpawnEmoji(name);
    }

    public void SpawnEmoji(string id, Vector3? spawnPos = null)
    {
        // 1. 查找对应的 Sprite
        Sprite targetSprite = null;
        foreach (var data in emojiLibrary)
        {
            if (data.id == id)
            {
                targetSprite = data.sprite;
                break;
            }
        }

        if (targetSprite == null)
        {
            Debug.LogWarning($"找不到 ID 为 {id} 的表情包，请检查拼写！");
            return;
        }

        // 2. 生成物体
        GameObject obj = Instantiate(emojiPrefab, emojiContainer);

        // 3. 设置位置 (注意：如果 emojiContainer 有 Layout Group，这行会失效，请确保 Container 只是普通 Panel)
        //obj.transform.position = spawnPos;

        // 4. 初始化动画
        EmojiObject script = obj.GetComponent<EmojiObject>();
        if (script) script.Init(targetSprite);
    }
    #endregion
}