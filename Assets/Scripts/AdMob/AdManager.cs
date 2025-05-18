using UnityEngine;

/// <summary>
/// 広告報酬の処理を一元管理するクラス
/// </summary>
public class AdManager : MonoBehaviour
{
    private static AdManager instance;
    public static AdManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AdManager");
                    instance = obj.AddComponent<AdManager>();
                }
            }
            return instance;
        }
    }
    [HideInInspector] public float CoolTime = 0f;

    // 効果の持続時間（180秒＝3分）
    private const float EFFECT_DURATION = 180f;
    
    // 金額報酬の場合の最低獲得金額
    [SerializeField] private int minMoneyReward = 1000;

    private void Start()
    {
        AdMobReward.Instance.OnRewardReceived += OnRewardReceived;
    }

    private void OnDestroy()
    {
        if (AdMobReward.Instance != null)
        {
            AdMobReward.Instance.OnRewardReceived -= OnRewardReceived;
        }
    }

    private void Update()
    {
        if (CoolTime > 0f)
        {
            CoolTime -= Time.deltaTime;
        }
        else
        {
            CoolTime = 0f;
        }
    }

    /// <summary>
    /// 広告報酬の受け取り処理（一元管理）
    /// </summary>
    private void OnRewardReceived(RewardType type)
    {
        // 広告削除済みの場合はクールタイムを設定
        if (GameManager.Instance.IsAdBlocked())
        {
            CoolTime = 60f;
        }
        else
        {
            CoolTime = 0f;
        }

        // 報酬タイプに応じた処理
        switch (type)
        {
            case RewardType.Money:
                GiveMoneyReward();
                break;
            case RewardType.PlayerSpeed:
                ActivatePlayerSpeedEffect();
                break;
            case RewardType.PlayerCapacity:
                ActivatePlayerCapacityEffect();
                break;
            case RewardType.MoneyCollection:
                ActivateMoneyCollectionEffect();
                break;
            case RewardType.Upgrade:
                // Upgrade報酬はGameManagerで処理されるため何もしない
                // ここではUIの更新は行わない、GameManagerのOnUpgradeイベントが発火するため
                break;
            default:
                Debug.LogWarning($"未実装の報酬タイプ: {type}");
                break;
        }
    }

    /// <summary>
    /// お金の報酬を付与する（所持金を2倍にする）
    /// </summary>
    private void GiveMoneyReward()
    {
        if (GameManager.Instance != null)
        {
            // 現在の所持金額を取得
            long currentMoney = GameManager.Instance.GetMoney();
            
            // 所持金を2倍にする（ただし最低でもminMoneyReward以上を追加）
            int rewardAmount = Mathf.Max((int)currentMoney, minMoneyReward);
            
            // 所持金に追加
            GameManager.Instance.AdjustMoney(rewardAmount);
            
            Debug.Log($"所持金が2倍になりました！ +{rewardAmount}円！");
        }
    }

    /// <summary>
    /// プレイヤー速度向上効果を有効化
    /// </summary>
    private void ActivatePlayerSpeedEffect()
    {
        if (GameManager.Instance != null && GameManager.Instance.GlobalData != null)
        {
            if (!GameManager.Instance.GlobalData.IsPlayerSpeedActive)
            {
                GameManager.Instance.GlobalData.PlayerSpeedRemainingSeconds = EFFECT_DURATION;
                Debug.Log("プレイヤー速度向上効果が発動しました！");
            }
            else
            {
                // 既に効果がある場合は時間を延長
                GameManager.Instance.GlobalData.PlayerSpeedRemainingSeconds += EFFECT_DURATION;
                Debug.Log("プレイヤー速度向上効果の時間が延長されました！");
            }
        }
    }

    /// <summary>
    /// プレイヤー容量向上効果を有効化
    /// </summary>
    private void ActivatePlayerCapacityEffect()
    {
        if (GameManager.Instance != null && GameManager.Instance.GlobalData != null)
        {
            if (!GameManager.Instance.GlobalData.IsPlayerCapacityActive)
            {
                GameManager.Instance.GlobalData.PlayerCapacityRemainingSeconds = EFFECT_DURATION;
                Debug.Log("プレイヤー容量向上効果が発動しました！");
            }
            else
            {
                // 既に効果がある場合は時間を延長
                GameManager.Instance.GlobalData.PlayerCapacityRemainingSeconds += EFFECT_DURATION;
                Debug.Log("プレイヤー容量向上効果の時間が延長されました！");
            }
        }
    }

    /// <summary>
    /// お金自動回収効果を有効化
    /// </summary>
    private void ActivateMoneyCollectionEffect()
    {
        if (GameManager.Instance != null && GameManager.Instance.GlobalData != null)
        {
            if (!GameManager.Instance.GlobalData.IsMoneyCollectionActive)
            {
                GameManager.Instance.GlobalData.MoneyCollectionRemainingSeconds = EFFECT_DURATION;
                Debug.Log("お金自動回収効果が発動しました！");
            }
            else
            {
                // 既に効果がある場合は時間を延長
                GameManager.Instance.GlobalData.MoneyCollectionRemainingSeconds += EFFECT_DURATION;
                Debug.Log("お金自動回収効果の時間が延長されました！");
            }
        }
    }
}
