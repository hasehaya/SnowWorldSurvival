using System.Collections;

using GoogleMobileAds.Api;

using UnityEngine;

public class AdMobBanner :MonoBehaviour
{
    private BannerView bannerView;
    private bool isRetrying = false; // リトライ中かどうか判定
    private float retryDelay = 3f;   // リトライ間隔（秒）

    private void Start()
    {
        // 広告削除済みならリクエストしない
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }
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
        string adUnitId = "ca-app-pub-2788807416533951/6867512863";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-2788807416533951/9079244246";
#else
        string adUnitId = "unexpected_platform";
#endif

        // 古いバナーが残ってたら削除
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        var bannerPosY = Screen.safeArea.yMax - Screen.height;
        bannerView = new BannerView(adUnitId, adaptiveSize, 0, (int)bannerPosY);

        // コールバック設定
        bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
        bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;

        // リクエスト作成
        AdRequest adRequest = new AdRequest();
        if (Application.systemLanguage == SystemLanguage.Japanese)
        {
            adRequest.Keywords.Add("ゲーム");
            adRequest.Keywords.Add("モバイルゲーム");
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
        Debug.Log("バナー表示完了");
        isRetrying = false; // 成功したらリトライ解除
    }

    private void OnBannerAdLoadFailed(LoadAdError error)
    {
        Debug.LogWarning("バナー読み込み失敗: " + error);

        if (!isRetrying)
        {
            isRetrying = true;
            StartCoroutine(RetryLoadBanner());
        }
    }

    #endregion

    //リトライ処理
    private IEnumerator RetryLoadBanner()
    {
        Debug.Log($"バナーリトライを{retryDelay}秒後に試行します");
        yield return new WaitForSeconds(retryDelay);
        RequestBanner();
    }
}
