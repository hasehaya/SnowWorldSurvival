using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// マップ上のAdObjectの生成・配置・管理を担当するクラス
/// </summary>
public class AdObjectManager :MonoBehaviour
{
    private static AdObjectManager instance;
    public static AdObjectManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<AdObjectManager>();
            return instance;
        }
    }

    [SerializeField]
    private AdObject[] adObjects;
    private Transform[] popPoses;
    private List<AdObject> activeAdObjects = new List<AdObject>();
    private Dictionary<AdObject, Transform> activeAdPositions = new Dictionary<AdObject, Transform>();
    
    // 最後に生成したAdObjectのプレハブインデックスを記録
    private List<int> recentlyUsedAdObjectIndices = new List<int>();
    private const int MAX_RECENT_HISTORY = 3; // 過去何個のAdObjectを記憶するか

    // 効果の持続時間（例：180秒＝3分）
    private const float TreeMinutesDuration = 180f;
    
    // AdObjectのリポップのクールタイム（1分間）
    private const float RepopCooldownTime = 60f;
    private bool isRepopCooldown = false;

    // 利用可能な RewardType を管理するリスト
    private List<RewardType> availableRewardTypes = new List<RewardType>();

    // GlobalData への参照（GameManager 経由で取得）
    private GlobalData globalData;

    // マップ上に表示するAdObjectの最大数（実際のpopPosesの半分）
    private int maxActiveAdObjects;
    
    // 効果が切れたときの確認用
    private bool wasPlayerSpeedActive;
    private bool wasPlayerCapacityActive;
    private bool wasMoneyCollectionActive;

    private void Start()
    {
        availableRewardTypes.Add(RewardType.PlayerSpeed);
        availableRewardTypes.Add(RewardType.PlayerCapacity);
        availableRewardTypes.Add(RewardType.MoneyCollection);
        availableRewardTypes.Add(RewardType.Money);

        popPoses = GetComponentsInChildren<Transform>();
        // popPosesの一つ目は自分自身なので除外して、半分の数を計算
        maxActiveAdObjects = (popPoses.Length - 1) / 2;
        
        // GameManager 経由で GlobalData を参照
        globalData = GameManager.Instance.GlobalData;
        
        // 初期状態を記録
        wasPlayerSpeedActive = globalData.IsPlayerSpeedActive;
        wasPlayerCapacityActive = globalData.IsPlayerCapacityActive;
        wasMoneyCollectionActive = globalData.IsMoneyCollectionActive;
        
        // 効果終了イベントを購読
        CoolTimePresenter.OnEffectEnded += HandleEffectEnded;
        
        // AdMobRewardのイベントを購読（効果付与後に使用可能なRewardTypeを更新するため）
        AdMobReward.Instance.OnRewardReceived += HandleRewardReceived;
        
        // 初期生成処理
        RepopAllAdObjects();
    }
    
    private void OnDestroy()
    {
        // イベント購読を解除
        CoolTimePresenter.OnEffectEnded -= HandleEffectEnded;
        
        if (AdMobReward.Instance != null)
        {
            AdMobReward.Instance.OnRewardReceived -= HandleRewardReceived;
        }
    }
    
    // 効果終了時の処理
    private void HandleEffectEnded(RewardType rewardType)
    {
        if (rewardType == RewardType.PlayerSpeed ||
            rewardType == RewardType.PlayerCapacity ||
            rewardType == RewardType.MoneyCollection)
        {
            availableRewardTypes.Add(rewardType);
            RepopAdObject();
        }
    }
    
    // 報酬受け取り時の処理（利用可能なRewardTypeを更新）
    private void HandleRewardReceived(RewardType rewardType)
    {
        // 効果が付与されるRewardTypeの場合、利用可能リストから削除
        if (rewardType == RewardType.PlayerSpeed ||
            rewardType == RewardType.PlayerCapacity ||
            rewardType == RewardType.MoneyCollection)
        {
            availableRewardTypes.Remove(rewardType);
            
            // AdObjectを再配置（クールダウン付き）
            StartCoroutine(RepopWithCooldown());
        }
    }

    private void Update()
    {
        // 効果の有効状態が変わった場合（効果が切れた場合）に再生成する
        if (wasPlayerSpeedActive && !globalData.IsPlayerSpeedActive)
        {
            availableRewardTypes.Add(RewardType.PlayerSpeed);
            RepopAdObject();
        }
        
        if (wasPlayerCapacityActive && !globalData.IsPlayerCapacityActive)
        {
            availableRewardTypes.Add(RewardType.PlayerCapacity);
            RepopAdObject();
        }
        
        if (wasMoneyCollectionActive && !globalData.IsMoneyCollectionActive)
        {
            availableRewardTypes.Add(RewardType.MoneyCollection);
            RepopAdObject();
        }
        
        // 状態を更新
        wasPlayerSpeedActive = globalData.IsPlayerSpeedActive;
        wasPlayerCapacityActive = globalData.IsPlayerCapacityActive;
        wasMoneyCollectionActive = globalData.IsMoneyCollectionActive;
    }

    private void OnAdObjectShow(AdObject adObj)
    {
        adObj.OnShowAd -= OnAdObjectShow;
        activeAdObjects.Remove(adObj);
        activeAdPositions.Remove(adObj);
        Destroy(adObj.gameObject);

        // AdObject表示後、クールダウン付きで新しいオブジェクトを生成
        StartCoroutine(RepopWithCooldown());
    }
    
    /// <summary>
    /// クールダウン付きでAdObjectを再生成するコルーチン
    /// </summary>
    private IEnumerator RepopWithCooldown()
    {
        if (!isRepopCooldown)
        {
            isRepopCooldown = true;
            yield return new WaitForSeconds(RepopCooldownTime);
            isRepopCooldown = false;
            RepopAdObject();
        }
    }

    // すべての利用可能なポジションに対してAdObjectを再生成
    private void RepopAllAdObjects()
    {
        // AdBlockされておらず、かつ広告がロードされていない場合は生成しない
        if (GameManager.Instance != null && !GameManager.Instance.IsAdBlocked() && 
            AdMobReward.Instance != null && !AdMobReward.Instance.IsAdLoaded)
        {
            return;
        }
        
        // 既存のオブジェクトをクリア
        foreach (var adObj in activeAdObjects)
        {
            adObj.OnShowAd -= OnAdObjectShow;
            Destroy(adObj.gameObject);
        }
        activeAdObjects.Clear();
        activeAdPositions.Clear();
        recentlyUsedAdObjectIndices.Clear();

        // 最大数まで再生成
        for (int i = 0; i < maxActiveAdObjects; i++)
        {
            RepopAdObject();
            
            // 利用可能なポジションがなくなった場合は終了
            if (GetAvailablePoses().Count == 0)
                break;
        }
    }

    // 利用可能なポジションのリストを取得
    private List<Transform> GetAvailablePoses()
    {
        List<Transform> availablePoses = new List<Transform>();
        // popPosesの0番目は自分自身なので、1から開始
        for (int i = 1; i < popPoses.Length; i++)
        {
            if (!activeAdPositions.ContainsValue(popPoses[i]))
                availablePoses.Add(popPoses[i]);
        }
        return availablePoses;
    }

    // 有効なAdObjectの最大数を計算
    private int CalculateMaxAdObjects()
    {
        // 基本は最大数の半分
        int maxCount = maxActiveAdObjects;
        
        // 効果が有効なRewardTypeの数だけ減らす
        int activeEffects = 0;
        if (globalData.IsPlayerSpeedActive) activeEffects++;
        if (globalData.IsPlayerCapacityActive) activeEffects++;
        if (globalData.IsMoneyCollectionActive) activeEffects++;
        
        // 最低でも1つは残す
        return Mathf.Max(1, maxCount - activeEffects);
    }

    // 直近に使用したAdObjectインデックスを記録
    private void AddToRecentlyUsed(int index)
    {
        recentlyUsedAdObjectIndices.Add(index);
        if (recentlyUsedAdObjectIndices.Count > MAX_RECENT_HISTORY)
        {
            recentlyUsedAdObjectIndices.RemoveAt(0);
        }
    }

    private void RepopAdObject()
    {
        // AdBlockされておらず、かつ広告がロードされていない場合は生成しない
        if (GameManager.Instance != null && !GameManager.Instance.IsAdBlocked() && 
            AdMobReward.Instance != null && !AdMobReward.Instance.IsAdLoaded)
        {
            return;
        }
        
        // クールダウン中は生成しない
        if (isRepopCooldown)
        {
            return;
        }
        
        // 現在の最大AdObject数を計算
        int currentMaxAdObjects = CalculateMaxAdObjects();
        
        // すでに最大数に達している場合は生成しない
        if (activeAdObjects.Count >= currentMaxAdObjects)
            return;
            
        List<Transform> availablePoses = GetAvailablePoses();
        
        if (availablePoses.Count == 0)
        {
            Debug.LogWarning("利用可能なポジションがありません。");
            return;
        }

        Transform spawnPos = availablePoses[UnityEngine.Random.Range(0, availablePoses.Count)];
        List<AdObject> candidateAds = new List<AdObject>();
        List<int> candidateIndices = new List<int>();

        for (int i = 0; i < adObjects.Length; i++)
        {
            var ad = adObjects[i];
            
            // 最近使用したAdObjectは除外
            if (recentlyUsedAdObjectIndices.Contains(i))
                continue;
                
            // 効果が発動中のRewardTypeと同じAdObjectは除外
            if (globalData.IsPlayerSpeedActive && ad.RewardType == RewardType.PlayerSpeed)
                continue;
            if (globalData.IsPlayerCapacityActive && ad.RewardType == RewardType.PlayerCapacity)
                continue;
            if (globalData.IsMoneyCollectionActive && ad.RewardType == RewardType.MoneyCollection)
                continue;
            
            // アクティブなAdObjectとして既に存在する場合はスキップ
            bool alreadyActive = false;
            foreach (var activeAd in activeAdObjects)
            {
                if (activeAd.RewardType == ad.RewardType && activeAd.RewardEffect == ad.RewardEffect)
                {
                    alreadyActive = true;
                    break;
                }
            }
            if (alreadyActive)
                continue;
                
            candidateAds.Add(ad);
            candidateIndices.Add(i);
        }

        if (candidateAds.Count == 0)
        {
            Debug.LogWarning("利用可能な RewardType の AdObject がありません。");
            
            // 候補がない場合は履歴をリセットして再トライ
            if (recentlyUsedAdObjectIndices.Count > 0)
            {
                recentlyUsedAdObjectIndices.Clear();
                RepopAdObject();
            }
            return;
        }

        int selectedIndex = UnityEngine.Random.Range(0, candidateAds.Count);
        AdObject newAd = Instantiate(candidateAds[selectedIndex], spawnPos.position, spawnPos.rotation);
        
        // 使用したインデックスを記録
        AddToRecentlyUsed(candidateIndices[selectedIndex]);
        
        newAd.OnShowAd += OnAdObjectShow;
        activeAdObjects.Add(newAd);
        activeAdPositions.Add(newAd, spawnPos);
    }
}
