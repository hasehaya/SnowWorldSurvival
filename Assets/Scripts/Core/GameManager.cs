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
    [SerializeField, Tooltip("・ｽ]・ｽﾆ茨ｿｽ・ｽv・ｽ・ｽ・ｽn・ｽu")]
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
    public GlobalData GlobalData => globalData;
    private string globalDataID = "GlobalData";

    void Awake()
    {
        // ・ｽV・ｽ・ｽ・ｽO・ｽ・ｽ・ｽg・ｽ・ｽ・ｽﾌ設抵ｿｽ
        Instance = this;

        // ・ｽ・ｽ・ｽﾝのシ・ｽ[・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽX・ｽg・ｽ・ｽ・ｽ・ｽID・ｽﾆゑｿｽ・ｽﾄ暦ｿｽ・ｽp
        stageID = SceneManager.GetActiveScene().name;

        // ・ｽZ・ｽ[・ｽu・ｽf・ｽ[・ｽ^・ｽﾌ・ｿｽ・ｽ[・ｽh・ｽi・ｽ・ｽ・ｽﾝゑｿｽ・ｽﾈゑｿｽ・ｽ鼾・ｿｽﾍ擾ｿｽ・ｽ・ｽ・ｽ・ｽﾔで作成・ｽj
        stageData = SaveSystem.LoadData<StageData>(stageID);
        if (stageData == null)
            stageData = new StageData(stageID, startingMoney);

        globalData = SaveSystem.LoadData<GlobalData>(globalDataID);
        if (globalData == null)
            globalData = new GlobalData();

        // Extract number from current stageID (e.g. "Stage1" -> 1)
        int currentStageNumber = int.Parse(new string(stageID.Where(char.IsDigit).ToArray()));

        // Only update if current stage number is greater
        if (currentStageNumber > globalData.StageId)
        {
            globalData.StageId = currentStageNumber;
            SaveSystem.SaveData(globalData, globalDataID);
        }

        // UI\ﾌめの擾ｿｽ・ｽ・ｽ・ｽﾊ貨設抵ｿｽ
        AdjustMoney(0);
    }

    void Start()
    {
        // ・ｽ・ｽ・ｽﾝのシ・ｽ[・ｽ・ｽ・ｽC・ｽ・ｽ・ｽf・ｽb・ｽN・ｽX・ｽ・ｽﾛ托ｿｽ
        SaveSystem.SaveData<int>(SceneManager.GetActiveScene().buildIndex, "LastSceneIndex");

        // ・ｽV・ｽ[・ｽ・ｽ・ｽJ・ｽn・ｽ・ｽ・ｽﾌフ・ｽF・ｽ[・ｽh・ｽA・ｽE・ｽg・ｽ・ｽ・ｽ・ｽ
        screenFader.FadeOut();

        // UnlockManager ・ｽﾌ擾ｿｽ・ｽ・ｽ・ｽ・ｽ・ｽi・ｽZ・ｽ[・ｽu・ｽf・ｽ[・ｽ^・ｽﾆ・ｿｽ・ｽX・ｽg・ｽ・ｽ・ｽ・ｽID・ｽ・ｽn・ｽ・ｽ・ｽj
        UnlockManager.Instance.InitializeUnlockManager(stageData, stageID);

        // ・ｽ]・ｽﾆ茨ｿｽ・ｽﾌス・ｽ|・ｽ[・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ
        SpawnEmployee();

        // BGM ・ｽﾌ再撰ｿｽ
        AudioManager.Instance.PlayBGM(backgroundMusic);
    }

    void SpawnEmployee()
    {
        var materialManagerList = FindObjectsOfType<MaterialParent>(true);
        // MaterialType ・ｽﾌ全・ｽ・ｽ
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
            // None ・ｽ^・ｽC・ｽv・ｽﾜゑｿｽ・ｽﾍ該・ｽ・ｽ・ｽ・ｽ・ｽ・ｽﾇ暦ｿｽ・ｽI・ｽu・ｽW・ｽF・ｽN・ｽg・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽﾎス・ｽL・ｽb・ｽv
            if (materialType == MaterialType.None || materialManager == null)
                continue;

            // ・ｽ・ｽ・ｽﾝの従・ｽﾆ茨ｿｽ・ｽ・ｽ・ｽ・ｽ・ｽ謫ｾ
            int currentCount = FindObjectsOfType<EmployeeController>()
                                .Count(e => e.MaterialType == materialType);
            // ・ｽZ・ｽ[・ｽu・ｽf・ｽ[・ｽ^・ｽ・ｽ・ｽ・ｽ]・ｽﾆ茨ｿｽ・ｽ・ｽ・ｽﾌア・ｽb・ｽv・ｽO・ｽ・ｽ・ｽ[・ｽh・ｽ・ｽ・ｽx・ｽ・ｽ・ｽ・ｽ・ｽ謫ｾ
            int employeeAmount = stageData.FindUpgrade(Upgrade.UpgradeType.EmployeeAmount, materialType).Level;
            int toSpawn = employeeAmount - currentCount;
            if (toSpawn <= 0)
                continue;

            // ・ｽ・ｽ・ｽC・ｽA・ｽE・ｽg・ｽp・ｽﾉ固抵ｿｽﾌ列数ゑｿｽ・ｽw・ｽ・ｽ
            int numberOfColumns = 5;
            for (int i = currentCount; i < employeeAmount; i++)
            {
                int columnIndex = i % numberOfColumns;
                int rowIndex = (i / numberOfColumns) + 1;

                // ・ｽw・ｽ・ｽﾌ暦ｿｽﾉ対会ｿｽ・ｽ・ｽ・ｽ・ｽp・ｽg・ｽ・ｽ・ｽ[・ｽ・ｽ・ｽn・ｽ_・ｽ・ｽ・ｽ謫ｾ
                MaterialParent.PatrolPoints patrol = materialManager.GetPatrolPointsForColumn(columnIndex + 1);
                if (patrol == null)
                {
                    Debug.LogWarning("Column " + (columnIndex + 1) + " ・ｽﾌパ・ｽg・ｽ・ｽ・ｽ[・ｽ・ｽ・ｽn・ｽ_・ｽ・ｽ・ｽ謫ｾ・ｽﾅゑｿｽ・ｽﾜゑｿｽ・ｽ・ｽB");
                    continue;
                }

                // ・ｽp・ｽg・ｽ・ｽ・ｽ[・ｽ・ｽ・ｽn・ｽ_・ｽp・ｽﾌ一時・ｽI・ｽu・ｽW・ｽF・ｽN・ｽg・ｽｶ撰ｿｽ
                GameObject pointAObj = new GameObject($"PatrolPointA_Column{columnIndex + 1}_{materialType}");
                pointAObj.transform.position = patrol.pointA;
                pointAObj.transform.parent = transform;

                GameObject pointBObj = new GameObject($"PatrolPointB_Column{columnIndex + 1}_{materialType}");
                pointBObj.transform.position = patrol.pointB;
                pointBObj.transform.parent = transform;

                // ・ｽ]・ｽﾆ茨ｿｽ・ｽﾌ撰ｿｽ・ｽ・ｽ・ｽﾆ擾ｿｽ・ｽ・ｽ・ｽ・ｽ
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
            return; // ・ｽ・ｽ・ｽﾝのシ・ｽ[・ｽ・ｽ・ｽﾌ再読み搾ｿｽ・ｽﾝゑｿｽh・ｽ~

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
