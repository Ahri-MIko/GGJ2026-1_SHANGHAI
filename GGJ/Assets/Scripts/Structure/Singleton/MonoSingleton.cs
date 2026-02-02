using UnityEngine;

/// <summary>
/// 随场景销毁的单例基类
/// <para>特点 1: 切换场景时会自动被销毁</para>
/// <para>特点 2: 如果场景中没有实例，访问 Instance 会自动创建一个空物体挂载脚本</para>
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;

    // 线程锁，防止多线程同时访问导致生成多个实例（虽然Unity主线程单线程，但这是好习惯）
    private static readonly object _lock = new object();

    // 标记程序是否正在退出，防止在退出时再次尝试创建单例导致报错
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                // 如果程序正在退出，返回null，不再创建新实例，防止报错
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // 1. 先尝试在场景里找
                    _instance = FindObjectOfType<T>();

                    if (FindObjectsOfType<T>().Length > 1)
                    {
                        Debug.LogError($"[MonoSingleton] 场景里存在多个 {typeof(T)} 的实例！这也太乱了。");
                        return _instance;
                    }

                    // 2. 如果场景里没有，就自动创建一个
                    if (_instance == null)
                    {
                        GameObject singletonObj = new GameObject();
                        _instance = singletonObj.AddComponent<T>();
                        singletonObj.name = $"(Singleton) {typeof(T)}";

                        Debug.Log($"[MonoSingleton] 场景里没找到 {typeof(T)}，已自动创建一个: {singletonObj.name}");
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            // 如果场景里已经有一个实例了，而又有人手动拖了一个进场景，销毁这多余的一个
            Debug.LogWarning($"[MonoSingleton] 试图创建第二个 {typeof(T)}，已销毁重复项。");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 当脚本被销毁时（场景切换或手动销毁），清空静态引用
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}