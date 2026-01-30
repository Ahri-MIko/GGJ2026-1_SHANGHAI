using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 事件信息基类（所有事件信息的公共父类）
/// 作用：提供类型约束，实现统一存储
/// </summary>
public abstract class EventInfoBase { }

/// <summary>
/// 泛型事件信息容器（封装带参数的事件处理委托）
/// 设计要点：
///   1. 使用泛型支持多种参数类型
///   2. 通过UnityAction<T>存储事件响应方法
/// </summary>
/// <typeparam name="T">事件参数类型</typeparam>
public class EventInfo<T> : EventInfoBase
{
    // 存储所有注册的事件处理方法
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

/// <summary>
/// 无参事件信息容器（封装无参数的事件处理委托）
/// 使用场景：不需要传递参数的事件通知
/// </summary>
public class EventInfo : EventInfoBase
{
    // 存储所有注册的无参事件处理方法
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

/// <summary>
/// 事件中心系统（单例模式）
/// 核心功能：
///   1. 统一的事件注册/注销接口
///   2. 安全的事件触发机制
///   3. 支持带参数和无参数事件
///   4. 类型安全的泛型处理
/// 
/// 协作规范：
///   1. 事件名称建议使用常量字符串（避免拼写错误）
///   2. 参数类型需与事件声明类型一致
///   3. 注意及时注销事件防止内存泄漏
/// </summary>
public class EventCenter : Singleton<EventCenter>
{
    // 事件字典：<事件名称, 事件处理器容器>
    private Dictionary<string, EventInfoBase> eventDic = new Dictionary<string, EventInfoBase>();

    /// <summary>
    /// 触发带参数事件
    /// 执行流程：
    ///   1. 检查事件是否已注册
    ///   2. 安全调用所有注册方法
    ///   3. 自动传递事件参数
    /// </summary>
    /// <typeparam name="T">事件参数类型</typeparam>
    /// <param name="eventName">事件标识符</param>
    /// <param name="info">事件参数数据</param>
    public void EventTrigger<T>(string eventName, T info)
    {
        // 安全检查：只触发已注册事件
        if (eventDic.TryGetValue(eventName, out EventInfoBase eventInfo))
        {
            // 类型转换并调用委托链
            (eventInfo as EventInfo<T>)?.actions?.Invoke(info);
        }
    }

    /// <summary>
    /// 触发无参数事件
    /// </summary>
    /// <param name="eventName">事件标识符</param>
    public void EventTrigger(string eventName)
    {
        if (eventDic.TryGetValue(eventName, out EventInfoBase eventInfo))
        {
            (eventInfo as EventInfo)?.actions?.Invoke();
        }
    }

    /// <summary>
    /// 注册带参数事件监听器
    /// 重要：确保T类型与事件触发时一致
    /// </summary>
    public void AddEventListener<T>(string eventName, UnityAction<T> func)
    {
        if (eventDic.TryGetValue(eventName, out EventInfoBase eventInfo))
        {
            // 向现有事件容器添加处理器
            (eventInfo as EventInfo<T>).actions += func;
        }
        else
        {
            // 创建新事件容器并注册
            eventDic.Add(eventName, new EventInfo<T>(func));
        }
    }

    /// <summary>
    /// 注册无参数事件监听器
    /// </summary>
    public void AddEventListener(string eventName, UnityAction func)
    {
        if (eventDic.TryGetValue(eventName, out EventInfoBase eventInfo))
        {
            (eventInfo as EventInfo).actions += func;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo(func));
        }
    }

    /// <summary>
    /// 注销带参数事件监听器
    /// 注意：注销方法必须与注册方法完全匹配
    /// </summary>
    public void RemoveEventListener<T>(string eventName, UnityAction<T> func)
    {
        if (eventDic.TryGetValue(eventName, out EventInfoBase eventInfo))
        {
            (eventInfo as EventInfo<T>).actions -= func;
        }
    }

    /// <summary>
    /// 注销无参数事件监听器
    /// </summary>
    public void RemoveEventListener(string eventName, UnityAction func)
    {
        if (eventDic.TryGetValue(eventName, out EventInfoBase eventInfo))
        {
            (eventInfo as EventInfo).actions -= func;
        }
    }

    /// <summary>
    /// 清空所有事件监听
    /// 使用场景：场景切换/游戏重置
    /// 注意：会移除所有已注册事件
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }

    /// <summary>
    /// 清除指定事件的所有监听
    /// </summary>
    public void Clear(string eventName)
    {
        eventDic.Remove(eventName);
    }

    public void ClearAll()
    {
        eventDic.Clear();
        return;
    }
}