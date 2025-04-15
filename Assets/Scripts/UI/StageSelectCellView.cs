using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectCellView : MonoBehaviour
{
    [SerializeField] private int stageIndex;
    [SerializeField] private Button btn;
    [SerializeField] private ProgressDisplayInTitle progressDisplay;

    public void Start()
    {
        // ステージインデックスをProgressDisplayに設定
        if (progressDisplay != null)
        {
            progressDisplay.SetStageIndex(stageIndex);
        }

        btn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Stage" + stageIndex);
        });
    }
}
