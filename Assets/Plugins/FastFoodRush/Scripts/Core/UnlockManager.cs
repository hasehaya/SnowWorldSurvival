using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    [Serializable]
    public class MaterialUnlockables
    {
        public MaterialType material;
        public List<Unlockable> unlockables;

        [Header("Pricing Settings")]
        [Tooltip("The base price for unlocking items in this group.")]
        public int baseUnlockPrice = 75;

        [Tooltip("The growth factor applied to unlock prices in this group.")]
        [Range(1.01f, 1.99f)]
        public float unlockGrowthFactor = 1.1f;
    }

    public class UnlockManager :MonoBehaviour
    {
        // �V���O���g���C���X�^���X
        private static UnlockManager instance;
        public static UnlockManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UnlockManager>();
                }
                return instance;
            }
        }

        [Header("Unlockable Settings")]
        [SerializeField, Tooltip("Prefab for the unlockable buyer UI element.")]
        private UnlockableBuyer unlockableBuyerPrefab;

        // �e MaterialType ���Ƃ� UnlockableBuyer �̃C���X�^���X���Ǘ����� Dictionary
        private Dictionary<MaterialType, UnlockableBuyer> activeBuyerInstances = new Dictionary<MaterialType, UnlockableBuyer>();

        [SerializeField, Tooltip("List of unlockable groups by MaterialType (None is excluded).")]
        private List<MaterialUnlockables> materialUnlockables;

        [SerializeField, Tooltip("Particle effect to play when unlocking something.")]
        private ParticleSystem unlockParticle;

        [SerializeField, Tooltip("Reference to the camera controller to focus on unlock points.")]
        private CameraController cameraController;

        // �C�x���g�F�S�̂̃A�����b�N�i����ʒm
        public event Action<float> OnUnlock;

        // SaveData �ƃ��X�g����ID �̕ێ�
        private SaveData data;
        private string restaurantID;

        // Awake �ŃV���O���g����ݒ�
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            // DontDestroyOnLoad(this.gameObject); // Uncomment if needed.
        }

        /// <summary>
        /// UnlockManager �̏������BGameManager ���� SaveData �ƃ��X�g����ID ��n���܂��B
        /// </summary>
        public void InitializeUnlockManager(SaveData saveData, string restaurantID)
        {
            this.data = saveData;
            this.restaurantID = restaurantID;

            // �e�O���[�v�ɂ��āA���ɉ���ς݂Ȃ�w���ς݂� Unlockable �𔽉f
            foreach (var group in materialUnlockables)
            {
                if (!data.MaterialUnlocked.ContainsKey(group.material))
                    continue;

                if (data.MaterialUnlocked[group.material])
                {
                    int count = data.UnlockCounts.ContainsKey(group.material) ? data.UnlockCounts[group.material] : 0;
                    for (int i = 0; i < count && i < group.unlockables.Count; i++)
                    {
                        group.unlockables[i].Unlock(false);
                    }
                }
            }

            // ���łɉ������Ă���e�O���[�v�ɑ΂��āAUnlockableBuyer �̃C���X�^���X�𐶐���UI�X�V
            UpdateAllUnlockableBuyers();
            // Update camera focus to show all active unlockable points.
            UpdateCameraFocusForAll();
        }

        /// <summary>
        /// �w�肳�ꂽ MaterialType �� Unlockable ���w�����܂��B
        /// </summary>
        public void BuyUnlockable(MaterialType material)
        {
            // �ΏۃO���[�v���擾
            var group = materialUnlockables.FirstOrDefault(g => g.material == material);
            if (group == null)
            {
                Debug.LogWarning("Group for material " + material + " not found.");
                return;
            }
            if (!data.MaterialUnlocked.ContainsKey(material) || !data.MaterialUnlocked[material])
            {
                Debug.LogWarning("Group for material " + material + " is not unlocked yet.");
                return;
            }
            int currentCount = data.UnlockCounts.ContainsKey(material) ? data.UnlockCounts[material] : 0;
            if (currentCount >= group.unlockables.Count)
            {
                Debug.LogWarning("All unlockables for material " + material + " have been purchased.");
                return;
            }

            if (currentCount % 3 == 2)
            {
                AdMobRewardInterstitial.Instance.ShowAdMobReward();
            }

            // �Ώ� Unlockable ���w��
            var unlockable = group.unlockables[currentCount];
            unlockable.Unlock();

            // �p�[�e�B�N���G�t�F�N�g�ƌ��ʉ����Đ�
            unlockParticle.transform.position = unlockable.transform.position;
            unlockParticle.Play();
            AudioManager.Instance.PlaySFX(AudioID.Magical);

            // �i���X�V
            if (data.UnlockCounts.ContainsKey(material))
                data.UnlockCounts[material]++;
            else
                data.UnlockCounts[material] = 1;
            data.PaidAmounts[material] = 0;

            SaveSystem.SaveData<SaveData>(data, restaurantID);

            // �O�̃O���[�v�̐i����50%�ȏ�̏ꍇ�A���̃O���[�v���������
            TryUnlockNextGroups();

            // �S�O���[�v�� UnlockableBuyer UI ���X�V
            UpdateAllUnlockableBuyers();
            // Also update camera focus so that it now visits all active unlockable points.
            UpdateCameraFocusForAll();

            // �I�v�V�����F�w�����ɑΏ� Unlockable �̈ʒu�փJ�������ړ��i�����͏ȗ��܂��͌ʑΉ��j
            // cameraController.FocusOnPointAndReturn(unlockable.GetBuyingPoint());
        }

        /// <summary>
        /// ���ׂẲ���ς� MaterialGroup �ɑ΂��āAUnlockableBuyer �� UI ���X�V�܂��͐������܂��B
        /// </summary>
        private void UpdateAllUnlockableBuyers()
        {
            foreach (var group in materialUnlockables)
            {
                // �������Ă��Ȃ���΃X�L�b�v
                if (!data.MaterialUnlocked.ContainsKey(group.material) || !data.MaterialUnlocked[group.material])
                    continue;

                int count = data.UnlockCounts.ContainsKey(group.material) ? data.UnlockCounts[group.material] : 0;

                // �O���[�v���S�čw���ς݂̏ꍇ�A������ UI ������Δ�\���ɂ���
                if (count >= group.unlockables.Count)
                {
                    if (activeBuyerInstances.ContainsKey(group.material))
                    {
                        activeBuyerInstances[group.material].gameObject.SetActive(false);
                    }
                    continue;
                }

                // �������Ȃ�Prefab���琶��
                if (!activeBuyerInstances.ContainsKey(group.material) || activeBuyerInstances[group.material] == null)
                {
                    if (unlockableBuyerPrefab != null)
                    {
                        var buyerInstance = Instantiate(unlockableBuyerPrefab);
                        activeBuyerInstances[group.material] = buyerInstance;
                    }
                    else
                    {
                        Debug.LogWarning("UnlockableBuyer prefab not assigned.");
                        continue;
                    }
                }

                // �ΏۃO���[�v�̎��ɍw���\�� Unlockable �̏����擾
                var buyerUI = activeBuyerInstances[group.material];
                var nextUnlockable = group.unlockables[count];
                // Calculate price using the group's own baseUnlockPrice and unlockGrowthFactor.
                int price = Mathf.RoundToInt(Mathf.Round(group.baseUnlockPrice * Mathf.Pow(group.unlockGrowthFactor, count)) / 5f) * 5;
                int paid = data.PaidAmounts.ContainsKey(group.material) ? data.PaidAmounts[group.material] : 0;

                // Update the buyer UI position and initialize it with the associated MaterialType.
                buyerUI.transform.position = nextUnlockable.GetBuyingPoint();
                buyerUI.Initialize(nextUnlockable, price, paid, group.material);
                buyerUI.gameObject.SetActive(true);
            }
            // �S�̂̐i����ʒm
            OnUnlock?.Invoke(ComputeOverallProgress());
        }

        /// <summary>
        /// �O�� MaterialGroup �̐i����50%�ȏ�̏ꍇ�A�������� MaterialGroup ��������܂��B
        /// </summary>
        private void TryUnlockNextGroups()
        {
            // MaterialType �̏����FLog, Rock, Snow, Tomato
            MaterialType[] order = new MaterialType[] { MaterialType.Log, MaterialType.Rock, MaterialType.Snow, MaterialType.Tomato };

            // 2�Ԗڈȍ~�̃O���[�v�ɂ��āA�O�̃O���[�v�̐i�����`�F�b�N
            for (int i = 1; i < order.Length; i++)
            {
                MaterialType prev = order[i - 1];
                MaterialType curr = order[i];
                if (data.MaterialUnlocked.ContainsKey(curr) && data.MaterialUnlocked[curr])
                    continue; // ���łɉ���ς݂Ȃ�X�L�b�v

                var prevGroup = materialUnlockables.FirstOrDefault(g => g.material == prev);
                if (prevGroup == null || prevGroup.unlockables.Count == 0)
                    continue;
                int prevCount = data.UnlockCounts.ContainsKey(prev) ? data.UnlockCounts[prev] : 0;
                float progress = prevCount / (float)prevGroup.unlockables.Count;
                if (progress >= 0.5f)
                {
                    data.MaterialUnlocked[curr] = true;
                    // �I�v�V�����F�V����������ꂽ�O���[�v�̍ŏ��� Unlockable �������w���i�i�� 1 �ɂ���j
                    data.UnlockCounts[curr] = 1;
                    data.PaidAmounts[curr] = 0;
                }
            }
        }

        /// <summary>
        /// �w�肳�ꂽ MaterialType �� Unlockable �ɑ΂���x�����ς݋��z���X�V���܂��B
        /// </summary>
        public void UpdatePaidAmount(MaterialType material, int amount)
        {
            if (data.PaidAmounts.ContainsKey(material))
                data.PaidAmounts[material] = amount;
            else
                data.PaidAmounts[material] = amount;
            UpdateAllUnlockableBuyers();
            UpdateCameraFocusForAll();
        }

        /// <summary>
        /// ���ׂẲ���ς݃O���[�v�ɑ΂���S�̂̐i���i�e�O���[�v�̐i���̕��ρj���v�Z���܂��B
        /// </summary>
        private float ComputeOverallProgress()
        {
            float totalProgress = 0f;
            int groupCount = 0;
            foreach (var group in materialUnlockables)
            {
                if (!data.MaterialUnlocked.ContainsKey(group.material) || !data.MaterialUnlocked[group.material])
                    continue;
                groupCount++;
                int count = data.UnlockCounts.ContainsKey(group.material) ? data.UnlockCounts[group.material] : 0;
                totalProgress += (group.unlockables.Count > 0 ? (count / (float)group.unlockables.Count) : 0);
            }
            if (groupCount == 0)
                return 0f;
            return totalProgress / groupCount;
        }

        /// <summary>
        /// Gather all active unlockable buying points from unlocked groups and pass them to the CameraController.
        /// </summary>
        private void UpdateCameraFocusForAll()
        {
            List<Vector3> focusPoints = new List<Vector3>();
            foreach (var group in materialUnlockables)
            {
                if (!data.MaterialUnlocked.ContainsKey(group.material) || !data.MaterialUnlocked[group.material])
                    continue;
                int count = data.UnlockCounts.ContainsKey(group.material) ? data.UnlockCounts[group.material] : 0;
                // Only add if the group still has pending unlockables.
                if (count < group.unlockables.Count)
                {
                    focusPoints.Add(group.unlockables[count].GetBuyingPoint());
                }
            }

            if (focusPoints.Count > 0)
            {
                cameraController.FocusOnPointsAndReturn(focusPoints);
            }
        }
    }
}
