using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // 必装 DOTween
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //[SerializeField] private Image[] maskIcons;    // QWER 四个图标
    //[SerializeField] private Transform maskHighlight; // 选中框

    [Header("=== 面板管理  ===")]
    [SerializeField] private StartPanel startPanel;      // 拖拽 StartPanel
    [SerializeField] private GameplayPanel gameplayPanel; // 拖拽 GameplayPanel
    [SerializeField] private ResultPanel resultPanel;     // 拖拽 ResultPanel
    [SerializeField] private PausePanel pausePanel;
    [SerializeField] private SettingsPanel settingsPanel;
    [SerializeField] private TutorialPanel tutorialPanel;



    void Awake()
    {
        // 单例模式，保证逻辑同学随处可调
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        // 读取上次保存的音量，如果没有则默认 1.0 (最大声)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = savedVolume;

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "UITest01") // 或者你的 TitleScene 名字
        {
            // 1. 始终显示开始界面
            ShowStartPanel();

            // 2. 【核心修改】检查是否看过教学
            // GetInt 第一个参数是 Key，第二个参数是默认值 (0代表没看过)
            if (PlayerPrefs.GetInt("HasSeenTutorial", 0) == 0)
            {
                ShowTutorialPanel();
            }
            else
            {
                // 如果看过，确保它隐藏（防止在编辑器里忘了关）
                if (tutorialPanel) tutorialPanel.Hide();
            }
        }
     
        else if (sceneName == "UITest02")
        {
            // 在遊戲場景，隱藏開始介面，顯示戰鬥介面
            if (startPanel) startPanel.Hide();
            if (gameplayPanel) gameplayPanel.Show();

            // 通知邏輯層：遊戲開始了！
            // GameManager.Instance.StartLevel(); 
        }
    }

    private void Update()
    {
        // 开发作弊码：按下 F12 清除教学记录
        if (Input.GetKeyDown(KeyCode.F12))
        {
            PlayerPrefs.DeleteKey("HasSeenTutorial");
            Debug.Log("教学记录已重置！下次启动会再次显示。");
        }
    }

    public void LoadScene(SceneName scene)
    {
        // 枚举转字符串
        string sceneNameStr = scene.ToString();

        // 加载场景
        SceneManager.LoadScene(sceneNameStr);

        // 可以在这里统一处理一些清理工作，比如重置时间
        Time.timeScale = 1f;
    }
    // =====================================
    //              API 区域
    // =====================================

    public void UpdateBossHP(float percent)
    {
        // 核心修改：UIManager 不干活，直接转包给 gameplayPanel
        if (gameplayPanel != null)
        {
            gameplayPanel.UpdateBossHP(percent);
        }
    }

    public void UpdatePlayerHP(float percent)
    {
        if (gameplayPanel != null)
        {
            gameplayPanel.UpdatePlayerHP(percent);
        }
    }




    #region 2. 判定反馈与飘字

    /// <summary>
    /// 显示判定结果
    /// </summary>
    /// <param name="type">0=Miss, 1=Good, 2=Perfect</param>
    public void ShowHitFeedback(int type, Vector3? customPos = null)
    {
        if (gameplayPanel != null)
        {
            gameplayPanel.ShowHitFeedback(type,customPos);
        }
    }


    #endregion

    #region 3. 对话与剧情

    // Boss 说话
    public void ShowBossDialogue(string content)
    {
        if (gameplayPanel != null)
        {
            // 现在改为发射弹幕
            gameplayPanel.SpawnBossDanmaku(content);
        }
    }

    // 玩家回怼
    public void ShowPlayerRetort(string content)
    {
        if (gameplayPanel != null)
        {
            gameplayPanel.SpawnPlayerDanmaku(content);
        }
    }

    /// <summary>
    /// 弹出表情包
    /// </summary>
    /// <param name="emojiID">你在 Inspector 里填的 ID，例如 "Angry"</param>
    /// <param name="targetPos">如果不填，默认出现在屏幕中间</param>
    public void ShowEmoji(string emojiID, Vector3? targetPos = null)
    {
        if (gameplayPanel != null)
        {
            // 如果没传位置，默认取屏幕中心 (或者你可以指定 Boss 的位置)
            Vector3 finalPos = targetPos.HasValue ? targetPos.Value : new Vector3(Screen.width / 2, Screen.height / 2, 0);

            gameplayPanel.SpawnEmoji(emojiID, finalPos);
        }
    }
    #endregion

    #region 4. 面板调用
    public void ShowStartPanel()
    {
        
        if (startPanel) startPanel.Show();
        if (gameplayPanel) gameplayPanel.Hide();
        if (resultPanel) resultPanel.Hide();
   
    }

    public void ShowTutorialPanel()
    {
        if ((tutorialPanel != null))
        {
            tutorialPanel.Show();
        }
    }

    // 当点击“开始游戏”按钮时触发
    public void OnStartLevel01()
    {
        // 1. UI 层面：隐藏开始，显示战斗
        //if (startPanel) startPanel.Hide();
        //if (gameplayPanel) gameplayPanel.Show();
        // 2. 逻辑层面：通知另一位程序同学的 GameManager
        // GameManager.Instance.StartGameLogic(); 
        LoadScene(SceneName.UITest02);
        Debug.Log("UI状态已切换：进入战斗");
    }

    public void OnStartLevel02()
    {
        // 1. UI 层面：隐藏开始，显示战斗
        //if (startPanel) startPanel.Hide();
        //if (gameplayPanel) gameplayPanel.Show();
        // 2. 逻辑层面：通知另一位程序同学的 GameManager
        // GameManager.Instance.StartGameLogic(); 
        LoadScene(SceneName.UITest03);
        Debug.Log("UI状态已切换：进入战斗");
    }

    // 提供给逻辑层调用的：切换暂停状态
    public void TogglePausePanel()
    {
        if (pausePanel == null) return;

        // 如果当前是激活的，就关闭；否则打开
        if (pausePanel.gameObject.activeSelf)
        {
            pausePanel.Hide();
        }
        else
        {
            pausePanel.Show();
        }
    }

    // 从暂停界面回标题
    public void OnBackToTitle()
    {
        // 1. 关闭暂停界面 (不需要动画，直接关)
        pausePanel.gameObject.SetActive(false);

        // 2. 关闭游戏界面
        //if (gameplayPanel) gameplayPanel.Hide();
        //if (resultPanel) resultPanel.Hide();

        // 3. 打开标题界面
        //if (startPanel) startPanel.Show();

        // 4. 通知逻辑层重置 (可选)
        // GameManager.Instance.ResetGame();

        LoadScene(SceneName.UITest01);  
    }

    // 打开设置 (给标题界面的“设置”按钮，或者游戏中的暂停界面用)
    public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.Show();
        }
    }

    // 关闭设置 (给 SettingsPanel 内部调用)
    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.Hide();
        }
    }

    /// <summary>
    /// 當玩家點擊「下一關」時觸發
    /// </summary>
    public void OnNextLevelUI()
    {
        // 1. 關閉結算介面
        if (resultPanel) resultPanel.Hide();

        // 2. 打開戰鬥介面 (準備開始新的一關)
        if (gameplayPanel) gameplayPanel.Show();

        // 3. 通知邏輯層加載下一關
        // 請告訴另一位程式同學在 GameManager 裡實作 LoadNextLevel()
        Debug.Log("UI通知：請求進入下一關");

        // 範例調用：
        // GameManager.Instance.LoadNextLevel();
    }
    #endregion
}