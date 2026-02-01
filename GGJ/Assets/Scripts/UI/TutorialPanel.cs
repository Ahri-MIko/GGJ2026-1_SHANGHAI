using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialPanel : BasePanel
{
    [Header("Pages")]
    public GameObject page1; // 第一页的内容容器
    public GameObject page2; // 第二页的内容容器

    [Header("Buttons")]
    public Button btnNext;   // 第一页的“下一步”
    public Button btnPrev;   // 第二页的“上一步” (可选)
    public Button btnClose;  // 第二页的“我知道了/关闭”

    private void Start()
    {
        // 绑定事件
        btnNext.onClick.AddListener(GoToPage2);
        if (btnPrev) btnPrev.onClick.AddListener(GoToPage1);
        btnClose.onClick.AddListener(OnCloseClicked);

        // 初始化状态：显示第一页
        //GoToPage1();
    }

    // 打开面板时，强制重置回第一页
    public override void Show()
    {
        base.Show();
        GoToPage1();
    }

    private void GoToPage1()
    {
        page1.SetActive(true);
        page2.SetActive(false);
    }

    private void GoToPage2()
    {
        page1.SetActive(false);
        page2.SetActive(true);
    }

    private void OnCloseClicked()
    {
        // 调用基类方法关闭自己
        Hide();
        PlayerPrefs.SetInt("HasSeenTutorial", 1);
        PlayerPrefs.Save();
        // 可选：播放一个音效
        // AudioManager.Instance.Play("UI_Close");
    }
}