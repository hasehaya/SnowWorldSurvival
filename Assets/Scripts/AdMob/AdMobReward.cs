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
    MoneyCollection,
}

/// <summary>
/// リワード広告の表示と報酬イベントの発火を担当するクラス
/// </summary>
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
    
    /// <summary>
    /// 広告報酬受け取り時のイベント
    /// </summary>
    public event Action<RewardType> OnRewardReceived;

    //リワード広告用の変数
    private RewardedAd rewardedAd;//RewardedAd型の変数 rewardedAdを宣言 この中にリワード広告の情報が入る

    // 広告IDと報酬タイプ
    private string adUnitId;
    private RewardType rewardType;
    
    /// <summary>
    /// 広告がロード済みかどうかを確認するプロパティ
    /// </summary>
    public bool IsAdLoaded => rewardedAd != null && rewardedAd.CanShowAd();

    private void Start()
    {
        // 広告削除済みならリクエストしない
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        //AndroidとiOSで広告IDが違うのでプラットフォームで処理を分けます。
        // 参考
        //【Unity】AndroidとiOSで処理を分ける方法
        // https://marumaro7.hatenablog.com/entry/platformsyoriwakeru

#if UNITY_ANDROID
        adUnitId = "ca-app-pub-2788807416533951/3130480235";
#elif UNITY_IPHONE
        adUnitId = "ca-app-pub-2788807416533951/7766162572";
#else
        adUnitId = "unexpected_platform";
#endif

        //リワード 読み込み開始
        Debug.Log("Rewarded ad load start");

        LoadRewardedAd();//リワード広告読み込み
    }

    /// <summary>
    /// 広告を破棄
    /// </summary>
    public void DestroyAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }

    /// <summary>
    /// リワード広告を表示する
    /// </summary>
    public void ShowAdMobReward(RewardType rewardType)
    {
        // 広告削除済みなら直接報酬を付与
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            OnRewardReceived?.Invoke(rewardType);
            return;
        }
        
        // 広告の読み込みが完了していたら広告表示
        if (rewardedAd != null && rewardedAd.CanShowAd() == true)
        {
            this.rewardType = rewardType;
            rewardedAd.Show(GetReward);
        }
        else
        {
            // 広告読み込み未完了の場合はログ出力
            Debug.Log("Rewarded ad not loaded");
        }
    }

    /// <summary>
    /// 報酬受け取り処理
    /// </summary>
    private void GetReward(Reward reward)
    {
        // イベント発火のみを行い、実際の報酬処理はAdManagerに委譲
        OnRewardReceived?.Invoke(rewardType);
    }

    /// <summary>
    /// リワード広告を読み込む
    /// </summary>
    public void LoadRewardedAd()
    {
        // 広告の再読み込みのための処理
        if (rewardedAd != null)
        {
            // リワード広告は使い捨てなので一旦破棄
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        // リクエストを生成
        AdRequest request = new AdRequest();

        // 広告のキーワードを追加
        //===================================================================
        // アプリに関連するキーワードを文字列で設定するとアプリと広告の関連性が高まります。
        // 結果、収益が上がる可能性があります。
        // 任意設定のため不要であれば消していただいて問題はありません。

        // Application.systemLanguageでOSの言語判別　
        // 返り値はSystemLanguage.言語
        // 端末の言語が日本語の時
        if (Application.systemLanguage == SystemLanguage.Japanese)
        {
            request.Keywords.Add("ゲーム");
            request.Keywords.Add("モバイルゲーム");
        }

        //端末の言語が日本語以外の時
        else
        {
            request.Keywords.Add("game");
            request.Keywords.Add("mobile games");
        }
        //==================================================================

        // 広告をロード
        RewardedAd.Load(adUnitId, request, OnRewardedAdLoaded);
    }

    /// <summary>
    /// 広告ロード完了時のコールバック
    /// </summary>
    private void OnRewardedAdLoaded(RewardedAd ad, LoadAdError error)
    {
        // エラーがあるか広告情報がない場合
        if (error != null || ad == null)
        {
            // 読み込み失敗
            Debug.LogError("Failed to load reward ad : " + error);
            Invoke("LoadRewardedAd", 3f);
            return;
        }

        // 読み込み完了
        Debug.Log("Reward ad loaded");

        // 広告情報を保存
        rewardedAd = ad;

        // 広告の 表示・表示終了・表示失敗 の内容を登録
        RegisterEventHandlers(rewardedAd);
    }

    /// <summary>
    /// 広告イベントハンドラの登録
    /// </summary>
    private void RegisterEventHandlers(RewardedAd ad)
    {
        // リワード広告が表示された時
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };

        // リワード広告が表示終了 となった時
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");

            // リワード再読み込み
            LoadRewardedAd();
        };

        // リワード広告の表示失敗 となった時
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content with error : " + error);

            // リワード再読み込み
            LoadRewardedAd();
        };
    }
}
