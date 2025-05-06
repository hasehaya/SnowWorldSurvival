using DG.Tweening;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
