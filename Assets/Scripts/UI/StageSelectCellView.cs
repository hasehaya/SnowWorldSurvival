using UnityEngine;
using UnityEngine.UI;

public class StageSelectCellView :MonoBehaviour
{
    [SerializeField] private int stageIndex;
    [SerializeField] private Button btn;

    public void SetUp(int stageIndex)
    {
        this.stageIndex = stageIndex;
        btn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Stage" + stageIndex);
        });
    }
}
