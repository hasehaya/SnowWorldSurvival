using TMPro;

using UnityEngine;


public class ProgressDisplay :MonoBehaviour
{
    [SerializeField] private Gradient progressGradient;

    [SerializeField] private SlicedFilledImage progressFill;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject nextStageText;

    void Start()
    {
        nextStageText.SetActive(false);
        // Subscribes to the RestaurantManager's OnUnlock event when enabled.
        UnlockManager.Instance.OnUnlock += UpdateProgress;
    }

    void OnDisable()
    {
        // Unsubscribes from the RestaurantManager's OnUnlock event when disabled.
        UnlockManager.Instance.OnUnlock -= UpdateProgress;
    }

    /// <summary>
    /// Updates the visual representation of the progress bar and text.
    /// </summary>
    /// <param name="progress">A float value representing the current progress (0 to 1).</param>
    void UpdateProgress(float progress)
    {
        // Update fill color based on gradient evaluation
        progressFill.color = progressGradient.Evaluate(progress);

        // Update fill amount of the progress bar
        progressFill.fillAmount = progress;

        // Update progress text with percentage format
        progressText.text = $"PROGRESS {progress * 100:0.##}%";
    }

    public void ShowNextStageText()
    {
        nextStageText.SetActive(true);
    }
}
