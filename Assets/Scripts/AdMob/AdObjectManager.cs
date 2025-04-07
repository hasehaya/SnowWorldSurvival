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

    // ���݃A�N�e�B�u�� AdObject �Ƃ��̐����ʒu�̊Ǘ�
    private List<AdObject> activeAdObjects = new List<AdObject>();
    private Dictionary<AdObject, Transform> activeAdPositions = new Dictionary<AdObject, Transform>();

    // Speed �� Amount �� TreeMinutes ���ʂ��A�N�e�B�u���ǂ����̃v���p�e�B
    public bool IsSpeedEffectActive { get; private set; }
    public bool IsAmountEffectActive { get; private set; }

    private const float TreeMinutesDuration = 180f; // 3��

    // ���p�\�� RewardType ���Ǘ��iActiveAdPositions �Ɠ��l�̍l�����j
    private List<RewardType> availableRewardTypes = new List<RewardType>();

    private void Start()
    {
        // ���p�\�� RewardType ���������i�K�v�ɉ����đ��� RewardType ���ǉ��j
        availableRewardTypes.Add(RewardType.PlayerSpeed);
        availableRewardTypes.Add(RewardType.PlayerCapacity);

        popPoses = GetComponentsInChildren<Transform>();

        // popPoses ���V���b�t��
        List<Transform> posesList = new List<Transform>(popPoses);
        for (int i = 0; i < posesList.Count; i++)
        {
            Transform temp = posesList[i];
            int randomIndex = Random.Range(i, posesList.Count);
            posesList[i] = posesList[randomIndex];
            posesList[randomIndex] = temp;
        }

        // �g�p����|�W�V�����͔����Ƃ���
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

    // AdObject �� ShowAd �C�x���g�������̏���
    private void OnAdObjectShow(AdObject adObj)
    {
        adObj.OnShowAd -= OnAdObjectShow;
        activeAdObjects.Remove(adObj);
        activeAdPositions.Remove(adObj);
        Destroy(adObj.gameObject);

        // TreeMinutes ���ʂ̏ꍇ�A�Y�� RewardType �𗘗p�s�ɂ��ă^�C�}�[���J�n
        if (adObj.RewardEffect == RewardEffect.TreeMinutes)
        {
            if (adObj.RewardType == RewardType.PlayerSpeed)
            {
                if (!IsSpeedEffectActive)
                {
                    // ���p�\���X�g���珜�O
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

        // ���̑��̎�ށi�܂��� Once �Ȃǁj�̏ꍇ�͒����ɍĐ���
        RepopAdObject();
    }

    // Speed ���ʂ̃^�C�}�[
    private IEnumerator SpeedEffectTimer()
    {
        IsSpeedEffectActive = true;
        yield return new WaitForSeconds(TreeMinutesDuration);
        IsSpeedEffectActive = false;
        // ���ʏI����A���p�\���X�g�ɍĒǉ�
        if (!availableRewardTypes.Contains(RewardType.PlayerSpeed))
            availableRewardTypes.Add(RewardType.PlayerSpeed);
        RepopAdObject();
    }

    // Amount ���ʂ̃^�C�}�[
    private IEnumerator AmountEffectTimer()
    {
        IsAmountEffectActive = true;
        yield return new WaitForSeconds(TreeMinutesDuration);
        IsAmountEffectActive = false;
        if (!availableRewardTypes.Contains(RewardType.PlayerCapacity))
            availableRewardTypes.Add(RewardType.PlayerCapacity);
        RepopAdObject();
    }

    // �Đ��������F���g�p�� spawn position ��T���A���p�\�� RewardType �� AdObject �𐶐�����
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
            Debug.LogWarning("���p�\�ȃ|�W�V����������܂���B");
            return;
        }

        Transform spawnPos = availablePoses[Random.Range(0, availablePoses.Count)];

        // ���p�\�� RewardType �Ɋ�Â��Đ����\�� AdObject ���t�B���^�����O
        List<AdObject> candidateAds = new List<AdObject>();
        foreach (var ad in adObjects)
        {
            // TreeMinutes ���ʂ̏ꍇ�ARewardType �� availableRewardTypes �Ɋ܂܂�Ă��邩�m�F
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
            Debug.LogWarning("���p�\�� RewardType �� AdObject ������܂���B");
            return;
        }

        // �����_���Ɍ�₩��I�����Đ���
        AdObject newAd = Instantiate(candidateAds[Random.Range(0, candidateAds.Count)], spawnPos.position, spawnPos.rotation);
        newAd.OnShowAd += OnAdObjectShow;
        activeAdObjects.Add(newAd);
        activeAdPositions.Add(newAd, spawnPos);
    }
}
