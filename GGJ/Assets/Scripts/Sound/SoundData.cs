using UnityEngine;

/// <summary>
/// 音频数据 - ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/Sound Data", order = 1)]
public class SoundData : ScriptableObject
{
    [Header("背景音乐（BGM）")]
    [Tooltip("BGM列表")]
    public BGMData[] bgmList;

    [Header("音效（Sound Effects）")]
    [Tooltip("音效列表")]
    public SoundEffectData[] soundEffectList;
    
    [Header("音频池（Audio Pool）")]
    [Tooltip("音频池 - 用于存储带ID的音频")]
    public AudioPoolEntry[] audioPool;
}

/// <summary>
/// BGM数据
/// </summary>
[System.Serializable]
public class BGMData
{
    [Tooltip("BGM名称标识")]
    public string bgmName;
    
    [Tooltip("音频片段")]
    public AudioClip clip;
    
    [Tooltip("音量（0-1）")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Tooltip("是否循环")]
    public bool loop = true;
}

/// <summary>
/// 音效数据
/// </summary>
[System.Serializable]
public class SoundEffectData
{
    [Tooltip("音效名称标识")]
    public string soundName;
    
    [Tooltip("音频片段")]
    public AudioClip clip;
    
    [Tooltip("音量（0-1）")]
    [Range(0f, 1f)]
    public float volume = 1f;
}

/// <summary>
/// 音频池条目 - 包含 AudioClip 和 ID
/// </summary>
[System.Serializable]
public class AudioPoolEntry
{
    [Tooltip("音频ID标识")]
    public string audioID;
    
    [Tooltip("音频片段")]
    public AudioClip clip;
    
    [Tooltip("音量（0-1）")]
    [Range(0f, 1f)]
    public float volume = 1f;
}
