using CryingSnow.FastFoodRush;

using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugManager :MonoBehaviour
{
    // Inspector ���� UI GameObject ��ݒ�ł���悤�ɂ��܂��B
    [SerializeField, Tooltip("Escape �L�[�ŕ\������ UI GameObject")]
    private GameObject debugUI;

    private string saveFileName;
    // �Q�[�����x��ύX����X�e�b�v�l�B�Ⴆ�� 0.5f ���ω�������
    float gameSpeedDegree = 0.5f;

    void Start()
    {
        // �K�v�ɉ����ď�����ԂŔ�\���ɂ���ꍇ�͈ȉ��̂悤�ɐݒ�
        //if (debugUI != null)
        //    debugUI.SetActive(false);
    }

    void Update()
    {
        // Escape �L�[�������ꂽ�� debugUI ���A�N�e�B�u�ɂ���
        //if (SimpleInput.GetKeyDown(KeyCode.Escape))
        //{
        //    if (debugUI != null)
        //        debugUI.SetActive(true);
        //}
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
}
