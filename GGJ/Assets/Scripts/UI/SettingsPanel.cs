using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsPanel : BasePanel
{
    [Header("UI Components")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button btnSaveAndExit;

    private const string PREF_VOLUME = "MasterVolume"; // 保存用的 Key

    private void Start()
    {
        // 绑定事件
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        btnSaveAndExit.onClick.AddListener(OnSaveAndExitClicked);
    }

    // 重写 Show 方法：每次打开面板时，都要读取当前音量
    public override void Show()
    {
        base.Show(); // 播放弹窗动画

        // 1. 获取当前全局音量 (0.0 ~ 1.0)
        // 如果没有保存过，默认是 1
        float currentVol = PlayerPrefs.GetFloat(PREF_VOLUME, 1f);

        // 2. 更新滑块位置 (但不触发 onValueChanged 事件，防止死循环或不必要的调用)
        volumeSlider.SetValueWithoutNotify(currentVol);
    }

    // 当滑块拖动时：实时调节音量 (这样玩家能边拖边听大小)
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    // 当点击保存并退出时
    private void OnSaveAndExitClicked()
    {
        // 1. 保存到硬盘 (持久化)
        PlayerPrefs.SetFloat(PREF_VOLUME, volumeSlider.value);
        PlayerPrefs.Save();

        // 2. 播放一个点击音效 (可选)
        // AudioManager.Instance.Play("UI_Confirm");

        // 3. 关闭面板
        // 可以直接调 Hide()，也可以通过 UIManager 统一管理
        UIManager.Instance.CloseSettingsPanel();
    }
}