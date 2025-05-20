using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Activator : Interactable
{
    private GameObject linkedObject;
    [HideInInspector] public MaterialType MaterialType;

    private void Start()
    {
        var employeeUpgrades = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in employeeUpgrades)
        {
            if (obj.CompareTag("EmployeeUpgrade"))
            {
                linkedObject = obj;
                break;
            }
        }

        if (linkedObject == null)
        {
            Debug.LogError("EmployeeUpgradeが見つかりませんでした。シーン内にEmployeeUpgradeタグを持つオブジェクトが存在することを確認してください。");
        }
    }

    /// <summary>
    /// Called when the player enters the trigger area. It activates the linked object.
    /// </summary>
    protected override void OnPlayerEnter()
    {
        linkedObject.SetActive(true);
        AutoUpgradeEmployeeAmount();

        var upgradeHandlerList = FindObjectsOfType<UpgradeHandler>();
        foreach (var upgradeHandler in upgradeHandlerList)
        {
            upgradeHandler.SetMaterialType(MaterialType);
        }
    }

    private void AutoUpgradeEmployeeAmount()
    {
        // ステージ1かつWood_1の場合、EmployeeAmountアップグレードが0なら1にする
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentStageNumber = int.Parse(new string(currentSceneName.Where(char.IsDigit).ToArray()));
        
        if (currentStageNumber == 1 && MaterialType == MaterialType.Wood_1)
        {
            StageData stageData = SaveSystem.LoadData<StageData>(currentSceneName);
            if (stageData != null)
            {
                Upgrade employeeAmountUpgrade = stageData.FindUpgrade(Upgrade.UpgradeType.EmployeeAmount, MaterialType.Wood_1);
                if (employeeAmountUpgrade != null && employeeAmountUpgrade.Level == 0)
                {
                    // GameManagerの新しいメソッドを使用して自動アップグレード
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AutoUpgradeEmployeeAmount(MaterialType.Wood_1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when the player exits the trigger area. It deactivates the linked object.
    /// </summary>
    protected override void OnPlayerExit()
    {
        linkedObject.SetActive(false); // Deactivates the linked object when the player exits the trigger area.
    }
}
