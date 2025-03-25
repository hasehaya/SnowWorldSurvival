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
        // シングルトンインスタンス
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

        // 各 MaterialType ごとの UnlockableBuyer のインスタンスを管理する Dictionary
        private Dictionary<MaterialType, UnlockableBuyer> activeBuyerInstances = new Dictionary<MaterialType, UnlockableBuyer>();

        [SerializeField, Tooltip("List of unlockable groups by MaterialType (None is excluded).")]
        private List<MaterialUnlockables> materialUnlockables;

        [SerializeField, Tooltip("Particle effect to play when unlocking something.")]
        private ParticleSystem unlockParticle;

        [SerializeField, Tooltip("Reference to the camera controller to focus on unlock points.")]
        private CameraController cameraController;

        // イベント：全体のアンロック進捗を通知
        public event Action<float> OnUnlock;

        // SaveData とレストランID の保持
        private SaveData data;
        private string restaurantID;

        // Awake でシングルトンを設定
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
        /// UnlockManager の初期化。GameManager から SaveData とレストランID を渡します。
        /// </summary>
        public void InitializeUnlockManager(SaveData saveData, string restaurantID)
        {
            this.data = saveData;
            this.restaurantID = restaurantID;

            // 各グループについて、既に解放済みなら購入済みの Unlockable を反映
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

            // すでに解放されている各グループに対して、UnlockableBuyer のインスタンスを生成しUI更新
            UpdateAllUnlockableBuyers();
            // Update camera focus to show all active unlockable points.
            UpdateCameraFocusForAll();
        }

        /// <summary>
        /// 指定された MaterialType の Unlockable を購入します。
        /// </summary>
        public void BuyUnlockable(MaterialType material)
        {
            // 対象グループを取得
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

            // 対象 Unlockable を購入
            var unlockable = group.unlockables[currentCount];
            unlockable.Unlock();

            // パーティクルエフェクトと効果音を再生
            unlockParticle.transform.position = unlockable.transform.position;
            unlockParticle.Play();
            AudioManager.Instance.PlaySFX(AudioID.Magical);

            // 進捗更新
            if (data.UnlockCounts.ContainsKey(material))
                data.UnlockCounts[material]++;
            else
                data.UnlockCounts[material] = 1;
            data.PaidAmounts[material] = 0;

            SaveSystem.SaveData<SaveData>(data, restaurantID);

            // 前のグループの進捗が50%以上の場合、次のグループを順次解放
            TryUnlockNextGroups();

            // 全グループの UnlockableBuyer UI を更新
            UpdateAllUnlockableBuyers();
            // Also update camera focus so that it now visits all active unlockable points.
            UpdateCameraFocusForAll();

            // オプション：購入時に対象 Unlockable の位置へカメラを移動（ここは省略または個別対応）
            // cameraController.FocusOnPointAndReturn(unlockable.GetBuyingPoint());
        }

        /// <summary>
        /// すべての解放済み MaterialGroup に対して、UnlockableBuyer の UI を更新または生成します。
        /// </summary>
        private void UpdateAllUnlockableBuyers()
        {
            foreach (var group in materialUnlockables)
            {
                // 解放されていなければスキップ
                if (!data.MaterialUnlocked.ContainsKey(group.material) || !data.MaterialUnlocked[group.material])
                    continue;

                int count = data.UnlockCounts.ContainsKey(group.material) ? data.UnlockCounts[group.material] : 0;

                // グループが全て購入済みの場合、既存の UI があれば非表示にする
                if (count >= group.unlockables.Count)
                {
                    if (activeBuyerInstances.ContainsKey(group.material))
                    {
                        activeBuyerInstances[group.material].gameObject.SetActive(false);
                    }
                    continue;
                }

                // 未生成ならPrefabから生成
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

                // 対象グループの次に購入可能な Unlockable の情報を取得
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
            // 全体の進捗を通知
            OnUnlock?.Invoke(ComputeOverallProgress());
        }

        /// <summary>
        /// 前の MaterialGroup の進捗が50%以上の場合、順次次の MaterialGroup を解放します。
        /// </summary>
        private void TryUnlockNextGroups()
        {
            // MaterialType の順序：Log, Rock, Snow, Tomato
            MaterialType[] order = new MaterialType[] { MaterialType.Log, MaterialType.Rock, MaterialType.Snow, MaterialType.Tomato };

            // 2番目以降のグループについて、前のグループの進捗をチェック
            for (int i = 1; i < order.Length; i++)
            {
                MaterialType prev = order[i - 1];
                MaterialType curr = order[i];
                if (data.MaterialUnlocked.ContainsKey(curr) && data.MaterialUnlocked[curr])
                    continue; // すでに解放済みならスキップ

                var prevGroup = materialUnlockables.FirstOrDefault(g => g.material == prev);
                if (prevGroup == null || prevGroup.unlockables.Count == 0)
                    continue;
                int prevCount = data.UnlockCounts.ContainsKey(prev) ? data.UnlockCounts[prev] : 0;
                float progress = prevCount / (float)prevGroup.unlockables.Count;
                if (progress >= 0.5f)
                {
                    data.MaterialUnlocked[curr] = true;
                    // オプション：新しく解放されたグループの最初の Unlockable を自動購入（進捗 1 にする）
                    data.UnlockCounts[curr] = 1;
                    data.PaidAmounts[curr] = 0;
                }
            }
        }

        /// <summary>
        /// 指定された MaterialType の Unlockable に対する支払い済み金額を更新します。
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
        /// すべての解放済みグループに対する全体の進捗（各グループの進捗の平均）を計算します。
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
