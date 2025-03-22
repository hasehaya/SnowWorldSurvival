using System.Collections;

using GoogleMobileAds.Api;

using UnityEngine;

public class AdMobBanner :MonoBehaviour
{
    private BannerView bannerView;
    private bool isRetrying = false; // ���g���C�����ǂ�������
    private float retryDelay = 3f;   // ���g���C�Ԋu�i�b�j

    private void Start()
    {
        RequestBanner();
    }

    public void BannerStart()
    {
        RequestBanner();
    }

    public void BannerDestroy()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
    }

    private void RequestBanner()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        string adUnitId = "unexpected_platform";
#endif

        // �Â��o�i�[���c���Ă���폜
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        bannerView = new BannerView(adUnitId, adaptiveSize, AdPosition.Top);

        // �R�[���o�b�N�ݒ�
        bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
        bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;

        // ���N�G�X�g�쐬
        AdRequest adRequest = new AdRequest();
        if (Application.systemLanguage == SystemLanguage.Japanese)
        {
            adRequest.Keywords.Add("�Q�[��");
            adRequest.Keywords.Add("���o�C���Q�[��");
        }
        else
        {
            adRequest.Keywords.Add("game");
            adRequest.Keywords.Add("mobile games");
        }

        bannerView.LoadAd(adRequest);
    }

    #region Banner callback handlers

    private void OnBannerAdLoaded()
    {
        Debug.Log("�o�i�[�\������");
        isRetrying = false; // ���������烊�g���C����
    }

    private void OnBannerAdLoadFailed(LoadAdError error)
    {
        Debug.LogWarning("�o�i�[�ǂݍ��ݎ��s: " + error);

        if (!isRetrying)
        {
            isRetrying = true;
            StartCoroutine(RetryLoadBanner());
        }
    }

    #endregion

    //���g���C����
    private IEnumerator RetryLoadBanner()
    {
        Debug.Log($"�o�i�[���g���C��{retryDelay}�b��Ɏ��s���܂�");
        yield return new WaitForSeconds(retryDelay);
        RequestBanner();
    }
}
