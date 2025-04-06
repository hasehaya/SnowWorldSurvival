using UnityEngine;


public class Activator :Interactable
{
    [SerializeField, Tooltip("The GameObject to be activated/deactivated.")]
    private GameObject linkedObject;

    [HideInInspector] public MaterialType MaterialType;

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
