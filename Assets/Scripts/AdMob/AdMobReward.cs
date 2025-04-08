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

    //やること
    //1.リワード広告IDの入力
    //2.GetReward関数に報酬内容を入力
    //3.リワード起動設定　ShowAdMobReward()を使う


    private RewardedAd rewardedAd;//RewardedAd型の変数 rewardedAdを宣言 この中にリワード広告の情報が入る

    private string adUnitId;
    private RewardType rewardType;

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

    public void DestroyAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }

    //リワード広告を表示する関数
    //ボタンに割付けして使用
    public void ShowAdMobReward(RewardType rewardType)
    {
        // 広告削除済みならリクエストしない
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            OnRewardReceived?.Invoke(rewardType);
            return;
        }
        //変数rewardedAdの中身が存在しており、広告の読み込みが完了していたら広告表示
        if (rewardedAd != null && rewardedAd.CanShowAd() == true)
        {
            this.rewardType = rewardType;
            rewardedAd.Show(GetReward);
        }
        else
        {
            //リワード広告読み込み未完了
            Debug.Log("Rewarded ad not loaded");
        }
    }

    //報酬受け取り処理
    private void GetReward(Reward reward)
    {
        OnRewardReceived?.Invoke(rewardType);
    }


    //リワード広告を読み込む関数 再読み込みにも使用
    public void LoadRewardedAd()
    {
        //広告の再読み込みのための処理
        //rewardedAdの中身が入っていた場合処理
        if (rewardedAd != null)
        {
            //リワード広告は使い捨てなので一旦破棄
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            return;
        }

        //リクエストを生成
        AdRequest request = new AdRequest();

        //広告のキーワードを追加
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

        //広告をロード  その後、関数OnRewardedAdLoadedを呼び出す
        RewardedAd.Load(adUnitId, request, OnRewardedAdLoaded);
    }


    // 広告のロードを実施した後に呼び出される関数
    private void OnRewardedAdLoaded(RewardedAd ad, LoadAdError error)
    {
        //変数errorに情報が入っている　または、変数adに情報がはいっていなかったら実行
        if (error != null || ad == null)
        {
            //リワード 読み込み失敗
            Debug.LogError("Failed to load reward ad : " + error);//error:エラー内容 
            Invoke("LoadRewardedAd", 3f);
            return;//この時点でこの関数の実行は終了
        }

        //リワード 読み込み完了
        Debug.Log("Reward ad loaded");

        //RewardedAd.Load(~略~)関数を実行することにより、RewardedAd型の変数adにRewardedAdのインスタンスを生成する。
        //生成したRewardedAd型のインスタンスを変数rewardedAdへ割り当て
        rewardedAd = ad;

        //広告の 表示・表示終了・表示失敗 の内容を登録
        RegisterEventHandlers(rewardedAd);
    }


    //広告の 表示・表示終了・表示失敗 の内容
    private void RegisterEventHandlers(RewardedAd ad)
    {
        //リワード広告が表示された時に起動する内容
        ad.OnAdFullScreenContentOpened += () =>
        {
            //リワード広告 表示
            Debug.Log("Rewarded ad full screen content opened.");
        };

        //リワード広告が表示終了 となった時に起動する内容
        ad.OnAdFullScreenContentClosed += () =>
        {
            //リワード広告 表示終了
            Debug.Log("Rewarded ad full screen content closed.");

            //リワード 再読み込み
            LoadRewardedAd();
        };

        //リワード広告の表示失敗 となった時に起動する内容
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            //エラー表示
            Debug.LogError("Rewarded ad failed to open full screen content with error : " + error);

            //リワード 再読み込み
            LoadRewardedAd();
        };
    }
}
