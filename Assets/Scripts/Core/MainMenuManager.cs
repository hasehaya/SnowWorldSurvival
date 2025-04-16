using DG.Tweening;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuManager :MonoBehaviour
{
    [SerializeField, Tooltip("The button to start the game")]
    private Button startButton;

    [SerializeField, Tooltip("Screen fader for transitioning between scenes")]
    private ScreenFader screenFader;

    [SerializeField, Tooltip("Background music for the main menu")]
    private AudioClip backgroundMusic;

    private int targetScene; // Scene index to load when starting the game

    void Start()
    {
        // Set the frame rate and disable vertical sync
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Load the last scene index from saved data, default to scene index 1 if not found
        int lastSceneIndex = SaveSystem.LoadData<int>("LastSceneIndex");
        targetScene = lastSceneIndex == 0 ? 1 : lastSceneIndex;

        // Add listener to start button
        startButton.onClick.AddListener(StartGame);

        // Animate the start button with a scaling effect
        startButton.transform.DOScale(Vector3.one * 1.1f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo);

        // Play background music
        AudioManager.Instance.PlayBGM(backgroundMusic);
    }

    void StartGame()
    {
        // Stop the button scale animation
        DOTween.Kill(startButton.transform);

        // Fade the screen and load the target scene
        screenFader.FadeIn(() => SceneManager.LoadScene(targetScene));

        // Play magical sound effect when starting the game
        AudioManager.Instance.PlaySFX(AudioID.Magical);
    }
}
