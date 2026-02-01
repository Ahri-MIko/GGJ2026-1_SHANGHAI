using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PausePanel : BasePanel
{
    [Header("Buttons")]
    public Button btnResume;
    public Button btnRestart;
    public Button btnQuit;

    [Header("Animation")]
    public Transform container; // 拖拽那个包含按钮的父物体

    private void Start()
    {
        btnResume.onClick.AddListener(OnResumeClicked);
        btnRestart.onClick.AddListener(OnRestartClicked);
        btnQuit.onClick.AddListener(OnQuitClicked);
    }

    // 重写 Show，增加弹窗动画和暂停逻辑
    public override void Show()
    {
        base.Show(); // 处理 CanvasGroup 的淡入

        // 1. 弹窗缩放动画 (Juice)
        if (container != null)
        {
            container.localScale = Vector3.zero;
            container.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true); // SetUpdate(true) 保证暂停时动画能动
        }

        
    }

    // 重写 Hide，恢复时间
    public override void Hide()
    {
        // 1. 缩放消失动画
        if (container != null)
        {
            container.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                base.Hide(); // 动画播完再隐藏 CanvasGroup
            });
        }
        else
        {
            base.Hide();
        }

      
    }

    // --- 按钮事件 ---

    private void OnResumeClicked()
    {
        // 调用 UIManager 关闭自己
        UIManager.Instance.TogglePausePanel();
    }

    private void OnRestartClicked()
    {
        // 恢复时间 (非常重要！否则重开后游戏是暂停的)
        //Time.timeScale = 1f;
        AudioListener.pause = false;

        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnQuitClicked()
    {
        // 恢复时间
        //Time.timeScale = 1f;
        AudioListener.pause = false;

        // 通知 UIManager 切换回标题画面
        // 注意：这里需要先关闭暂停界面，再切界面
        UIManager.Instance.OnBackToTitle();

        
    }
}