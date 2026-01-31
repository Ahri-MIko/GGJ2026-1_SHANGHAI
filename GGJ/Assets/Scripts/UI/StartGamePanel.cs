using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System; // 必装

public class StartPanel : BasePanel
{
    [Header("UI Elements")]
    public Button startButton;
    public Button quitButton;
    public Button settingsButton;
    public Transform titleImage; // 标题图片，用来做动效

    private void Start()
    {
        // 绑定按钮事件
        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        // --- 标题动效 (Idle Animation) ---
        // 让标题像呼吸一样轻微缩放，增加动感
        if (titleImage != null)
        {
            titleImage.DOScale(1.05f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }

    

    private void OnStartClicked()
    {
        // 1. 播放按钮点击音效 (如果有 AudioManager)
        // AudioManager.Instance.Play("UI_Click");

        // 2. 按钮点击反馈 (变小一下)
        startButton.transform.DOPunchScale(Vector3.one * -0.1f, 0.1f).OnComplete(() =>
        {
            // 3. 通知 UIManager 切换状态
            // 注意：这里我们只负责通知 UI 变化，具体的“开始游戏逻辑”交给 Manager 协调
            UIManager.Instance.OnStartGameUI();
        });
    }

    private void OnQuitClicked()
    {
        // 退出游戏
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnSettingsClicked()
    {
        // 1. 播放按钮点击音效 (如果有 AudioManager)
        // AudioManager.Instance.Play("UI_Click");

        // 2. 按钮点击反馈 (变小一下)
        startButton.transform.DOPunchScale(Vector3.one * -0.1f, 0.1f).OnComplete(() =>
        {
            // 3. 通知 UIManager 切换状态
            UIManager.Instance.OpenSettingsPanel();
        });
    }
}