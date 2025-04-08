using System.Collections.Generic;

using UnityEngine;

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

    // 効果の持続時間（例：180秒＝3分）
    private const float TreeMinutesDuration = 180f;

    // 利用可能な RewardType を管理するリスト
    private List<RewardType> availableRewardTypes = new List<RewardType>();

    // GlobalData への参照（GameManager 経由で取得）
    private GlobalData globalData;

    private void Start()
    {
        availableRewardTypes.Add(RewardType.PlayerSpeed);
        availableRewardTypes.Add(RewardType.PlayerCapacity);
        availableRewardTypes.Add(RewardType.MoneyCollection);

        popPoses = GetComponentsInChildren<Transform>();
        // 初期生成処理やポジションシャッフルなど

        // GameManager 経由で GlobalData を参照
        globalData = GameManager.Instance.GlobalData;
    }

    private void OnAdObjectShow(AdObject adObj)
    {
        adObj.OnShowAd -= OnAdObjectShow;
        activeAdObjects.Remove(adObj);
        activeAdPositions.Remove(adObj);
        Destroy(adObj.gameObject);

        if (adObj.RewardEffect == RewardEffect.TreeMinutes)
        {
            if (adObj.RewardType == RewardType.PlayerSpeed && !globalData.IsPlayerSpeedActive)
            {
                availableRewardTypes.Remove(RewardType.PlayerSpeed);
                globalData.PlayerSpeedRemainingSeconds = TreeMinutesDuration;
                return;
            }
            else if (adObj.RewardType == RewardType.PlayerCapacity && !globalData.IsPlayerCapacityActive)
            {
                availableRewardTypes.Remove(RewardType.PlayerCapacity);
                globalData.PlayerCapacityRemainingSeconds = TreeMinutesDuration;
                return;
            }
            else if (adObj.RewardType == RewardType.MoneyCollection && !globalData.IsMoneyCollectionActive)
            {
                availableRewardTypes.Remove(RewardType.MoneyCollection);
                globalData.MoneyCollectionRemainingSeconds = TreeMinutesDuration;
                return;
            }
        }

        RepopAdObject();
    }

    private void RepopAdObject()
    {
        List<Transform> availablePoses = new List<Transform>();
        foreach (Transform pos in popPoses)
        {
            if (!activeAdPositions.ContainsValue(pos))
                availablePoses.Add(pos);
        }

        if (availablePoses.Count == 0)
        {
            Debug.LogWarning("利用可能なポジションがありません。");
            return;
        }

        Transform spawnPos = availablePoses[UnityEngine.Random.Range(0, availablePoses.Count)];
        List<AdObject> candidateAds = new List<AdObject>();

        foreach (var ad in adObjects)
        {
            if (ad.RewardEffect == RewardEffect.TreeMinutes &&
               (ad.RewardType == RewardType.PlayerSpeed || ad.RewardType == RewardType.PlayerCapacity || ad.RewardType == RewardType.MoneyCollection))
            {
                // 該当する効果が既にアクティブなら候補から除外
                if ((ad.RewardType == RewardType.PlayerSpeed && globalData.IsPlayerSpeedActive) ||
                    (ad.RewardType == RewardType.PlayerCapacity && globalData.IsPlayerCapacityActive) ||
                    (ad.RewardType == RewardType.MoneyCollection && globalData.IsMoneyCollectionActive))
                    continue;
            }
            candidateAds.Add(ad);
        }

        if (candidateAds.Count == 0)
        {
            Debug.LogWarning("利用可能な RewardType の AdObject がありません。");
            return;
        }

        AdObject newAd = Instantiate(candidateAds[UnityEngine.Random.Range(0, candidateAds.Count)], spawnPos.position, spawnPos.rotation);
        newAd.OnShowAd += OnAdObjectShow;
        activeAdObjects.Add(newAd);
        activeAdPositions.Add(newAd, spawnPos);
    }
}
