using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class ResultPanel : BasePanel
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Image endingImage;

    [Header("Buttons")]
    public Button btnRestart;    // 重试按钮
    public Button btnNextLevel;  // <--- 新增：下一关按钮
    public Button btnQuit;       // 返回标题

    [Header("Statistics")]
    public TextMeshProUGUI perfectCountText;
    public TextMeshProUGUI missCountText;

    public GameObject SucceedImage;
    public GameObject LoseImage;
    private void Start()
    {
        // 绑定事件
        if (btnRestart) btnRestart.onClick.AddListener(OnRestartClicked);
        if (btnNextLevel) btnNextLevel.onClick.AddListener(OnNextLevelClicked);
        if (btnQuit) btnQuit.onClick.AddListener(OnQuitClicked);
    }
    public  void Show(StageEndData data)
    {
        base.Show();
        SetupResult(data);
        
    }

    
    public void SetupResult(StageEndData data)
    {
        int total = data.GetTotalCount();
        float ratio = data.GetPerfectRate();

        //bool isWin = false; // 判断是否过关

        // 1. 根据分数设定结局文字和颜色
        if (ratio >= 0.8f)
        {
            SetEndingUI("S级：家族传说", "你成为了别人家的小孩！", Color.yellow);
        }
        else if (ratio >= 0.5f)
        {
            SetEndingUI("A级：平平淡淡", "也就是个普通人，明年继续被催。", Color.white);
        }
        else
        {
            SetEndingUI("C级：家族之耻", "聚餐只能坐小孩那桌。", Color.gray);
        }

        perfectCountText.text = $"完美回怼: {data.PerfectCount}";
        missCountText.text = $"完美率: {ratio:P2}";

        if(data.IsWin)
        {
            LoseImage.SetActive(false);
            SucceedImage.SetActive(true);
        }
        else
        {
            SucceedImage.SetActive(false);
            LoseImage.SetActive(true);
        }
        /*
        // 2. 核心逻辑：根据胜负控制按钮显示
        if (btnNextLevel)
        {
            if (isWin)
            {
                // 如果赢了，显示「下一关」
                btnNextLevel.gameObject.SetActive(true);
                // 增加一点动效，让按钮弹出来，引导玩家点击
                btnNextLevel.transform.localScale = Vector3.zero;
                btnNextLevel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.5f);
            }
            else
            {
                // 如果输了，隐藏「下一关」，强制重试
                btnNextLevel.gameObject.SetActive(false);
            }
        }
        */
    }

    void SetEndingUI(string title, string desc, Color color)
    {
        titleText.text = title;
        titleText.color = color;
        descText.text = desc;
    }

    // --- 按钮点击事件 ---

    private void OnNextLevelClicked()
    {
        // 呼叫 Manager 进入下一关
        UIManager.Instance.OnNextLevelUI();
    }

    private void OnRestartClicked()
    {
        // 呼叫 Manager 重试本关
        //UIManager.Instance.OnRestartGameUI();
    }

    private void OnQuitClicked()
    {
        UIManager.Instance.OnBackToTitle();
    }
}