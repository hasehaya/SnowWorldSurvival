using DG.Tweening;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class MainMenuManager :MonoBehaviour
{
    [SerializeField, Tooltip("The button to start the game")]
    private Button startButton;

    [SerializeField, Tooltip("The button to quit the game")]
    private GameObject startText;

    [SerializeField, Tooltip("Screen fader for transitioning between scenes")]
    private ScreenFader screenFader;

    [SerializeField, Tooltip("Background music for the main menu")]
    private AudioClip backgroundMusic;

    void Awake()
    {
        // 言語設定の初期化
        StartCoroutine(InitializeLocalizationSettings());
    }

    void Start()
    {
        // Set the frame rate and disable vertical sync
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Add listener to start button
        startButton.onClick.AddListener(StartGame);

        // Animate the start button with a scaling effect
        startText.transform.DOScale(Vector3.one * 1.1f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo);

        // Play background music
        AudioManager.Instance.PlayBGM(backgroundMusic);
    }

    private System.Collections.IEnumerator InitializeLocalizationSettings()
    {
        // LocalizationSettingsの初期化を待つ
        yield return LocalizationSettings.InitializationOperation;
        
        // 保存されていた言語設定を適用
        string savedLocaleCode = LocaleSettings.GetSavedLocaleCode();
        if (!string.IsNullOrEmpty(savedLocaleCode))
        {
            var savedLocale = LocalizationSettings.AvailableLocales.GetLocale(savedLocaleCode);
            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale = savedLocale;
                Debug.Log($"保存されていた言語設定 {savedLocaleCode} を適用しました。");
            }
        }
    }

    void StartGame()
    {
        // Stop the button scale animation
        DOTween.Kill(startText.transform);

        // グローバルデータを読み込み
        GlobalData globalData = SaveSystem.LoadData<GlobalData>("GlobalData");
        
        // 解放されている最新のステージIDを取得
        string targetScene = "StageSelect"; // デフォルト値
        
        if (globalData != null && globalData.StageId > 0)
        {
            // 最新のアンロックされたステージに直接遷移
            targetScene = "Stage" + globalData.StageId;
        }

        // Fade the screen and load the target scene
        screenFader.FadeIn(() => SceneManager.LoadScene(targetScene));

        // Play magical sound effect when starting the game
        AudioManager.Instance.PlaySFX(AudioID.Magical);
    }
}
