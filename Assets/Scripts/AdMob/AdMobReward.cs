using System;

using GoogleMobileAds.Api;

using UnityEngine;

public enum RewardType
{
    None,
    Money,
    PlayerSpeed,
    PlayerCapacity,
    Upgrade,
}

public class AdMobReward :MonoBehaviour
{
    private static AdMobReward instance;
    public static AdMobReward Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdMobReward>();
            }
            return instance;
        }
    }
    public event Action<RewardType> OnRewardReceived;

    //��邱��
    //1.�����[�h�L��ID�̓���
    //2.GetReward�֐��ɕ�V���e�����
    //3.�����[�h�N���ݒ�@ShowAdMobReward()���g��


    private RewardedAd rewardedAd;//RewardedAd�^�̕ϐ� rewardedAd��錾 ���̒��Ƀ����[�h�L���̏�񂪓���

    private string adUnitId;
    private RewardType rewardType;

    private void Start()
    {
        // �L���폜�ς݂Ȃ烊�N�G�X�g���Ȃ�
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        //Android��iOS�ōL��ID���Ⴄ�̂Ńv���b�g�t�H�[���ŏ����𕪂��܂��B
        // �Q�l
        //�yUnity�zAndroid��iOS�ŏ����𕪂�����@
        // https://marumaro7.hatenablog.com/entry/platformsyoriwakeru

#if UNITY_ANDROID
        adUnitId = "ca-app-pub-2788807416533951/3130480235";
#elif UNITY_IPHONE
        adUnitId = "ca-app-pub-2788807416533951/7766162572";
#else
        adUnitId = "unexpected_platform";
#endif

        //�����[�h �ǂݍ��݊J�n
        Debug.Log("Rewarded ad load start");

        LoadRewardedAd();//�����[�h�L���ǂݍ���
    }

    public void DestroyAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }

    //�����[�h�L����\������֐�
    //�{�^���Ɋ��t�����Ďg�p
    public void ShowAdMobReward(RewardType rewardType)
    {
        // �L���폜�ς݂Ȃ烊�N�G�X�g���Ȃ�
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            OnRewardReceived?.Invoke(rewardType);
            return;
        }
        //�ϐ�rewardedAd�̒��g�����݂��Ă���A�L���̓ǂݍ��݂��������Ă�����L���\��
        if (rewardedAd != null && rewardedAd.CanShowAd() == true)
        {
            this.rewardType = rewardType;
            rewardedAd.Show(GetReward);
        }
        else
        {
            //�����[�h�L���ǂݍ��ݖ�����
            Debug.Log("Rewarded ad not loaded");
        }
    }

    //��V�󂯎�菈��
    private void GetReward(Reward reward)
    {
        OnRewardReceived?.Invoke(rewardType);
    }


    //�����[�h�L����ǂݍ��ފ֐� �ēǂݍ��݂ɂ��g�p
    public void LoadRewardedAd()
    {
        //�L���̍ēǂݍ��݂̂��߂̏���
        //rewardedAd�̒��g�������Ă����ꍇ����
        if (rewardedAd != null)
        {
            //�����[�h�L���͎g���̂ĂȂ̂ň�U�j��
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        //���N�G�X�g�𐶐�
        AdRequest request = new AdRequest();

        //�L���̃L�[���[�h��ǉ�
        //===================================================================
        // �A�v���Ɋ֘A����L�[���[�h�𕶎���Őݒ肷��ƃA�v���ƍL���̊֘A�������܂�܂��B
        // ���ʁA���v���オ��\��������܂��B
        // �C�Ӑݒ�̂��ߕs�v�ł���Ώ����Ă��������Ė��͂���܂���B

        // Application.systemLanguage��OS�̌��ꔻ�ʁ@
        // �Ԃ�l��SystemLanguage.����
        // �[���̌��ꂪ���{��̎�
        if (Application.systemLanguage == SystemLanguage.Japanese)
        {
            request.Keywords.Add("�Q�[��");
            request.Keywords.Add("���o�C���Q�[��");
        }

        //�[���̌��ꂪ���{��ȊO�̎�
        else
        {
            request.Keywords.Add("game");
            request.Keywords.Add("mobile games");
        }
        //==================================================================

        //�L�������[�h  ���̌�A�֐�OnRewardedAdLoaded���Ăяo��
        RewardedAd.Load(adUnitId, request, OnRewardedAdLoaded);
    }


    // �L���̃��[�h�����{������ɌĂяo�����֐�
    private void OnRewardedAdLoaded(RewardedAd ad, LoadAdError error)
    {
        //�ϐ�error�ɏ�񂪓����Ă���@�܂��́A�ϐ�ad�ɏ�񂪂͂����Ă��Ȃ���������s
        if (error != null || ad == null)
        {
            //�����[�h �ǂݍ��ݎ��s
            Debug.LogError("Failed to load reward ad : " + error);//error:�G���[���e 
            Invoke("LoadRewardedAd", 3f);
            return;//���̎��_�ł��̊֐��̎��s�͏I��
        }

        //�����[�h �ǂݍ��݊���
        Debug.Log("Reward ad loaded");

        //RewardedAd.Load(~��~)�֐������s���邱�Ƃɂ��ARewardedAd�^�̕ϐ�ad��RewardedAd�̃C���X�^���X�𐶐�����B
        //��������RewardedAd�^�̃C���X�^���X��ϐ�rewardedAd�֊��蓖��
        rewardedAd = ad;

        //�L���� �\���E�\���I���E�\�����s �̓��e��o�^
        RegisterEventHandlers(rewardedAd);
    }


    //�L���� �\���E�\���I���E�\�����s �̓��e
    private void RegisterEventHandlers(RewardedAd ad)
    {
        //�����[�h�L�����\�����ꂽ���ɋN��������e
        ad.OnAdFullScreenContentOpened += () =>
        {
            //�����[�h�L�� �\��
            Debug.Log("Rewarded ad full screen content opened.");
        };

        //�����[�h�L�����\���I�� �ƂȂ������ɋN��������e
        ad.OnAdFullScreenContentClosed += () =>
        {
            //�����[�h�L�� �\���I��
            Debug.Log("Rewarded ad full screen content closed.");

            //�����[�h �ēǂݍ���
            LoadRewardedAd();
        };

        //�����[�h�L���̕\�����s �ƂȂ������ɋN��������e
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            //�G���[�\��
            Debug.LogError("Rewarded ad failed to open full screen content with error : " + error);

            //�����[�h �ēǂݍ���
            LoadRewardedAd();
        };
    }
}
