using System;
using System.Collections.Generic;

namespace CryingSnow.FastFoodRush
{
    [Serializable]
    public class SaveData
    {
        public string RestaurantID { get; set; }
        public long Money { get; set; }
        public List<Upgrade> UpgradeList { get; set; }
        public int PlayerSpeed { get; set; }
        public int PlayerCapacity { get; set; }
        public int Profit { get; set; }

        // Dictionaries to track the number of unlockables purchased and the paid amount for each material.
        public Dictionary<MaterialType, int> UnlockCounts { get; set; }
        public Dictionary<MaterialType, int> PaidAmounts { get; set; }

        // For each material (except None), tracks whether that group's unlockables have been opened.
        public Dictionary<MaterialType, bool> MaterialUnlocked { get; set; }

        public SaveData(string restaurantID, long money)
        {
            RestaurantID = restaurantID;
            Money = money;
            PlayerSpeed = 1;
            PlayerCapacity = 1;
            Profit = 0;
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
    }
}
