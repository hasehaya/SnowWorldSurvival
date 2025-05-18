using System;
using System.Linq;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;

public class GameManager :MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int baseUpgradePrice = 250;
    private float upgradeGrowthFactor = 1.35f;

    [SerializeField]
    private Canvas canvas;

    [Header("従業員関連")]
    [SerializeField, Tooltip("従業員のプレハブ")]
    private EmployeeController employeePrefab;

    [Header("ユーザーインターフェース")]
    [SerializeField, Tooltip("現在の所持金を表示するテキストフィールド。")]
    private TMP_Text moneyDisplay;

    [SerializeField, Tooltip("シーン遷移時のフェード用スクリプト。")]
    private ScreenFader screenFader;

    [Header("エフェクト")]
    [SerializeField, Tooltip("レストラン内で再生するBGM。")]
    private AudioClip backgroundMusic;

    // ゲーム進行時間（グローバルデータから取得）
    public float ElapsedTime => globalData?.TotalElapsedTime ?? 0f;

    public Canvas Canvas => canvas;

    public event Action OnUpgrade;

    private StageData stageData;
    private string stageID;
    private GlobalData globalData;
    public GlobalData GlobalData => globalData;
    private string globalDataID = "GlobalData";

    // アップグレード報酬用の一時保存変数
    private Upgrade.UpgradeType pendingUpgradeType;
    private MaterialType pendingMaterialType;

    void Awake()
    {
        // シングルトンの設定
        Instance = this;

        // 現在のシーン名をレストランのIDとして使用
        stageID = SceneManager.GetActiveScene().name;
        // 現在のシーンIDから数字のみ抽出（例："Stage1" -> 1）
        int currentStageNumber = int.Parse(new string(stageID.Where(char.IsDigit).ToArray()));

        // セーブデータの読み込み。存在しなければ初期状態で作成する
        stageData = SaveSystem.LoadData<StageData>(stageID);
        if (stageData == null)
            stageData = new StageData(currentStageNumber);

        globalData = SaveSystem.LoadData<GlobalData>(globalDataID);
        if (globalData == null)
            globalData = new GlobalData();

        // 現在のシーン番号がグローバルデータのシーン番号より大きければ更新する
        if (currentStageNumber > globalData.StageId)
        {
            globalData.StageId = currentStageNumber;
            SaveSystem.SaveData(globalData, globalDataID);
        }

        // UI上の所持金表示を初期化
        AdjustMoney(0);
    }

    void Start()
    {
        // 現在のシーンのビルドインデックスを保存する
        SaveSystem.SaveData<int>(SceneManager.GetActiveScene().buildIndex, "LastSceneIndex");

        // シーン開始時のフェードアウト処理
        screenFader.FadeOut();

        // UnlockManager の初期化（セーブデータとシーンIDを渡す）
        UnlockManager.Instance.InitializeUnlockManager(stageData, stageID);

        // 従業員の生成処理
        SpawnEmployee();

        // BGM の再生
        AudioManager.Instance.PlayBGM(backgroundMusic);
        
        // アップグレード報酬用のイベント購読
        AdMobReward.Instance.OnRewardReceived += OnUpgradeRewardReceived;
    }
    
    void OnDestroy()
    {
        if (AdMobReward.Instance != null)
        {
            AdMobReward.Instance.OnRewardReceived -= OnUpgradeRewardReceived;
        }
        
        DOTween.KillAll();
    }

    public void SpawnEmployee()
    {
        // シーン内の MaterialParent コンポーネントを全取得
        var materialManagerList = FindObjectsOfType<MaterialParent>(true);

        // 各 MaterialType ごとに従業員を生成する
        foreach (MaterialType materialType in Enum.GetValues(typeof(MaterialType)))
        {
            MaterialParent materialManager = null;
            foreach (var manager in materialManagerList)
            {
                if (manager.MaterialType == materialType)
                {
                    materialManager = manager;
                    break;
                }
            }
            // MaterialType が None または該当の管理オブジェクトがなければスキップ
            if (materialType == MaterialType.None || materialManager == null)
                continue;

            // 現在の従業員数を取得
            int currentCount = FindObjectsOfType<EmployeeController>()
                                .Count(e => e.MaterialType == materialType);

            // セーブデータ上の従業員数（アップグレードレベル）を取得
            int employeeAmount = stageData.FindUpgrade(Upgrade.UpgradeType.EmployeeAmount, materialType).Level;
            int toSpawn = employeeAmount - currentCount;
            if (toSpawn <= 0)
                continue;

            // レイアウト調整用に、固定の列数（例：5列）を設定
            int numberOfColumns = 5;
            for (int i = currentCount; i < employeeAmount; i++)
            {
                int columnIndex = i % numberOfColumns;
                int rowIndex = (i / numberOfColumns) + 1;

                // 指定列に対応するパトロールポイントを取得
                MaterialParent.PatrolPoints patrol = materialManager.GetPatrolPointsForColumn(columnIndex + 1);
                if (patrol == null)
                {
                    Debug.LogWarning("Column " + (columnIndex + 1) + " のパトロールポイントが取得できません。");
                    continue;
                }

                // パトロール用の一時オブジェクトを生成
                GameObject pointAObj = new GameObject($"PatrolPointA_Column{columnIndex + 1}_{materialType}");
                pointAObj.transform.position = patrol.pointA;
                pointAObj.transform.parent = transform;

                GameObject pointBObj = new GameObject($"PatrolPointB_Column{columnIndex + 1}_{materialType}");
                pointBObj.transform.position = patrol.pointB;
                pointBObj.transform.parent = transform;

                // 従業員の生成と初期化
                EmployeeController employee = Instantiate(employeePrefab, pointAObj.transform.position, Quaternion.identity);
                employee.SetPatrolPoints(pointAObj.transform, pointBObj.transform);
                employee.Column = columnIndex + 1;
                employee.Row = rowIndex;
                employee.MaterialType = materialType;
            }
        }
    }

    void Update()
    {
        // 経過時間を更新する（グローバルデータに格納）
        if (globalData != null)
        {
            globalData.TotalElapsedTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// 所持金を変更する
    /// </summary>
    public void AdjustMoney(int change)
    {
        stageData.Money += change;
        moneyDisplay.text = GetFormattedMoney(stageData.Money);
        
        // 変更があった場合は保存
        if (change != 0)
        {
            SaveSystem.SaveData<StageData>(stageData, stageID);
        }
    }

    /// <summary>
    /// 現在の所持金を取得する
    /// </summary>
    public long GetMoney() => stageData.Money;

    /// <summary>
    /// 所持金を読みやすい形式に整形する
    /// </summary>
    public string GetFormattedMoney(long money)
    {
        if (money < 1000)
            return money.ToString();
        else if (money < 1000000)
            return (money / 1000f).ToString("0.##") + "k";
        else if (money < 1000000000)
            return (money / 1000000f).ToString("0.##") + "m";
        else if (money < 1000000000000)
            return (money / 1000000000f).ToString("0.##") + "b";
        else
            return (money / 1000000000000f).ToString("0.##") + "t";
    }

    /// <summary>
    /// アップグレードを購入する
    /// </summary>
    public void PurchaseUpgrade(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        int price = GetUpgradePrice(upgradeType, materialType);
        AdjustMoney(-price);

        // アップグレードの実施
        stageData.UpgradeUpgrade(upgradeType, materialType);
        if (upgradeType == Upgrade.UpgradeType.EmployeeAmount)
        {
            SpawnEmployee();
        }

        AudioManager.Instance.PlaySFX(AudioID.Kaching);
        SaveSystem.SaveData<StageData>(stageData, stageID);
        OnUpgrade?.Invoke();
    }

    /// <summary>
    /// 広告を視聴してアップグレードを行うリクエスト
    /// </summary>
    public void RequestUpgradeByAd(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        pendingUpgradeType = upgradeType;
        pendingMaterialType = materialType;

        AdMobReward.Instance.ShowAdMobReward(RewardType.Upgrade);
    }

    /// <summary>
    /// アップグレード報酬を受け取ったときの処理
    /// </summary>
    private void OnUpgradeRewardReceived(RewardType rewardType)
    {
        if (rewardType != RewardType.Upgrade)
            return;

        PurchaseUpgradeByAd(pendingUpgradeType, pendingMaterialType);
    }

    /// <summary>
    /// 広告報酬によるアップグレード実施
    /// </summary>
    private void PurchaseUpgradeByAd(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        stageData.UpgradeUpgrade(upgradeType, materialType);
        if (upgradeType == Upgrade.UpgradeType.EmployeeAmount)
        {
            SpawnEmployee();
        }

        SaveSystem.SaveData<StageData>(stageData, stageID);
        OnUpgrade?.Invoke();
        
        // アップグレード効果音を再生
        AudioManager.Instance.PlaySFX(AudioID.Magical);
    }

    /// <summary>
    /// アップグレードの価格を計算する
    /// </summary>
    public int GetUpgradePrice(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        int currentLevel = GetUpgradeLevel(upgradeType, materialType);
        float levelPrice = baseUpgradePrice * Mathf.Pow(upgradeGrowthFactor, currentLevel);
        
        // ステージ番号を取得（10の位の数字）
        int stageNumber = (int)materialType / 10;
        // そのステージの最初のマテリアル（Wood）の値を取得
        int baseMaterialValue = stageNumber * 10 + 1; // Wood_Xの値
        
        float typePrice = levelPrice * MathF.Pow(2.35f, (int)materialType - baseMaterialValue);
        return Mathf.RoundToInt(Mathf.Round(typePrice) / 20f) * 20;
    }

    /// <summary>
    /// アップグレードのレベルを取得する
    /// </summary>
    public int GetUpgradeLevel(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        return stageData.FindUpgrade(upgradeType, materialType).Level;
    }

    /// <summary>
    /// 指定したレストランシーンを読み込む
    /// </summary>
    public void LoadRestaurant(int index)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (index == currentSceneIndex)
            return; // 同一シーンの再読み込みは行わない

        screenFader.FadeIn(() =>
        {
            SaveSystem.SaveData<StageData>(stageData, stageID);
            SaveSystem.SaveData(globalData, globalDataID);
            SceneManager.LoadScene(index);
        });
    }

    /// <summary>
    /// 広告ブロック機能を購入する
    /// </summary>
    public void PurchaseAdBlock()
    {
        globalData.IsAdRemoved = true;
        SaveSystem.SaveData(globalData, globalDataID);

        // 各種広告オブジェクトの削除処理
        AdMobBanner banner = FindObjectOfType<AdMobBanner>();
        if (banner != null)
        {
            banner.BannerDestroy();
        }

        AdMobInterstitial inter = FindObjectOfType<AdMobInterstitial>();
        if (inter != null)
        {
            inter.DestroyAd();
        }

        AdMobReward reward = FindObjectOfType<AdMobReward>();
        if (reward != null)
        {
            reward.DestroyAd();
        }

        AdMobRewardInterstitial rewardInter = FindObjectOfType<AdMobRewardInterstitial>();
        if (rewardInter != null)
        {
            rewardInter.DestroyAd();
        }

        AdMobOpen adMobOpen = FindObjectOfType<AdMobOpen>();
        if (adMobOpen != null)
        {
            adMobOpen.DestroyAd();
        }
        
        // AdBlockBtnオブジェクトを取得し、非表示にする
        AdBlockBtn adBlockBtn = FindObjectOfType<AdBlockBtn>();
        if (adBlockBtn != null)
        {
            adBlockBtn.ClosePopup();
            adBlockBtn.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ステージ選択画面に戻る
    /// </summary>
    public void ChangeSceneStageSelect()
    {
        if (stageData.NextStagePrompt)
        {
            stageData.NextStagePrompt = false;
            SaveSystem.SaveData<StageData>(stageData, stageID);
            StartCoroutine(InAppReviewManager.RequestReview());
        }
        
        screenFader.FadeIn(() =>
        {
            SaveSystem.SaveData(globalData, globalDataID);
            SceneManager.LoadScene("StageSelect");
            // ステージセレクト画面ではAdを表示しない
            AdMobReward.Instance.DestroyAd();
        });
    }

    /// <summary>
    /// 広告ブロック機能が有効かどうか
    /// </summary>
    public bool IsAdBlocked() => globalData.IsAdRemoved;

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveSystem.SaveData<StageData>(stageData, stageID);
            SaveSystem.SaveData(globalData, globalDataID);
        }
    }

    void OnApplicationQuit()
    {
        SaveSystem.SaveData<StageData>(stageData, stageID);
        SaveSystem.SaveData(globalData, globalDataID);
    }

    void OnDisable()
    {
        DOTween.KillAll();
    }

    /// <summary>
    /// EmployeeAmountの自動アップグレードを実施する
    /// </summary>
    public void AutoUpgradeEmployeeAmount(MaterialType materialType)
    {
        // アップグレードの実施
        stageData.UpgradeUpgrade(Upgrade.UpgradeType.EmployeeAmount, materialType);
        SpawnEmployee();
        
        SaveSystem.SaveData<StageData>(stageData, stageID);
        OnUpgrade?.Invoke();
    }
}

[Serializable]
public class Upgrade
{
    public enum UpgradeType
    {
        EmployeeSpeed,
        EmployeeCapacity,
        EmployeeAmount,
    }
    public UpgradeType upgradeType;
    public MaterialType MaterialType;
    public int Level;

    public Upgrade(UpgradeType type = UpgradeType.EmployeeSpeed, MaterialType materialType = MaterialType.None, int level = 0)
    {
        this.upgradeType = type;
        this.MaterialType = materialType;
        this.Level = level;
    }
}
