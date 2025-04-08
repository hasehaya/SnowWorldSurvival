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

    // ---- 追加ポイント: リロード関連 ----
    [Header("Retry Settings")]
    [Tooltip("失敗した時に何秒後に再リロードするか")]
    [SerializeField] private float reloadInterval = 5f;

    // ロード中フラグ：二重ロードを防止する
    private bool isLoading = false;

    /// <summary>
    /// 広告が有効かどうか判定
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
        // 広告削除済みならリクエストしない
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        // 広告SDKを初期化
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("GoogleMobileAds Initialized");
        });

        // アプリ起動時にロードを試みる
        LoadAppOpenAd(false);
    }

    /// <summary>
    /// OnApplicationPause でフォアグラウンドに復帰したタイミングを拾う
    /// </summary>
    private void OnApplicationPause(bool paused)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        // アプリがバック→フォアグラウンドに戻ったら広告を表示
        if (!paused)
        {
            if (IsAdAvailable)
            {
                // まだ4時間以内で有効なら即表示
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
    /// オープン広告の読み込み
    /// </summary>
    public void LoadAppOpenAd(bool showWhenLoaded)
    {
        // すでにロード中なら、重複リクエストを避けるためにreturn
        if (isLoading)
        {
            Debug.Log("LoadAppOpenAd: Already loading an ad. Skip.");
            return;
        }

        // 古い広告が残ってたら破棄＆イベント解除
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

        // リクエスト作成
        AdRequest adRequest = new AdRequest();

        // ロード実行
        AppOpenAd.Load(
            _adUnitId,
            adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                // コールバック戻ったらロード完了（成功 or 失敗）
                isLoading = false;
                bool isFailed = error != null || ad == null;
                if (isFailed && !showWhenLoaded)
                {
                    Debug.LogError("App open ad failed to load with error : " + error);

                    // ---- 失敗時に再リロード ----
                    Debug.Log($"Retry load after {reloadInterval} seconds...");
                    Invoke(nameof(LoadAppOpenAd), reloadInterval);

                    return;
                }

                Debug.Log("App open ad loaded successfully! ResponseInfo: " + ad.GetResponseInfo());
                // 有効期限を4時間先に設定
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
    /// オープン広告の表示
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
    /// イベントハンドラの登録
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
    /// イベントハンドラの解除
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

    // --- 以下、各イベントの処理 ---

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

        // 次回に備えて再ロード
        LoadAppOpenAd(false);
    }

    private void OnAdFullScreenContentFailed(AdError error)
    {
        Debug.LogError("App open ad failed to open full screen content with error : " + error);

        // 失敗でも再ロード
        LoadAppOpenAd(false);
    }
}
