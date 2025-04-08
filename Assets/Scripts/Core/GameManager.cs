using System;
using System.Linq;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager :MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Tooltip("Base cost of upgrading the restaurant.")]
    private int baseUpgradePrice = 250;

    [SerializeField, Range(1.01f, 1.99f), Tooltip("The growth factor applied to upgrade prices with each upgrade.")]
    private float upgradeGrowthFactor = 1.5f;

    private long startingMoney = 200;

    [SerializeField]
    private Canvas canvas;

    [Header("Employee")]
    [SerializeField, Tooltip("従業員プレハブ")]
    private EmployeeController employeePrefab;

    [Header("User Interface")]
    [SerializeField, Tooltip("Text field displaying the current money.")]
    private TMP_Text moneyDisplay;

    [SerializeField, Tooltip("Screen fader for transitions between scenes.")]
    private ScreenFader screenFader;

    [Header("Effects")]
    [SerializeField, Tooltip("Background music to play in the restaurant.")]
    private AudioClip backgroundMusic;

    public float ElapsedTime => stageData?.ElapsedTime ?? 0f;

    public Canvas Canvas => canvas;

    public event Action OnUpgrade;

    private StageData stageData;
    private string stageID;
    private GlobalData globalData;
    private string globalDataID = "GlobalData";

    void Awake()
    {
        // シングルトンの設定
        Instance = this;

        // 現在のシーン名をレストランIDとして利用
        stageID = SceneManager.GetActiveScene().name;

        // セーブデータのロード（存在しない場合は初期状態で作成）
        stageData = SaveSystem.LoadData<StageData>(stageID);
        if (stageData == null)
            stageData = new StageData(stageID, startingMoney);

        globalData = SaveSystem.LoadData<GlobalData>(globalDataID);
        if (globalData == null)
            globalData = new GlobalData();

        // UI表示のための初期通貨設定
        AdjustMoney(0);
    }

    void Start()
    {
        // 現在のシーンインデックスを保存
        SaveSystem.SaveData<int>(SceneManager.GetActiveScene().buildIndex, "LastSceneIndex");

        // シーン開始時のフェードアウト処理
        screenFader.FadeOut();

        // UnlockManager の初期化（セーブデータとレストランIDを渡す）
        UnlockManager.Instance.InitializeUnlockManager(stageData, stageID);

        // 従業員のスポーン処理
        SpawnEmployee();

        // BGM の再生
        AudioManager.Instance.PlayBGM(backgroundMusic);
    }

    void SpawnEmployee()
    {
        var materialManagerList = FindObjectsOfType<MaterialParent>(true);
        // MaterialType の全列挙
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
            // None タイプまたは該当する管理オブジェクトが無ければスキップ
            if (materialType == MaterialType.None || materialManager == null)
                continue;

            // 現在の従業員数を取得
            int currentCount = FindObjectsOfType<EmployeeController>()
                                .Count(e => e.MaterialType == materialType);
            // セーブデータから従業員数のアップグレードレベルを取得
            int employeeAmount = stageData.FindUpgrade(Upgrade.UpgradeType.EmployeeAmount, materialType).Level;
            int toSpawn = employeeAmount - currentCount;
            if (toSpawn <= 0)
                continue;

            // レイアウト用に固定の列数を指定
            int numberOfColumns = 5;
            for (int i = currentCount; i < employeeAmount; i++)
            {
                int columnIndex = i % numberOfColumns;
                int rowIndex = (i / numberOfColumns) + 1;

                // 指定の列に対応するパトロール地点を取得
                MaterialParent.PatrolPoints patrol = materialManager.GetPatrolPointsForColumn(columnIndex + 1);
                if (patrol == null)
                {
                    Debug.LogWarning("Column " + (columnIndex + 1) + " のパトロール地点が取得できません。");
                    continue;
                }

                // パトロール地点用の一時オブジェクトを生成
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
        if (stageData != null)
        {
            stageData.ElapsedTime += Time.unscaledDeltaTime;
        }
    }

    public float GetStackOffset(MaterialType materialType) => materialType switch
    {
        MaterialType.Log => 0.3f,
        MaterialType.Rock => 0.3f,
        MaterialType.Snow => 0.3f,
        MaterialType.Tomato => 0.3f,
        MaterialType.None => 0f,
        _ => 0f
    };

    public void AdjustMoney(int change)
    {
        stageData.Money += change;
        moneyDisplay.text = GetFormattedMoney(stageData.Money);
    }

    public long GetMoney() => stageData.Money;

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

    public void PurchaseUpgrade(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        int price = GetUpgradePrice(upgradeType, materialType);
        AdjustMoney(-price);

        stageData.UpgradeUpgrade(upgradeType, materialType);
        if (upgradeType == Upgrade.UpgradeType.EmployeeAmount)
        {
            SpawnEmployee();
        }

        AudioManager.Instance.PlaySFX(AudioID.Kaching);
        SaveSystem.SaveData<StageData>(stageData, stageID);
        OnUpgrade?.Invoke();
    }

    public void RequestUpgradeByAd(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        AdMobReward.Instance.OnRewardReceived += OnAdRewardReceived;

        pendingUpgradeType = upgradeType;
        pendingMaterialType = materialType;

        AdMobReward.Instance.ShowAdMobReward(RewardType.Upgrade);
    }

    private Upgrade.UpgradeType pendingUpgradeType;
    private MaterialType pendingMaterialType;

    private void OnAdRewardReceived(RewardType rewardType)
    {
        if (rewardType != RewardType.Upgrade)
            return;
        PurchaseUpgradeByAd(pendingUpgradeType, pendingMaterialType);

        AdMobReward.Instance.OnRewardReceived -= OnAdRewardReceived;
    }

    private void PurchaseUpgradeByAd(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        stageData.UpgradeUpgrade(upgradeType, materialType);
        if (upgradeType == Upgrade.UpgradeType.EmployeeAmount)
        {
            SpawnEmployee();
        }

        SaveSystem.SaveData<StageData>(stageData, stageID);
        OnUpgrade?.Invoke();
    }

    public int GetUpgradePrice(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        int currentLevel = GetUpgradeLevel(upgradeType, materialType);
        float levelPrice = baseUpgradePrice * Mathf.Pow(upgradeGrowthFactor, currentLevel);
        float typePrice = levelPrice * MathF.Pow(2, (int)materialType - (int)MaterialType.Log);
        return Mathf.RoundToInt(Mathf.Round(typePrice) / 50f) * 50;
    }

    public int GetUpgradeLevel(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        return stageData.FindUpgrade(upgradeType, materialType).Level;
    }

    public void LoadRestaurant(int index)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (index == currentSceneIndex)
            return; // 現在のシーンの再読み込みを防止

        screenFader.FadeIn(() =>
        {
            SaveSystem.SaveData<StageData>(stageData, stageID);
            SceneManager.LoadScene(index);
        });
    }

    public void PurchaseAdBlock()
    {
        globalData.IsAdRemoved = true;
        SaveSystem.SaveData(globalData, globalDataID);

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
    }

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
}

[Serializable]
public class Upgrade
{
    public enum UpgradeType
    {
        EmployeeSpeed, EmployeeCapacity, EmployeeAmount,
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
