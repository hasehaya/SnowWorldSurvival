using TMPro;
using UnityEngine;

public class ProgressDisplayInTitle : MonoBehaviour
{
    [SerializeField, Tooltip("Gradient used to visually represent progress from start to completion.")]
    private Gradient progressGradient;

    [SerializeField, Tooltip("Stage index to display progress for. If not set, will use current stage.")]
    private int stageIndex = -1;

    private SlicedFilledImage progressFill;
    private TMP_Text progressText;
    private StageData stageData;

    void Awake()
    {
        // Initializes references to child components for progress display.
        progressFill = GetComponentInChildren<SlicedFilledImage>();
        progressText = GetComponentInChildren<TMP_Text>();

        // Load stage data based on stage index
        LoadStageData();
    }

    public void SetStageIndex(int index)
    {
        stageIndex = index;
        LoadStageData();
    }

    private void LoadStageData()
    {
        string stageID = stageIndex >= 0 ? $"Stage{stageIndex}" : UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        stageData = SaveSystem.LoadData<StageData>(stageID);
        
        if (stageData != null)
        {
            UpdateProgress(CalculateProgress());
        }
    }

    private float CalculateProgress()
    {
        if (stageData == null) return 0f;

        int totalUnlocks = 0;
        int unlockedCount = 0;

        // Count total unlocks and unlocked items
        foreach (var materialType in stageData.MaterialUnlocked.Keys)
        {
            if (materialType == MaterialType.None) continue;

            // Count material group unlocks
            totalUnlocks++;
            if (stageData.MaterialUnlocked[materialType])
            {
                unlockedCount++;
            }

            // Count individual unlocks
            if (stageData.UnlockCounts.ContainsKey(materialType))
            {
                totalUnlocks += stageData.UnlockCounts[materialType];
                unlockedCount += stageData.UnlockCounts[materialType];
            }
        }

        return totalUnlocks > 0 ? (float)unlockedCount / totalUnlocks : 0f;
    }

    void UpdateProgress(float progress)
    {
        // Update fill color based on gradient evaluation
        progressFill.color = progressGradient.Evaluate(progress);

        // Update fill amount of the progress bar
        progressFill.fillAmount = progress;

        // Update progress text with percentage format
        progressText.text = $"PROGRESS {progress * 100:0.##}%";
    }
}
