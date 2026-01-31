using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频管理器 - 单例模式
/// </summary>
public class AudioManager : SingletonMono<AudioManager>
{
    [Header("音频数据")]
    [Tooltip("音频数据ScriptableObject")]
    [SerializeField] private SoundData soundData;

    [Header("音频源")]
    [Tooltip("BGM音频源")]
    [SerializeField] private AudioSource bgmSource;
    
    [Tooltip("音效音频源")]
    [SerializeField] private AudioSource sfxSource;

    [Header("音量设置")]
    [Tooltip("BGM总音量")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmMasterVolume = 1f;
    
    [Tooltip("音效总音量")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxMasterVolume = 1f;

    // 缓存字典
    private Dictionary<string, BGMData> bgmDictionary;
    private Dictionary<string, SoundEffectData> sfxDictionary;
    
    // 当前播放的BGM
    private string currentBGMName = "";

    protected override void Awake()
    {
        base.Awake();
        InitializeAudioSources();
        BuildDictionaries();
    }

    /// <summary>
    /// 初始化音频源
    /// </summary>
    private void InitializeAudioSources()
    {
        // 如果没有设置音频源，自动创建
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_AudioSource");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_AudioSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// 构建音频字典
    /// </summary>
    private void BuildDictionaries()
    {
        if (soundData == null)
        {
            Debug.LogWarning("AudioManager: SoundData未设置！");
            return;
        }

        // 构建BGM字典
        bgmDictionary = new Dictionary<string, BGMData>();
        if (soundData.bgmList != null)
        {
            foreach (var bgm in soundData.bgmList)
            {
                if (!string.IsNullOrEmpty(bgm.bgmName) && bgm.clip != null)
                {
                    if (!bgmDictionary.ContainsKey(bgm.bgmName))
                    {
                        bgmDictionary.Add(bgm.bgmName, bgm);
                    }
                    else
                    {
                        Debug.LogWarning($"AudioManager: BGM名称重复 - {bgm.bgmName}");
                    }
                }
            }
        }

        // 构建音效字典
        sfxDictionary = new Dictionary<string, SoundEffectData>();
        if (soundData.soundEffectList != null)
        {
            foreach (var sfx in soundData.soundEffectList)
            {
                if (!string.IsNullOrEmpty(sfx.soundName) && sfx.clip != null)
                {
                    if (!sfxDictionary.ContainsKey(sfx.soundName))
                    {
                        sfxDictionary.Add(sfx.soundName, sfx);
                    }
                    else
                    {
                        Debug.LogWarning($"AudioManager: 音效名称重复 - {sfx.soundName}");
                    }
                }
            }
        }

        Debug.Log($"AudioManager初始化完成: {bgmDictionary.Count} BGM, {sfxDictionary.Count} 音效");
    }

    #region BGM控制

    /// <summary>
    /// 播放BGM
    /// </summary>
    /// <param name="bgmName">BGM名称</param>
    /// <param name="fadeTime">淡入时间（秒）</param>
    public void PlayBGM(string bgmName, float fadeTime = 0f)
    {
        if (bgmDictionary == null || !bgmDictionary.ContainsKey(bgmName))
        {
            Debug.LogWarning($"AudioManager: 找不到BGM - {bgmName}");
            return;
        }

        BGMData bgmData = bgmDictionary[bgmName];

        // 如果已经在播放相同的BGM，不重复播放
        if (currentBGMName == bgmName && bgmSource.isPlaying)
        {
            return;
        }

        // 停止当前BGM
        if (fadeTime > 0f)
        {
            StartCoroutine(FadeOutAndPlay(bgmData, fadeTime));
        }
        else
        {
            PlayBGMImmediate(bgmData);
        }

        currentBGMName = bgmName;
    }

    /// <summary>
    /// 立即播放BGM
    /// </summary>
    private void PlayBGMImmediate(BGMData bgmData)
    {
        bgmSource.clip = bgmData.clip;
        bgmSource.volume = bgmData.volume * bgmMasterVolume;
        bgmSource.loop = bgmData.loop;
        bgmSource.Play();
        Debug.Log($"播放BGM: {bgmData.bgmName}");
    }

    /// <summary>
    /// 淡出当前BGM并播放新的
    /// </summary>
    private System.Collections.IEnumerator FadeOutAndPlay(BGMData newBgm, float fadeTime)
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;

        // 淡出
        while (timer < fadeTime && bgmSource.isPlaying)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }

        // 播放新BGM
        PlayBGMImmediate(newBgm);

        // 淡入
        timer = 0f;
        float targetVolume = newBgm.volume * bgmMasterVolume;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, timer / fadeTime);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    /// <summary>
    /// 停止BGM
    /// </summary>
    /// <param name="fadeTime">淡出时间（秒）</param>
    public void StopBGM(float fadeTime = 0f)
    {
        if (fadeTime > 0f)
        {
            StartCoroutine(FadeOutBGM(fadeTime));
        }
        else
        {
            bgmSource.Stop();
            currentBGMName = "";
        }
    }

    /// <summary>
    /// 淡出BGM
    /// </summary>
    private System.Collections.IEnumerator FadeOutBGM(float fadeTime)
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }

        bgmSource.Stop();
        currentBGMName = "";
    }

    /// <summary>
    /// 暂停BGM
    /// </summary>
    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    /// <summary>
    /// 恢复BGM
    /// </summary>
    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    #endregion

    #region 音效控制

    /// <summary>
    /// 播放音效（OneShot）
    /// </summary>
    /// <param name="soundName">音效名称</param>
    public void PlaySound(string soundName)
    {
        if (sfxDictionary == null || !sfxDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"AudioManager: 找不到音效 - {soundName}");
            return;
        }

        SoundEffectData sfxData = sfxDictionary[soundName];
        float volume = sfxData.volume * sfxMasterVolume;
        
        sfxSource.PlayOneShot(sfxData.clip, volume);
        Debug.Log($"播放音效: {soundName}");
    }

    /// <summary>
    /// 精确时间播放音效（用于音游，使用DSP时间调度）
    /// </summary>
    /// <param name="soundName">音效名称</param>
    /// <param name="dspTime">DSP时间（使用AudioSettings.dspTime）</param>
    public void PlaySoundScheduled(string soundName, double dspTime)
    {
        if (sfxDictionary == null || !sfxDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"AudioManager: 找不到音效 - {soundName}");
            return;
        }

        SoundEffectData sfxData = sfxDictionary[soundName];
        
        // Create a temporary AudioSource for scheduled playback
        GameObject tempObj = new GameObject($"SFX_{soundName}");
        tempObj.transform.SetParent(transform);
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();
        
        tempSource.clip = sfxData.clip;
        tempSource.volume = sfxData.volume * sfxMasterVolume;
        tempSource.playOnAwake = false;
        
        // Schedule the sound to play at exact DSP time
        tempSource.PlayScheduled(dspTime);
        
        // Destroy the temporary object after the clip finishes
        Destroy(tempObj, sfxData.clip.length + 0.1f);
        
        Debug.Log($"Scheduled sound: {soundName} at DSP time {dspTime:F4}");
    }

    /// <summary>
    /// 播放音效（指定AudioClip）
    /// </summary>
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume * sfxMasterVolume);
        }
    }

    #endregion

    #region 音量控制

    /// <summary>
    /// 设置BGM总音量
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmMasterVolume = Mathf.Clamp01(volume);
        
        // 更新当前播放的BGM音量
        if (bgmSource.isPlaying && bgmDictionary != null && bgmDictionary.ContainsKey(currentBGMName))
        {
            bgmSource.volume = bgmDictionary[currentBGMName].volume * bgmMasterVolume;
        }
    }

    /// <summary>
    /// 设置音效总音量
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxMasterVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxMasterVolume;
    }

    /// <summary>
    /// 获取BGM音量
    /// </summary>
    public float GetBGMVolume()
    {
        return bgmMasterVolume;
    }

    /// <summary>
    /// 获取音效音量
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxMasterVolume;
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 获取当前播放的BGM名称
    /// </summary>
    public string GetCurrentBGMName()
    {
        return currentBGMName;
    }

    /// <summary>
    /// BGM是否正在播放
    /// </summary>
    public bool IsBGMPlaying()
    {
        return bgmSource.isPlaying;
    }

    #endregion
}
