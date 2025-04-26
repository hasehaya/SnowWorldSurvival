using UnityEngine;

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
        var upgradeHandlerList = FindObjectsOfType<UpgradeHandler>();
        foreach (var upgradeHandler in upgradeHandlerList)
        {
            upgradeHandler.SetMaterialType(MaterialType);
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
