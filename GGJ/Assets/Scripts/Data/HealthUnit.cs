using UnityEngine;
using System;
using UnityEngine.EventSystems; // 用于 Action 事件

[System.Serializable]
public class HealthUnit
{
    [Header("Settings")]
    public float maxFace = 100f; // 最大面子值

    [Header("Runtime (Read Only)")]
    [SerializeField] private BindableProperty<float> currentFace; // 当前面子值

    // --- 事件：当血量变化时触发 ---
    // 参数 float 是当前的百分比 (0.0 ~ 1.0)，方便 UI 条直接用
    public event Action<float> OnFaceChanged;

    // --- 事件：当血量归零时触发 ---
    public event Action OnFaceDepleted;

    /// <summary>
    /// 初始化（在游戏开始时调用）
    /// </summary>
    public void Init()
    {
        currentFace = new BindableProperty<float>(maxFace);
        //currentFace = maxFace;
        NotifyUI();
    }

    /// <summary>
    /// 修改面子值（扣血传负数，回血传正数）
    /// </summary>
    public void Modify(float amount)
    {
        currentFace.Value += amount;

        // 限制范围：不能小于0，不能超过最大值
        currentFace.Value = Mathf.Clamp(currentFace.Value, 0f, maxFace);

        // 通知 UI 更新
        Debug.Log("通知对UI更新01");
        NotifyUI();

        // 检查是否挂了
        if (currentFace.Value <= 0)
        {
            OnFaceDepleted?.Invoke();
        }
    }

    /// <summary>
    /// 强制设置一个值（比如作弊）
    /// </summary>
    public void SetValue(float value)
    {
        currentFace.Value = Mathf.Clamp(value, 0f, maxFace);
        NotifyUI();
    }

    // 内部辅助函数：计算百分比并触发事件
    private void NotifyUI()
    {
        float percent = currentFace.Value / maxFace;
        OnFaceChanged?.Invoke(percent);
        
        //Debug.Log("通知对UI更新02");
    }

    // 获取当前具体数值（如果需要显示数字）
    public float Current => currentFace.Value;
}