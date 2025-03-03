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
            // upgradeMeshes �� null �܂��͋�̏ꍇ�͏����𒆒f
            if (upgradeMeshes == null || upgradeMeshes.Length == 0)
            {
                Debug.LogWarning("Upgrade meshes are not assigned on " + gameObject.name);
                return;
            }

            // unlockLevel ���K�؂Ȕ͈͂��`�F�b�N�i�����ł� unlockLevel �� 2 �����̏ꍇ�̓A�b�v�O���[�h�ΏۊO�j
            if (unlockLevel < 2 || unlockLevel >= upgradeMeshes.Length + 2)
            {
                Debug.LogWarning("The unlock level is out of valid range on " + gameObject.name);
                return;
            }

            // meshFilter �� null �łȂ����m�F�i�ʏ�� RequireComponent �ŕۏ؂����j
            if (meshFilter == null)
            {
                Debug.LogWarning("MeshFilter component is missing on " + gameObject.name);
                return;
            }

            // �Ή����郁�b�V����ݒ�
            meshFilter.mesh = upgradeMeshes[unlockLevel - 2];
        }

    }
}
