using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugManager :MonoBehaviour
{
    // Inspector ���� UI GameObject ��ݒ�ł���悤�ɂ��܂��B
    [SerializeField, Tooltip("Escape �L�[�ŕ\������ UI GameObject")]
    private GameObject debugUI;

    [SerializeField]
    private TMP_Text elapsedTimeText;

    private string saveFileName;
    // �Q�[�����x��ύX����X�e�b�v�l�B�Ⴆ�� 0.5f ���ω�������
    float gameSpeedDegree = 0.5f;

    private void Update()
    {
        float elapsed = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : 0f;
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(elapsed);
        elapsedTimeText.text = $"{timeSpan:hh\\:mm\\:ss}";
    }

    public void ToggleDebugUI()
    {
        // debugUI �� null �łȂ��ꍇ�AactiveSelf �𔽓]������
        if (debugUI != null)
        {
            debugUI.SetActive(!debugUI.activeSelf);
        }
    }

    public void ClearData()
    {
        saveFileName = SceneManager.GetActiveScene().name;
        // �Z�[�u�f�[�^���폜
        SaveSystem.DeleteData(saveFileName);
        // ���݂̃V�[���������[�h
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SpeedUpGameSpeed()
    {
        // Time.timeScale �� gameSpeedDegree �������グ��
        Time.timeScale += gameSpeedDegree;
        Debug.Log("Game speed increased to: " + Time.timeScale);
    }

    public void SpeedDownGameSpeed()
    {
        // Time.timeScale �� gameSpeedDegree ������������B�Œ�l�� 0.5f �ɐݒ�B
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
