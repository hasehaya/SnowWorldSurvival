using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    [RequireComponent(typeof(MeshFilter))]
    public class UpgradeableMesh :MonoBehaviour
    {
        [SerializeField, Tooltip("Array of meshes to represent the upgraded versions of this object.")]
        Mesh[] upgradeMeshes;

        private MeshFilter meshFilter;  // The MeshFilter component to apply upgrades to

        void Awake()
        {
            // Get the MeshFilter component attached to the object
            meshFilter = GetComponent<MeshFilter>();
        }

        /// <summary>
        /// Applies the appropriate mesh based on the provided unlock level.
        /// </summary>
        /// <param name="unlockLevel">The level at which the upgrade is applied.</param>
        public void ApplyUpgrade(int unlockLevel)
        {
            // upgradeMeshes が null または空の場合は処理を中断
            if (upgradeMeshes == null || upgradeMeshes.Length == 0)
            {
                Debug.LogWarning("Upgrade meshes are not assigned on " + gameObject.name);
                return;
            }

            // unlockLevel が適切な範囲かチェック（ここでは unlockLevel が 2 未満の場合はアップグレード対象外）
            if (unlockLevel < 2 || unlockLevel >= upgradeMeshes.Length + 2)
            {
                Debug.LogWarning("The unlock level is out of valid range on " + gameObject.name);
                return;
            }

            // meshFilter が null でないか確認（通常は RequireComponent で保証される）
            if (meshFilter == null)
            {
                Debug.LogWarning("MeshFilter component is missing on " + gameObject.name);
                return;
            }

            // 対応するメッシュを設定
            meshFilter.mesh = upgradeMeshes[unlockLevel - 2];
        }

    }
}
