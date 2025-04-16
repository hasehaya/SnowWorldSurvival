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

    public void UpdateProgress(float progress)
    {
        // Update fill color based on gradient evaluation
        progressFill.color = progressGradient.Evaluate(progress);

        // Update fill amount of the progress bar
        progressFill.fillAmount = progress;

        // Update progress text with percentage format
        progressText.text = $"PROGRESS {progress * 100:0.##}%";
    }

    private void LoadStageData()
    {
        string stageID = stageIndex >= 0 ? $"Stage{stageIndex}" : UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        stageData = SaveSystem.LoadData<StageData>(stageID);
        
        if (stageData != null)
        {
            UpdateProgress(stageData.CalculateProgress());
        }
    }
}
