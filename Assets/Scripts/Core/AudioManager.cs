using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DG.Tweening;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField, Tooltip("Audio source for playing background music (BGM)")]
    private AudioSource BGMPlayer;

    [SerializeField, Tooltip("Audio source for playing sound effects (SFX)")]
    private AudioSource SFXPlayer;

    [SerializeField, Tooltip("Duration for fading in/out background music")]
    private float fadeDuration = 0.75f;

    [SerializeField, Tooltip("List of sound effects data")]
    private List<AudioData> SFXList;

    [Header("音量バランス設定")]
    [SerializeField, Tooltip("音量バランス機能を有効にする")]
    private bool enableVolumeBalancing = true;
    
    [SerializeField, Tooltip("BGMの基準音量"), Range(0.1f, 1f)]
    private float baseBGMVolume = 0.7f;
    
    [SerializeField, Tooltip("SEの基準音量"), Range(0.1f, 1f)]
    private float baseSFXVolume = 0.7f;

    [SerializeField, Tooltip("BGMのデフォルト正規化音量"), Range(0.1f, 2f)]
    private float defaultBGMNormalizedVolume = 1.0f;

    [SerializeField, Tooltip("音量正規化を分析済みのクリップに適用")]
    private bool analyzeOnStart = true;

    // A dictionary that maps AudioID values to their corresponding AudioData,
    // allowing efficient lookup of sound effects by their ID
    private Dictionary<AudioID, AudioData> SFXLookup;

    private AudioClip currentBGM;
    private float originalBGMVolume;
    
    // BGM用の分析データ
    private Dictionary<AudioClip, float> bgmVolumeAnalysis = new Dictionary<AudioClip, float>();

    void Awake()
    {
        // Singleton pattern to ensure only one instance of AudioManager exists across scenes.
        if (Instance == null)
        {
            Instance = this; // If no instance exists, assign this instance as the singleton
            DontDestroyOnLoad(gameObject); // Prevent the instance from being destroyed when loading new scenes
        }
        else
        {
            Destroy(gameObject); // Destroy the duplicate instance
            return; // Exit the method to prevent further execution
        }

        originalBGMVolume = BGMPlayer.volume;
        SFXLookup = SFXList.ToDictionary(x => x.id);
        
        if (analyzeOnStart && enableVolumeBalancing)
        {
            AnalyzeAllAudioClips();
        }
    }

    /// <summary>
    /// 全てのBGMとSEクリップの音量を分析し、正規化係数を設定します
    /// </summary>
    private void AnalyzeAllAudioClips()
    {
        // SEクリップを分析
        foreach (var audioData in SFXList)
        {
            if (audioData.clip != null && !audioData.isAnalyzed)
            {
                audioData.normalizedVolume = AnalyzeAudioClip(audioData.clip);
                audioData.isAnalyzed = true;
            }
        }
        
        // 再生予定のBGMがあれば分析
        if (currentBGM != null && !bgmVolumeAnalysis.ContainsKey(currentBGM))
        {
            bgmVolumeAnalysis[currentBGM] = AnalyzeAudioClip(currentBGM);
        }
        
        Debug.Log($"全てのオーディオクリップの音量分析が完了しました。");
    }

    /// <summary>
    /// オーディオクリップの音量レベルを分析し、正規化係数を返します
    /// </summary>
    /// <param name="clip">分析するオーディオクリップ</param>
    /// <returns>音量正規化係数</returns>
    private float AnalyzeAudioClip(AudioClip clip)
    {
        if (clip == null) return 1.0f;
        
        // クリップからオーディオデータを取得
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        
        // RMSを計算して平均音量を取得
        float rms = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            rms += samples[i] * samples[i];
        }
        rms = Mathf.Sqrt(rms / samples.Length);
        
        // RMSが0に近い場合はデフォルト値を返す
        if (rms < 0.0001f) return 1.0f;
        
        // 正規化係数を計算（小さい音なら大きく、大きい音なら小さく）
        // 0.1がターゲットRMS値（適切な目標音量）
        float normalizedVolume = 0.1f / rms;
        
        // 極端な値にならないよう制限
        normalizedVolume = Mathf.Clamp(normalizedVolume, 0.1f, 3.0f);
        
        return normalizedVolume;
    }

    /// <summary>
    /// 指定したクリップのBGMを再生し、適切な音量バランスで調整します
    /// </summary>
    public void PlayBGM(AudioClip clip, bool loop = true, bool fade = true)
    {
        if (clip == null || clip == currentBGM)
            return;

        currentBGM = clip;
        
        // このBGMがまだ分析されていなければ分析
        if (enableVolumeBalancing && !bgmVolumeAnalysis.ContainsKey(clip))
        {
            bgmVolumeAnalysis[clip] = AnalyzeAudioClip(clip);
        }
        
        StartCoroutine(PlayBGMAsync(clip, loop, fade));
    }

    /// <summary>
    /// AudioClipを使用してSEを再生し、適切な音量バランスで調整します
    /// </summary>
    public void PlaySFX(AudioClip clip, bool pauseBGM = false)
    {
        if (clip == null)
            return;

        if (pauseBGM)
        {
            BGMPlayer.Pause();
            StartCoroutine(UnPauseBGM(clip.length));
        }

        // 未分析のクリップの場合、その場で分析
        float volumeScale = 1.0f;
        if (enableVolumeBalancing)
        {
            // SFXリストにあるかどうか確認
            AudioData foundData = SFXList.FirstOrDefault(x => x.clip == clip);
            if (foundData != null)
            {
                if (!foundData.isAnalyzed)
                {
                    foundData.normalizedVolume = AnalyzeAudioClip(clip);
                    foundData.isAnalyzed = true;
                }
                volumeScale = foundData.normalizedVolume;
            }
            else
            {
                // リストにないSEの場合はその場で分析
                volumeScale = AnalyzeAudioClip(clip);
            }
        }

        // 音量調整して再生
        SFXPlayer.PlayOneShot(clip, baseSFXVolume * volumeScale);
    }

    /// <summary>
    /// AudioIDを使用してSEを再生し、適切な音量バランスで調整します
    /// </summary>
    public void PlaySFX(AudioID audioID, bool pauseBGM = false)
    {
        if (!SFXLookup.ContainsKey(audioID))
            return;

        var audioData = SFXLookup[audioID];
        
        if (pauseBGM)
        {
            BGMPlayer.Pause();
            StartCoroutine(UnPauseBGM(audioData.clip.length));
        }
        
        // 未分析なら分析
        if (enableVolumeBalancing && !audioData.isAnalyzed)
        {
            audioData.normalizedVolume = AnalyzeAudioClip(audioData.clip);
            audioData.isAnalyzed = true;
        }
        
        // 音量調整して再生
        float volumeScale = enableVolumeBalancing ? audioData.normalizedVolume : 1.0f;
        SFXPlayer.PlayOneShot(audioData.clip, baseSFXVolume * volumeScale);
    }

    IEnumerator PlayBGMAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade)
            yield return BGMPlayer.DOFade(0, fadeDuration).WaitForCompletion();

        BGMPlayer.clip = clip;
        BGMPlayer.loop = loop;
        
        // 音量バランスに基づいて音量を設定
        float targetVolume = originalBGMVolume;
        if (enableVolumeBalancing && bgmVolumeAnalysis.ContainsKey(clip))
        {
            targetVolume = baseBGMVolume * bgmVolumeAnalysis[clip];
        }
        else
        {
            targetVolume = baseBGMVolume * defaultBGMNormalizedVolume;
        }
        
        BGMPlayer.Play();

        if (fade)
            yield return BGMPlayer.DOFade(targetVolume, fadeDuration).WaitForCompletion();
        else
            BGMPlayer.volume = targetVolume;
    }

    IEnumerator UnPauseBGM(float delay)
    {
        yield return new WaitForSeconds(delay);
        BGMPlayer.volume = 0;
        BGMPlayer.UnPause();
        
        // 現在のBGMに基づいて音量を設定
        float targetVolume = originalBGMVolume;
        if (enableVolumeBalancing && currentBGM != null && bgmVolumeAnalysis.ContainsKey(currentBGM))
        {
            targetVolume = baseBGMVolume * bgmVolumeAnalysis[currentBGM];
        }
        else
        {
            targetVolume = baseBGMVolume * defaultBGMNormalizedVolume;
        }
        
        BGMPlayer.DOFade(targetVolume, fadeDuration);
    }
    
    /// <summary>
    /// 音量バランス機能の有効/無効を切り替えます
    /// </summary>
    public void SetVolumeBalancingEnabled(bool enabled)
    {
        enableVolumeBalancing = enabled;
    }
    
    /// <summary>
    /// BGMの基準音量を設定します
    /// </summary>
    public void SetBaseBGMVolume(float volume)
    {
        baseBGMVolume = Mathf.Clamp(volume, 0.1f, 1f);
    }
    
    /// <summary>
    /// SEの基準音量を設定します
    /// </summary>
    public void SetBaseSFXVolume(float volume)
    {
        baseSFXVolume = Mathf.Clamp(volume, 0.1f, 1f);
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディタでのみ使用可能な音量分析機能
    /// </summary>
    [ContextMenu("全てのオーディオクリップを分析")]
    private void AnalyzeAllClipsInEditor()
    {
        AnalyzeAllAudioClips();
    }
#endif
}

public enum AudioID
{
    Money,
    Pop,
    Trash,
    Bin,
    Magical,
    Kaching
}

[System.Serializable]
public class AudioData
{
    [Tooltip("Unique ID for each audio clip")]
    public AudioID id;

    [Tooltip("The audio clip associated with the audio ID")]
    public AudioClip clip;
    
    [Tooltip("音量分析が完了しているか")]
    public bool isAnalyzed = false;
    
    [Tooltip("正規化された音量スケール（小さい音ほど大きい値）"), Range(0.1f, 3.0f)]
    public float normalizedVolume = 1.0f;
}
