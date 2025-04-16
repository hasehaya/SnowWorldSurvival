using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectCellView :MonoBehaviour
{
    [SerializeField] private int stageIndex;
    [SerializeField] private Button btn;
    [SerializeField] private ProgressDisplayInTitle progressDisplay;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private TMP_Text stageNumText;

    private StageData stageData;
    private StageData previousStageData;

    public void Start()
    {
        stageNumText.text = stageIndex.ToString();
        string stageID = $"Stage{stageIndex}";
        stageData = SaveSystem.LoadData<StageData>(stageID);

        // Load previous stage data
        if (stageIndex > 1)
        {
            string previousStageID = $"Stage{stageIndex - 1}";
            previousStageData = SaveSystem.LoadData<StageData>(previousStageID);
        }

        // ステージインデックスをProgressDisplayに設定
        if (progressDisplay != null)
        {
            progressDisplay.SetStageIndex(stageIndex);
            if (stageData != null)
            {
                progressDisplay.UpdateProgress(stageData.CalculateProgress());
            }
        }

        // 前のステージの進捗率が80%未満の場合はボタンを無効化し、テキストをグレーに
        if (stageIndex > 1 && (previousStageData == null || previousStageData.CalculateProgress() < 0.8f))
        {
            btn.interactable = false;
            if (effectText != null)
            {
                effectText.color = new Color(0.5f, 0.5f, 0.5f, 1f); // グレー色
            }
        }
        else
        {
            btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Stage" + stageIndex);
            });
        }
    }
}
