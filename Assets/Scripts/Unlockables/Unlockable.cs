﻿using System.Collections.Generic;
using System.Linq;

using DG.Tweening;

using UnityEngine;


public class Unlockable :MonoBehaviour
{
    [SerializeField, Tooltip("The point at which the object can be bought or unlocked (relative to the object position)")]
    private Vector3 buyingPoint = Vector3.zero;

    [SerializeField, Tooltip("The scale effect applied when the object is unlocked")]
    private Vector3 punchScale = new Vector3(0.1f, 0.2f, 0.1f);

    [SerializeField]
    private Sprite contentIcon;

    public Sprite ContentIcon => contentIcon;  // The icon representing the unlockable object

    protected int unlockLevel;  // The current level of the unlock

    private List<UpgradeableMesh> upgradeableMeshes = new List<UpgradeableMesh>();  // List of upgradeable meshes to apply upgrades to

    protected virtual void Awake()
    {
        upgradeableMeshes = GetComponentsInChildren<UpgradeableMesh>(true)
            .Where(x => x != null)  // null を除外
            .ToList();
        gameObject.SetActive(false);  // Set the object to inactive initially
    }

    /// <summary>
    /// Unlocks the object, applying upgrades and visual effects based on the unlock level.
    /// </summary>
    /// <param name="animate">Determines if the unlocking animation should be played</param>
    public virtual void Unlock(bool animate = true)
    {
        unlockLevel++;  // Increment the unlock level

        // Apply upgrade to meshes if the unlock level is greater than 1
        if (unlockLevel > 1)
        {
            if (upgradeableMeshes.Count > 0)
            {
                upgradeableMeshes.ForEach(x => x.ApplyUpgrade(unlockLevel));
            }
        }
        else
        {
            gameObject.SetActive(true);
        }

        UpdateStats();  // Update the stats of the object based on the unlock level

        if (!animate)
            return;  // Skip animation if not required

        // Animate the upgradeable meshes if animation is enabled
        foreach (var upgradeableMesh in upgradeableMeshes)
        {
            upgradeableMesh.transform.DOPunchScale(punchScale, 0.3f);
        }
    }

    /// <summary>
    /// This method can be overridden to update specific stats related to the unlockable object.
    /// </summary>
    protected virtual void UpdateStats() { }

    /// <summary>
    /// Gets the world position of the buying point.
    /// </summary>
    /// <returns>The world position of the buying point</returns>
    public Vector3 GetBuyingPoint() => transform.TransformPoint(buyingPoint);

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        var center = GetBuyingPoint(); // Get the world position of the buying point
        var size = new Vector3(2f, 0.2f, 2f);
        Gizmos.DrawWireCube(center, size);

        // Label the buying point in the editor
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        UnityEditor.Handles.Label(center + Vector3.up * 0.5f, "Buying\nPoint", style);
    }
#endif
}
