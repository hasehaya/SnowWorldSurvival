using System;
using System.Collections.Generic;

namespace CryingSnow.FastFoodRush
{
    [System.Serializable]
    public class SaveData
    {
        public string RestaurantID { get; set; }

        public long Money { get; set; }

        public List<Upgrade> UpgradeList { get; set; }
        public int PlayerSpeed { get; set; }
        public int PlayerCapacity { get; set; }
        public int Profit { get; set; }

        public int UnlockCount { get; set; }  // The number of unlockables that have already been purchased.
        public int PaidAmount { get; set; } // The amount of money paid towards the most recent unlockable.
        public bool IsUnlocked { get; set; } // Whether the restaurant is unlocked. This is determined by whether the previous restaurant has been unlocked.

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

        private void InitUpgrade()
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

        public SaveData(string restaurantID, long money)
        {
            RestaurantID = restaurantID;
            Money = money;
            PlayerSpeed = 1;
            PlayerCapacity = 1;
            Profit = 0;
            UnlockCount = 0;
            PaidAmount = 0;
            IsUnlocked = false;
            InitUpgrade();
        }
    }
}
