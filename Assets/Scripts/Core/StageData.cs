﻿using System;
using System.Collections.Generic;


[Serializable]
public class StageData
{
    public string StageID;
    public bool IsUnlocked;
    public long Money;
    public List<Upgrade> UpgradeList;
    public int PlayerSpeed;
    public int PlayerCapacity;
    public int Profit;

    public float ElapsedTime;

    public bool IsAdRemoved;

    // Dictionaries to track the number of unlockables purchased and the paid amount for each material.
    public Dictionary<MaterialType, int> UnlockCounts;
    public Dictionary<MaterialType, int> PaidAmounts;

    // For each material (except None), tracks whether that group's unlockables have been opened.
    public Dictionary<MaterialType, bool> MaterialUnlocked;

    public StageData(string StageID, long money)
    {
        this.StageID = StageID;
        IsUnlocked = false;
        Money = money;
        PlayerSpeed = 1;
        PlayerCapacity = 1;
        Profit = 0;
        ElapsedTime = 0;
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
            // Only the first group (assumed here to be Log) is initially unlocked.
            MaterialUnlocked[material] = (material == MaterialType.Log);
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

        // Count total unlocks and unlocked items
        foreach (var materialType in MaterialUnlocked.Keys)
        {
            if (materialType == MaterialType.None) continue;

            // Count material group unlocks
            totalUnlocks++;
            if (MaterialUnlocked[materialType])
            {
                unlockedCount++;
            }

            // Count individual unlocks
            if (UnlockCounts.ContainsKey(materialType))
            {
                totalUnlocks += UnlockCounts[materialType];
                unlockedCount += UnlockCounts[materialType];
            }
        }

        return totalUnlocks > 0 ? (float)unlockedCount / totalUnlocks : 0f;
    }
}
