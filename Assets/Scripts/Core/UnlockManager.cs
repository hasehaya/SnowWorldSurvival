using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

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
    private List<MaterialObjectParent> materialUnlockables;

    [SerializeField, Tooltip("Particle effect to play when unlocking something.")]
    private ParticleSystem unlockParticle;

    [SerializeField, Tooltip("Reference to the camera controller to focus on unlock points.")]
    private CameraController cameraController;

    [SerializeField, Tooltip("Reference to the progress display to show next stage text.")]
    private ProgressDisplay progressDisplay;

    // イベント：全体のアンロック進捗を通知
    public event Action<float> OnUnlock;

    // SaveData とレストランID の保持
    private StageData data;
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
        
        // ProgressDisplayの参照を取得
        if (progressDisplay == null)
        {
            progressDisplay = FindObjectOfType<ProgressDisplay>();
        }
    }

    /// <summary>
    /// UnlockManager の初期化。GameManager から SaveData とレストランID を渡します。
    /// </summary>
    public void InitializeUnlockManager(StageData saveData, string restaurantID)
    {
        this.data = saveData;
        this.restaurantID = restaurantID;

        // 各グループについて、既に解放済みなら購入済みの Unlockable を反映
        foreach (var group in materialUnlockables)
        {
            if (!data.MaterialUnlocked.ContainsKey(group.materialType))
                continue;

            if (data.MaterialUnlocked[group.materialType])
            {
                int count = data.UnlockCounts.ContainsKey(group.materialType) ? data.UnlockCounts[group.materialType] : 0;
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
        
        // NextStagePromptフラグがONの場合は、NextStageTextを表示
        if (data.NextStagePrompt && progressDisplay != null)
        {
            progressDisplay.ShowNextStageText();
        }
    }

    /// <summary>
    /// 指定された MaterialType の Unlockable を購入します。
    /// </summary>
    public void BuyUnlockable(MaterialType material)
    {
        // 対象グループを取得
        var group = materialUnlockables.FirstOrDefault(g => g.materialType == material);
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

        // 全体のUnlockCountを計算
        int overallUnlockCount = data.UnlockCounts.Values.Sum();
        // 全体のUnlockCountが3で割った余りが2の場合、広告を表示する
        // ステージ1の場合、6で割った余りが5の場合に広告を表示
        int currentStageNumber = int.Parse(new string(restaurantID.Where(char.IsDigit).ToArray()));
        if (currentStageNumber == 1)
        {
            if(overallUnlockCount % 7 == 6)
            {
                AdMobInterstitial.Instance.ShowAdMobInterstitial();
            }
        }
        else if (overallUnlockCount % 3 == 2)
        {
            AdMobInterstitial.Instance.ShowAdMobInterstitial();
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

        SaveSystem.SaveData<StageData>(data, restaurantID);

        // 前のグループの進捗が50%以上の場合、次のグループを順次解放
        bool unlockedNewGroup = TryUnlockNextGroups();

        // 全グループの UnlockableBuyer UI を更新
        UpdateAllUnlockableBuyers();

        // カメラフォーカスの更新
        if (unlockedNewGroup)
        {
            // 新しいグループが解放された場合は2つのUnlockableにフォーカス
            UpdateCameraFocusForGroupUnlock(material);
        }
        else
        {
            // 新しいグループが解放されなかった場合は次のUnlockableのみにフォーカス
            List<Vector3> focusPoints = new List<Vector3>();
            int nextCount = data.UnlockCounts.ContainsKey(material) ? data.UnlockCounts[material] : 0;
            if (nextCount < group.unlockables.Count)
            {
                focusPoints.Add(group.unlockables[nextCount].GetBuyingPoint());
                cameraController.FocusOnPointsAndReturn(focusPoints);
            }
        }
    }

    /// <summary>
    /// すべての解放済み MaterialGroup に対して、UnlockableBuyer の UI を更新または生成します。
    /// </summary>
    private void UpdateAllUnlockableBuyers()
    {
        foreach (var group in materialUnlockables)
        {
            // 解放されていなければスキップ
            if (!data.MaterialUnlocked.ContainsKey(group.materialType) || !data.MaterialUnlocked[group.materialType])
                continue;

            int count = data.UnlockCounts.ContainsKey(group.materialType) ? data.UnlockCounts[group.materialType] : 0;

            // グループが全て購入済みの場合、既存の UI があれば非表示にする
            if (count >= group.unlockables.Count)
            {
                if (activeBuyerInstances.ContainsKey(group.materialType))
                {
                    activeBuyerInstances[group.materialType].gameObject.SetActive(false);
                }
                continue;
            }

            // 未生成ならPrefabから生成
            if (!activeBuyerInstances.ContainsKey(group.materialType) || activeBuyerInstances[group.materialType] == null)
            {
                if (unlockableBuyerPrefab != null)
                {
                    var buyerInstance = Instantiate(unlockableBuyerPrefab);
                    activeBuyerInstances[group.materialType] = buyerInstance;
                }
                else
                {
                    Debug.LogWarning("UnlockableBuyer prefab not assigned.");
                    continue;
                }
            }

            // 対象グループの次に購入可能な Unlockable の情報を取得
            var buyerUI = activeBuyerInstances[group.materialType];
            var nextUnlockable = group.unlockables[count];
            // Calculate price using the group's own baseUnlockPrice and unlockGrowthFactor.
            int price = Mathf.RoundToInt(Mathf.Round(group.baseUnlockPrice * Mathf.Pow(group.unlockGrowthFactor, count)) / 5f) * 5;
            int paid = data.PaidAmounts.ContainsKey(group.materialType) ? data.PaidAmounts[group.materialType] : 0;

            // Update the buyer UI position and initialize it with the associated MaterialType.
            buyerUI.transform.position = nextUnlockable.GetBuyingPoint();
            buyerUI.Initialize(nextUnlockable, price, paid, group.materialType);
            buyerUI.gameObject.SetActive(true);
        }
        // 全体の進捗を通知
        float progress = data.CalculateProgress();
        OnUnlock?.Invoke(progress);
        
        // 進捗率が80%以上で、まだNextStagePromptが表示されていない場合に次のステージへの誘導を表示
        // NextStagePromptShownOnceフラグが一度でもtrueになったら、二度と案内は表示されない
        if (progress >= 0.8f && !data.NextStagePrompt && !data.NextStagePromptShownOnce)
        {
            data.NextStagePrompt = true;
            data.NextStagePromptShownOnce = true;
            SaveSystem.SaveData<StageData>(data, restaurantID);
            progressDisplay.ShowNextStageText();
        }
    }

    /// <summary>
    /// 前の MaterialGroup の進捗が50%以上の場合、順次次の MaterialGroup を解放します。
    /// </summary>
    /// <returns>新しいグループが解放された場合はtrue、それ以外はfalse</returns>
    private bool TryUnlockNextGroups()
    {
        bool unlockedNewGroup = false;
        // 現在のステージ番号を取得
        int currentStageNumber = int.Parse(new string(restaurantID.Where(char.IsDigit).ToArray()));
        
        // 現在のステージのマテリアルタイプの順序を設定
        MaterialType[] order;
        switch (currentStageNumber)
        {
            case 1:
                order = new MaterialType[] { MaterialType.Wood_1, MaterialType.Rock_1 };
                break;
            case 2:
                order = new MaterialType[] { MaterialType.Wood_2, MaterialType.Flower_2, MaterialType.Rock_2 };
                break;
            case 3:
                order = new MaterialType[] { MaterialType.Wood_3, MaterialType.Rock_3, MaterialType.Snow_3, MaterialType.Tomato_3 };
                break;
            case 4:
                order = new MaterialType[] { MaterialType.Wood_4, MaterialType.Rock_4, MaterialType.Flower_4, MaterialType.Car_4, MaterialType.Pig_4 };
                break;
            case 5:
                order = new MaterialType[] { MaterialType.Wood_5, MaterialType.Snow_5, MaterialType.Rock_5, MaterialType.Juice_5, MaterialType.Skeleton_5, MaterialType.Wood2_5 };
                break;
            case 6:
                order = new MaterialType[] { MaterialType.Wood_6, MaterialType.Flower_6, MaterialType.MushRoom_6, MaterialType.Car_6, MaterialType.Rock_6, MaterialType.Wood2_6 };
                break;
            case 7:
                order = new MaterialType[] { MaterialType.Wood_7, MaterialType.Saboten_7, MaterialType.Pumpkin_7, MaterialType.Dumbbell_7, MaterialType.Flower_7, MaterialType.Wood2_7 };
                break;
            default:
                order = new MaterialType[] { };
                break;
        }

        // 2番目以降のグループについて、前のグループの進捗をチェック
        for (int i = 1; i < order.Length; i++)
        {
            MaterialType prev = order[i - 1];
            MaterialType curr = order[i];
            if (data.MaterialUnlocked.ContainsKey(curr) && data.MaterialUnlocked[curr])
                continue; // すでに解放済みならスキップ

            var prevGroup = materialUnlockables.FirstOrDefault(g => g.materialType == prev);
            if (prevGroup == null || prevGroup.unlockables.Count == 0)
                continue;
            int prevCount = data.UnlockCounts.ContainsKey(prev) ? data.UnlockCounts[prev] : 0;
            float progress = prevCount / (float)prevGroup.unlockables.Count;
            if (progress >= 0.5f)
            {
                data.MaterialUnlocked[curr] = true;
                unlockedNewGroup = true;
            }
        }
        return unlockedNewGroup;
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
    /// Gather all active unlockable buying points from unlocked groups and pass them to the CameraController.
    /// </summary>
    private void UpdateCameraFocusForAll()
    {
        List<Vector3> focusPoints = new List<Vector3>();
        foreach (var group in materialUnlockables)
        {
            if (!data.MaterialUnlocked.ContainsKey(group.materialType) || !data.MaterialUnlocked[group.materialType])
                continue;
            int count = data.UnlockCounts.ContainsKey(group.materialType) ? data.UnlockCounts[group.materialType] : 0;
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

    /// <summary>
    /// グループ解放時に2つのUnlockableにフォーカスする
    /// </summary>
    private void UpdateCameraFocusForGroupUnlock(MaterialType unlockedMaterial)
    {
        List<Vector3> focusPoints = new List<Vector3>();
        
        // 解放されたグループの次のUnlockable
        var unlockedGroup = materialUnlockables.FirstOrDefault(g => g.materialType == unlockedMaterial);
        if (unlockedGroup != null)
        {
            int count = data.UnlockCounts.ContainsKey(unlockedMaterial) ? data.UnlockCounts[unlockedMaterial] : 0;
            if (count < unlockedGroup.unlockables.Count)
            {
                focusPoints.Add(unlockedGroup.unlockables[count].GetBuyingPoint());
            }
        }

        // 次のグループの最初のUnlockable
        var nextGroup = materialUnlockables.FirstOrDefault(g => !data.MaterialUnlocked.ContainsKey(g.materialType) || !data.MaterialUnlocked[g.materialType]);
        if (nextGroup != null)
        {
            focusPoints.Add(nextGroup.unlockables[0].GetBuyingPoint());
        }

        if (focusPoints.Count > 0)
        {
            cameraController.FocusOnPointsAndReturn(focusPoints);
        }
    }
}
