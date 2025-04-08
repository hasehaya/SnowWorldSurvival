using System;

using GoogleMobileAds.Api;

using UnityEngine;

[AddComponentMenu("GoogleMobileAds/Samples/AppOpenAdController")]
public class AdMobOpen :MonoBehaviour
{
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-2788807416533951/5520640162";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-2788807416533951/9467823722";
#else
    private string _adUnitId = "unused";
#endif

    private AppOpenAd appOpenAd;
    private DateTime _expireTime;

    // ---- �ǉ��|�C���g: �����[�h�֘A ----
    [Header("Retry Settings")]
    [Tooltip("���s�������ɉ��b��ɍă����[�h���邩")]
    [SerializeField] private float reloadInterval = 5f;

    // ���[�h���t���O�F��d���[�h��h�~����
    private bool isLoading = false;

    /// <summary>
    /// �L�����L�����ǂ�������
    /// </summary>
    public bool IsAdAvailable
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
            {
                return false;
            }
            return appOpenAd != null
                   && appOpenAd.CanShowAd()
                   && DateTime.Now < _expireTime;
        }
    }

    private void Start()
    {
        // �L���폜�ς݂Ȃ烊�N�G�X�g���Ȃ�
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        // �L��SDK��������
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("GoogleMobileAds Initialized");
        });

        // �A�v���N�����Ƀ��[�h�����݂�
        LoadAppOpenAd(false);
    }

    /// <summary>
    /// OnApplicationPause �Ńt�H�A�O���E���h�ɕ��A�����^�C�~���O���E��
    /// </summary>
    private void OnApplicationPause(bool paused)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        // �A�v�����o�b�N���t�H�A�O���E���h�ɖ߂�����L����\��
        if (!paused)
        {
            if (IsAdAvailable)
            {
                // �܂�4���Ԉȓ��ŗL���Ȃ瑦�\��
                ShowAppOpenAd();
            }
            else
            {
                LoadAppOpenAd(true);
            }
        }
    }

    public void DestroyAd()
    {
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }
    }

    /// <summary>
    /// �I�[�v���L���̓ǂݍ���
    /// </summary>
    public void LoadAppOpenAd(bool showWhenLoaded)
    {
        // ���łɃ��[�h���Ȃ�A�d�����N�G�X�g������邽�߂�return
        if (isLoading)
        {
            Debug.Log("LoadAppOpenAd: Already loading an ad. Skip.");
            return;
        }

        // �Â��L�����c���Ă���j�����C�x���g����
        if (appOpenAd != null)
        {
            UnregisterEventHandlers(appOpenAd);
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        Debug.Log("Loading the app open ad...");
        isLoading = true;

        // ���N�G�X�g�쐬
        AdRequest adRequest = new AdRequest();

        // ���[�h���s
        AppOpenAd.Load(
            _adUnitId,
            adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                // �R�[���o�b�N�߂����烍�[�h�����i���� or ���s�j
                isLoading = false;
                bool isFailed = error != null || ad == null;
                if (isFailed && !showWhenLoaded)
                {
                    Debug.LogError("App open ad failed to load with error : " + error);

                    // ---- ���s���ɍă����[�h ----
                    Debug.Log($"Retry load after {reloadInterval} seconds...");
                    Invoke(nameof(LoadAppOpenAd), reloadInterval);

                    return;
                }

                Debug.Log("App open ad loaded successfully! ResponseInfo: " + ad.GetResponseInfo());
                // �L��������4���Ԑ�ɐݒ�
                _expireTime = DateTime.Now + TimeSpan.FromHours(4);
                appOpenAd = ad;
                RegisterEventHandlers(appOpenAd);

                if (showWhenLoaded && IsAdAvailable)
                {
                    ShowAppOpenAd();
                }
            }
        );
    }

    /// <summary>
    /// �I�[�v���L���̕\��
    /// </summary>
    public void ShowAppOpenAd()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        if (IsAdAvailable)
        {
            Debug.Log("Showing app open ad.");
            appOpenAd.Show();
        }
        else
        {
            Debug.LogWarning("App open ad is not ready or expired.");
        }
    }

    /// <summary>
    /// �C�x���g�n���h���̓o�^
    /// </summary>
    private void RegisterEventHandlers(AppOpenAd ad)
    {
        ad.OnAdPaid += OnAdPaid;
        ad.OnAdImpressionRecorded += OnAdImpressionRecorded;
        ad.OnAdClicked += OnAdClicked;
        ad.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
        ad.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
        ad.OnAdFullScreenContentFailed += OnAdFullScreenContentFailed;
    }

    /// <summary>
    /// �C�x���g�n���h���̉���
    /// </summary>
    private void UnregisterEventHandlers(AppOpenAd ad)
    {
        ad.OnAdPaid -= OnAdPaid;
        ad.OnAdImpressionRecorded -= OnAdImpressionRecorded;
        ad.OnAdClicked -= OnAdClicked;
        ad.OnAdFullScreenContentOpened -= OnAdFullScreenContentOpened;
        ad.OnAdFullScreenContentClosed -= OnAdFullScreenContentClosed;
        ad.OnAdFullScreenContentFailed -= OnAdFullScreenContentFailed;
    }

    private void OnDestroy()
    {
        if (appOpenAd != null)
        {
            UnregisterEventHandlers(appOpenAd);
            appOpenAd.Destroy();
        }
    }

    // --- �ȉ��A�e�C�x���g�̏��� ---

    private void OnAdPaid(AdValue adValue)
    {
        Debug.Log($"App open ad paid {adValue.Value} {adValue.CurrencyCode}");
    }

    private void OnAdImpressionRecorded()
    {
        Debug.Log("App open ad recorded an impression.");
    }

    private void OnAdClicked()
    {
        Debug.Log("App open ad was clicked.");
    }

    private void OnAdFullScreenContentOpened()
    {
        Debug.Log("App open ad full screen content opened.");
    }

    private void OnAdFullScreenContentClosed()
    {
        Debug.Log("App open ad full screen content closed.");

        // ����ɔ����čă��[�h
        LoadAppOpenAd(false);
    }

    private void OnAdFullScreenContentFailed(AdError error)
    {
        Debug.LogError("App open ad failed to open full screen content with error : " + error);

        // ���s�ł��ă��[�h
        LoadAppOpenAd(false);
    }
}
