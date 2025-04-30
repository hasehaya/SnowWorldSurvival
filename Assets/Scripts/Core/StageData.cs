using System;
using System.Collections.Generic;


[Serializable]
public class StageData
{
    public int StageID;
    public bool IsUnlocked;
    public long Money;
    public List<Upgrade> UpgradeList;
    public int PlayerSpeed;
    public int PlayerCapacity;
    public int Profit;

    public float ElapsedTime;

    public bool IsAdRemoved;
    
    // 次のステージへの誘導フラグ
    public bool NextStagePrompt;
    
    // 次のステージへの誘導が一度でも表示されたかを記録するフラグ
    public bool NextStagePromptShownOnce;

    // Dictionaries to track the number of unlockables purchased and the paid amount for each material.
    public Dictionary<MaterialType, int> UnlockCounts;
    public Dictionary<MaterialType, int> PaidAmounts;

    // For each material (except None), tracks whether that group's unlockables have been opened.
    public Dictionary<MaterialType, bool> MaterialUnlocked;

    public StageData(int stageId = 0)
    {
        this.StageID = stageId;
        IsUnlocked = false;
        Money = 200;
        PlayerSpeed = 1;
        PlayerCapacity = 1;
        Profit = 0;
        ElapsedTime = 0;
        NextStagePrompt = false;
        NextStagePromptShownOnce = false;
        InitUpgrades();
        InitUnlockData();
    }

    private void InitUpgrades()
    {
        UpgradeList = new List<Upgrade>();
        foreach (Upgrade.UpgradeType upgradeType in Enum.GetValues(typeof(Upgrade.UpgradeType)))
        {
            foreach (MaterialType materialType in Enum.GetValues(typeof(MaterialType)))
            {
                var upgrade = new Upgrade(upgradeType, materialType, 0);
                UpgradeList.Add(upgrade);
            }
        }
    }

    private void InitUnlockData()
    {
        UnlockCounts = new Dictionary<MaterialType, int>();
        PaidAmounts = new Dictionary<MaterialType, int>();
        MaterialUnlocked = new Dictionary<MaterialType, bool>();

        // For each MaterialType except None, initialize the values.
        foreach (MaterialType material in Enum.GetValues(typeof(MaterialType)))
        {
            if (material == MaterialType.None)
                continue;
            UnlockCounts[material] = 0;
            PaidAmounts[material] = 0;

            switch (StageID)
            {
                case 1:
                    MaterialUnlocked[material] = (material == MaterialType.Wood_1);
                    break;
                case 2:
                    MaterialUnlocked[material] = (material == MaterialType.Wood_2);
                    break;
                case 3:
                    MaterialUnlocked[material] = (material == MaterialType.Wood_3);
                    break;
                case 4:
                    MaterialUnlocked[material] = (material == MaterialType.Wood_4);
                    break;
                case 5:
                    MaterialUnlocked[material] = (material == MaterialType.Wood_5);
                    break;
                case 6:
                    MaterialUnlocked[material] = (material == MaterialType.Wood_6);
                    break;
                case 7:
                    MaterialUnlocked[material] = (material == MaterialType.Wood_7);
                    break;
                default:
                    MaterialUnlocked[material] = false;
                    break;
            }
        }
    }

    public void UpgradeUpgrade(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        foreach (var upgrade in UpgradeList)
        {
            if (upgrade.upgradeType == upgradeType && upgrade.MaterialType == materialType)
            {
                upgrade.Level++;
                break;
            }
        }
    }

    public Upgrade FindUpgrade(Upgrade.UpgradeType upgradeType, MaterialType materialType)
    {
        foreach (var upgrade in UpgradeList)
        {
            if (upgrade.upgradeType == upgradeType && upgrade.MaterialType == materialType)
            {
                return upgrade;
            }
        }
        return new Upgrade();
    }

    public float CalculateProgress()
    {
        int totalUnlocks = 0;
        int unlockedCount = 0;

        // Count individual unlocks
        foreach (var materialType in MaterialUnlocked.Keys)
        {
            if (materialType == MaterialType.None) continue;

            if (UnlockCounts.ContainsKey(materialType))
            {
                unlockedCount += UnlockCounts[materialType];
            }
        }

        // Calculate total unlocks based on stage material count × 16
        int materialCount = 0;
        switch (StageID)
        {
            case 1:
                materialCount = 2; // Wood_1, Rock_1
                break;
            case 2:
                materialCount = 3; // Wood_2, Flower_2, Rock_2
                break;
            case 3:
                materialCount = 4; // Wood_3, Rock_3, Snow_3, Tomato_3
                break;
            case 4:
                materialCount = 5; // Wood_4, Rock_4, Flower_4, Car_4, Pig_4
                break;
            case 5:
                materialCount = 6; // Wood_5, Snow_5, Rock_5, Juice_5, Skeleton_5, Wood2_5
                break;
            case 6:
                materialCount = 6; // Wood_6, Flower_6, MushRoom_6, Car_6, Rock_6, Wood2_6
                break;
            case 7:
                materialCount = 6; // Wood_7, Saboten_7, Pumpkin_7, Dumbbell_7, Flower_7, Wood2_7
                break;
            default:
                materialCount = 0;
                break;
        }
        totalUnlocks = materialCount * 16;

        return totalUnlocks > 0 ? (float)unlockedCount / totalUnlocks : 0f;
    }
}
