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

    // ���ʂ̎������ԁi��F180�b��3���j
    private const float TreeMinutesDuration = 180f;

    // ���p�\�� RewardType ���Ǘ����郊�X�g
    private List<RewardType> availableRewardTypes = new List<RewardType>();

    // GlobalData �ւ̎Q�ƁiGameManager �o�R�Ŏ擾�j
    private GlobalData globalData;

    private void Start()
    {
        availableRewardTypes.Add(RewardType.PlayerSpeed);
        availableRewardTypes.Add(RewardType.PlayerCapacity);
        availableRewardTypes.Add(RewardType.MoneyCollection);

        popPoses = GetComponentsInChildren<Transform>();
        // ��������������|�W�V�����V���b�t���Ȃ�

        // GameManager �o�R�� GlobalData ���Q��
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
            Debug.LogWarning("���p�\�ȃ|�W�V����������܂���B");
            return;
        }

        Transform spawnPos = availablePoses[UnityEngine.Random.Range(0, availablePoses.Count)];
        List<AdObject> candidateAds = new List<AdObject>();

        foreach (var ad in adObjects)
        {
            if (ad.RewardEffect == RewardEffect.TreeMinutes &&
               (ad.RewardType == RewardType.PlayerSpeed || ad.RewardType == RewardType.PlayerCapacity || ad.RewardType == RewardType.MoneyCollection))
            {
                // �Y��������ʂ����ɃA�N�e�B�u�Ȃ��₩�珜�O
                if ((ad.RewardType == RewardType.PlayerSpeed && globalData.IsPlayerSpeedActive) ||
                    (ad.RewardType == RewardType.PlayerCapacity && globalData.IsPlayerCapacityActive) ||
                    (ad.RewardType == RewardType.MoneyCollection && globalData.IsMoneyCollectionActive))
                    continue;
            }
            candidateAds.Add(ad);
        }

        if (candidateAds.Count == 0)
        {
            Debug.LogWarning("���p�\�� RewardType �� AdObject ������܂���B");
            return;
        }

        AdObject newAd = Instantiate(candidateAds[UnityEngine.Random.Range(0, candidateAds.Count)], spawnPos.position, spawnPos.rotation);
        newAd.OnShowAd += OnAdObjectShow;
        activeAdObjects.Add(newAd);
        activeAdPositions.Add(newAd, spawnPos);
    }
}
