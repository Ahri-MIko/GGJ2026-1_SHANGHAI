using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindableProperty<T>
{
    private T mValue = default(T);
    public Action<T> onValueChanged;
    // 增加构造函数
    public BindableProperty(T defaultValue = default(T)) { mValue = defaultValue; }
    public T Value
    {
        get { return mValue; }
        set
        {
            if (!value.Equals(mValue))
            {
                mValue = value;
                onValueChanged?.Invoke(mValue);
            }
        }
    }

    /// <summary>
    /// 强制触发值变化事件
    /// 即使值没有改变也会触发（用于集合类型修改后重新通知）
    /// </summary>
    public void ForceNotify()
    {
        onValueChanged?.Invoke(mValue);
    }
}

/// <summary>
/// 可序列化的BindableProperty包装类
/// 允许在Inspector中查看和编辑初始值
/// </summary>
[System.Serializable]
public class SerializableBindableProperty<T>
{
    [SerializeField]
    private T initialValue = default(T);

    [SerializeField]
    private T debugRuntimeValue = default(T);

    private BindableProperty<T> property;

    /// <summary>
    /// 获取或初始化BindableProperty
    /// </summary>
    public BindableProperty<T> GetProperty()
    {
        if (property == null)
        {
            property = new BindableProperty<T>();
            property.Value = initialValue;
        }
        return property;
    }

    /// <summary>
    /// 获取当前值
    /// </summary>
    public T GetValue()
    {
        return GetProperty().Value;
    }

    /// <summary>
    /// 设置值
    /// </summary>
    public void SetValue(T value)
    {
        GetProperty().Value = value;
        UpdateDebugValue();
    }

    /// <summary>
    /// 订阅值变化事件
    /// </summary>
    public void OnValueChanged(System.Action<T> callback)
    {
        GetProperty().onValueChanged += callback;
    }

    /// <summary>
    /// 在Editor中设置初始值（调试用）
    /// </summary>
    public void SetInitialValue(T value)
    {
        initialValue = value;
    }

    /// <summary>
    /// 更新调试用的运行时值显示
    /// 在获取值时自动调用
    /// </summary>
    public void UpdateDebugValue()
    {
        if (property != null)
        {
            debugRuntimeValue = property.Value;
        }
    }
}