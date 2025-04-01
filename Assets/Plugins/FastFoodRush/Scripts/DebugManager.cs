using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugManager :MonoBehaviour
{
    // Inspector から UI GameObject を設定できるようにします。
    [SerializeField, Tooltip("Escape キーで表示する UI GameObject")]
    private GameObject debugUI;

    [SerializeField]
    private TMP_Text elapsedTimeText;

    private string saveFileName;
    // ゲーム速度を変更するステップ値。例えば 0.5f ずつ変化させる
    float gameSpeedDegree = 0.5f;

    private void Update()
    {
        float elapsed = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : 0f;
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(elapsed);
        elapsedTimeText.text = $"{timeSpan:hh\\:mm\\:ss}";
    }

    public void ToggleDebugUI()
    {
        // debugUI が null でない場合、activeSelf を反転させる
        if (debugUI != null)
        {
            debugUI.SetActive(!debugUI.activeSelf);
        }
    }

    public void ClearData()
    {
        saveFileName = SceneManager.GetActiveScene().name;
        // セーブデータを削除
        SaveSystem.DeleteData(saveFileName);
        // 現在のシーンをリロード
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SpeedUpGameSpeed()
    {
        // Time.timeScale を gameSpeedDegree 分だけ上げる
        Time.timeScale += gameSpeedDegree;
        Debug.Log("Game speed increased to: " + Time.timeScale);
    }

    public void SpeedDownGameSpeed()
    {
        // Time.timeScale を gameSpeedDegree 分だけ下げる。最低値は 0.5f に設定。
        Time.timeScale = Mathf.Max(0.5f, Time.timeScale - gameSpeedDegree);
        Debug.Log("Game speed decreased to: " + Time.timeScale);
    }

    public void AddMoney()
    {
        GameManager.Instance.AdjustMoney(1000);
    }

    public void AddMoney20000()
    {
        GameManager.Instance.AdjustMoney(20000);
    }

    public void ToggleAd()
    {

    }
}
