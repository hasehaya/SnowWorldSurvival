using GoogleMobileAds.Api;

using UnityEngine;

public class AdMobRewardInterstitial :MonoBehaviour
{
    private static AdMobRewardInterstitial instance;
    public static AdMobRewardInterstitial Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdMobRewardInterstitial>();
            }
            return instance;
        }
    }
    //��邱��
    //1.�����[�h�L��ID�̓���
    //2.GetReward�֐��ɕ�V���e�����
    //3.�����[�h�N���ݒ�@ShowAdMobReward()���g��

    private RewardedInterstitialAd rewardedInterstitialAd;//RewardedAd�^�̕ϐ� rewardedAd��錾 ���̒��Ƀ����[�h�L���̏�񂪓���

    private string adUnitId;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAdRemoved())
        {
            return;
        }

        //Android��iOS�ōL��ID���Ⴄ�̂Ńv���b�g�t�H�[���ŏ����𕪂��܂��B
        // �Q�l
        //�yUnity�zAndroid��iOS�ŏ����𕪂�����@
        // https://marumaro7.hatenablog.com/entry/platformsyoriwakeru

#if UNITY_ANDROID
        adUnitId = "ca-app-pub-2788807416533951/7735739968";
#elif UNITY_IPHONE
        adUnitId = "ca-app-pub-2788807416533951/9675174452";//������iOS�̃����[�h�L��ID�����
#else
        adUnitId = "unexpected_platform";
#endif

        //�����[�h �ǂݍ��݊J�n
        Debug.Log("Rewarded ad load start");

        LoadRewardedAd();//�����[�h�L���ǂݍ���
    }

    public void DestroyAd()
    {
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }
    }

    //�����[�h�L����\������֐�
    //�{�^���Ɋ��t�����Ďg�p
    public void ShowAdMobReward()
    {
        //�ϐ�rewardedAd�̒��g�����݂��Ă���A�L���̓ǂݍ��݂��������Ă�����L���\��
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd() == true)
        {
            //�����[�h�L�� �\�������{�@��V�̎󂯎��̊֐�GetReward�������ɐݒ�
            rewardedInterstitialAd.Show(GetReward);
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
        //��V�󂯎��
        Debug.Log("GetReward");

        //�����ɕ�V�̏���������
    }

    //�����[�h�L����ǂݍ��ފ֐� �ēǂݍ��݂ɂ��g�p
    public void LoadRewardedAd()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAdRemoved())
        {
            return;
        }

        //�L���̍ēǂݍ��݂̂��߂̏���
        //rewardedAd�̒��g�������Ă����ꍇ����
        if (rewardedInterstitialAd != null)
        {
            //�����[�h�L���͎g���̂ĂȂ̂ň�U�j��
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
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
        RewardedInterstitialAd.Load(adUnitId, request, OnRewardedAdLoaded);
    }


    // �L���̃��[�h�����{������ɌĂяo�����֐�
    private void OnRewardedAdLoaded(RewardedInterstitialAd ad, LoadAdError error)
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
        rewardedInterstitialAd = ad;

        //�L���� �\���E�\���I���E�\�����s �̓��e��o�^
        RegisterEventHandlers(rewardedInterstitialAd);
    }


    //�L���� �\���E�\���I���E�\�����s �̓��e
    private void RegisterEventHandlers(RewardedInterstitialAd ad)
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
