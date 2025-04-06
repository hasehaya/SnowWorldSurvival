using System.Collections;
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
            {
                instance = FindObjectOfType<AdObjectManager>();
            }
            return instance;
        }
    }

    [SerializeField]
    private AdObject[] adObjects;

    private Transform[] popPoses;

    // 現在アクティブな AdObject とその生成位置の管理
    private List<AdObject> activeAdObjects = new List<AdObject>();
    private Dictionary<AdObject, Transform> activeAdPositions = new Dictionary<AdObject, Transform>();

    // Speed と Amount の TreeMinutes 効果がアクティブかどうかのプロパティ
    public bool IsSpeedEffectActive { get; private set; }
    public bool IsAmountEffectActive { get; private set; }

    private const float TreeMinutesDuration = 180f; // 3分

    // 利用可能な RewardType を管理（ActiveAdPositions と同様の考え方）
    private List<RewardType> availableRewardTypes = new List<RewardType>();

    private void Start()
    {
        // 利用可能な RewardType を初期化（必要に応じて他の RewardType も追加）
        availableRewardTypes.Add(RewardType.PlayerSpeed);
        availableRewardTypes.Add(RewardType.PlayerCapacity);

        popPoses = GetComponentsInChildren<Transform>();

        // popPoses をシャッフル
        List<Transform> posesList = new List<Transform>(popPoses);
        for (int i = 0; i < posesList.Count; i++)
        {
            Transform temp = posesList[i];
            int randomIndex = Random.Range(i, posesList.Count);
            posesList[i] = posesList[randomIndex];
            posesList[randomIndex] = temp;
        }

        // 使用するポジションは半分とする
        int halfCount = posesList.Count / 2;
        int count = Mathf.Min(halfCount, adObjects.Length);

        for (int i = 0; i < count; i++)
        {
            Transform spawnPos = posesList[i];
            AdObject adInstance = Instantiate(adObjects[i], spawnPos.position, spawnPos.rotation);
            adInstance.OnShowAd += OnAdObjectShow;
            activeAdObjects.Add(adInstance);
            activeAdPositions.Add(adInstance, spawnPos);
        }
    }

    // AdObject の ShowAd イベント発生時の処理
    private void OnAdObjectShow(AdObject adObj)
    {
        adObj.OnShowAd -= OnAdObjectShow;
        activeAdObjects.Remove(adObj);
        activeAdPositions.Remove(adObj);
        Destroy(adObj.gameObject);

        // TreeMinutes 効果の場合、該当 RewardType を利用不可にしてタイマーを開始
        if (adObj.RewardEffect == RewardEffect.TreeMinutes)
        {
            if (adObj.RewardType == RewardType.PlayerSpeed)
            {
                if (!IsSpeedEffectActive)
                {
                    // 利用可能リストから除外
                    availableRewardTypes.Remove(RewardType.PlayerSpeed);
                    StartCoroutine(SpeedEffectTimer());
                }
                return;
            }
            else if (adObj.RewardType == RewardType.PlayerCapacity)
            {
                if (!IsAmountEffectActive)
                {
                    availableRewardTypes.Remove(RewardType.PlayerCapacity);
                    StartCoroutine(AmountEffectTimer());
                }
                return;
            }
        }

        // その他の種類（または Once など）の場合は直ちに再生成
        RepopAdObject();
    }

    // Speed 効果のタイマー
    private IEnumerator SpeedEffectTimer()
    {
        IsSpeedEffectActive = true;
        yield return new WaitForSeconds(TreeMinutesDuration);
        IsSpeedEffectActive = false;
        // 効果終了後、利用可能リストに再追加
        if (!availableRewardTypes.Contains(RewardType.PlayerSpeed))
            availableRewardTypes.Add(RewardType.PlayerSpeed);
        RepopAdObject();
    }

    // Amount 効果のタイマー
    private IEnumerator AmountEffectTimer()
    {
        IsAmountEffectActive = true;
        yield return new WaitForSeconds(TreeMinutesDuration);
        IsAmountEffectActive = false;
        if (!availableRewardTypes.Contains(RewardType.PlayerCapacity))
            availableRewardTypes.Add(RewardType.PlayerCapacity);
        RepopAdObject();
    }

    // 再生成処理：未使用の spawn position を探し、利用可能な RewardType の AdObject を生成する
    private void RepopAdObject()
    {
        List<Transform> availablePoses = new List<Transform>();
        foreach (Transform pos in popPoses)
        {
            if (!activeAdPositions.ContainsValue(pos))
            {
                availablePoses.Add(pos);
            }
        }

        if (availablePoses.Count == 0)
        {
            Debug.LogWarning("利用可能なポジションがありません。");
            return;
        }

        Transform spawnPos = availablePoses[Random.Range(0, availablePoses.Count)];

        // 利用可能な RewardType に基づいて生成可能な AdObject をフィルタリング
        List<AdObject> candidateAds = new List<AdObject>();
        foreach (var ad in adObjects)
        {
            // TreeMinutes 効果の場合、RewardType が availableRewardTypes に含まれているか確認
            if (ad.RewardEffect == RewardEffect.TreeMinutes &&
                (ad.RewardType == RewardType.PlayerSpeed || ad.RewardType == RewardType.PlayerCapacity))
            {
                if (!availableRewardTypes.Contains(ad.RewardType))
                    continue;
            }
            candidateAds.Add(ad);
        }

        if (candidateAds.Count == 0)
        {
            Debug.LogWarning("利用可能な RewardType の AdObject がありません。");
            return;
        }

        // ランダムに候補から選択して生成
        AdObject newAd = Instantiate(candidateAds[Random.Range(0, candidateAds.Count)], spawnPos.position, spawnPos.rotation);
        newAd.OnShowAd += OnAdObjectShow;
        activeAdObjects.Add(newAd);
        activeAdPositions.Add(newAd, spawnPos);
    }
}
