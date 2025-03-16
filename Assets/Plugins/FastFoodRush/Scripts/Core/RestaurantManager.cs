using System;
using System.Collections.Generic;
using System.Linq;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CryingSnow.FastFoodRush
{
    public class RestaurantManager :MonoBehaviour
    {
        public static RestaurantManager Instance { get; private set; }

        [SerializeField, Tooltip("Base cost of upgrading the restaurant.")]
        private int baseUpgradePrice = 250;

        [SerializeField, Range(1.01f, 1.99f), Tooltip("The growth factor applied to upgrade prices with each upgrade.")]
        private float upgradeGrowthFactor = 1.5f;

        [SerializeField, Tooltip("Base cost for unlocking items or features.")]
        private int baseUnlockPrice = 75;

        [SerializeField, Range(1.01f, 1.99f), Tooltip("The growth factor applied to unlock prices with each unlock.")]
        private float unlockGrowthFactor = 1.1f;

        [SerializeField, Tooltip("Starting money for the restaurant.")]
        private long startingMoney = 1000;

        [SerializeField, Tooltip("Offset distance for package items in the stack.")]
        private float logOffset = 0.3f;

        [SerializeField, Tooltip("Offset distance for package items in the stack.")]
        private float rockOffset = 0.3f;

        [SerializeField]
        private Canvas canvas;

        [Header("Employee")]
        [SerializeField, Tooltip("The point where employees will spawn.")]
        private Transform employeePoint;

        [Header("Employee (Log)")]
        [SerializeField, Tooltip("Log用の従業員プレハブ")]
        private EmployeeController employeePrefab;

        [SerializeField, Tooltip("Radius within which employees will spawn.")]
        private float employeeSpawnRadius = 3f;

        [Header("User Interface")]
        [SerializeField, Tooltip("Text field displaying the current money.")]
        private TMP_Text moneyDisplay;

        [SerializeField, Tooltip("Screen fader for transitions between scenes.")]
        private ScreenFader screenFader;

        [Header("Effects")]
        [SerializeField, Tooltip("Particle effect to play when unlocking something.")]
        private ParticleSystem unlockParticle;

        [SerializeField, Tooltip("Background music to play in the restaurant.")]
        private AudioClip backgroundMusic;

        [Header("Unlockable")]
        [SerializeField, Tooltip("The buyer object responsible for unlocking features.")]
        private UnlockableBuyer unlockableBuyer;

        [SerializeField, Tooltip("List of unlockables that can be bought.")]
        private List<Unlockable> unlockables;

        [SerializeField]
        private CameraController cameraController;

        #region Reference Properties

        public Canvas Canvas => canvas;

        public List<ObjectPile> TrashPiles { get; private set; } = new List<ObjectPile>();
        public TrashBin TrashBin { get; private set; }

        public List<ObjectPile> FoodPiles { get; private set; } = new List<ObjectPile>();
        public List<ObjectStack> FoodStacks { get; private set; } = new List<ObjectStack>();

        public ObjectPile PackagePile { get; private set; }
        public ObjectStack PackageStack { get; private set; }
        #endregion

        public event System.Action OnUpgrade;
        public event System.Action<float> OnUnlock;

        public int PaidAmount
        {
            get => data.PaidAmount;
            set => data.PaidAmount = value;
        }

        private int unlockCount
        {
            get => data.UnlockCount;
            set => data.UnlockCount = value;
        }

        private RestaurantData data;

        private string restaurantID;

        void Awake()
        {
            // Ensures that there is only one instance of RestaurantManager throughout the game.
            Instance = this;

            // Gets the name of the current scene to use as a unique restaurant ID.
            restaurantID = SceneManager.GetActiveScene().name;

            // Loads saved data for the current restaurant (if any exists).
            data = SaveSystem.LoadData<RestaurantData>(restaurantID);
            // If no data is found, initialize with default values (starting money and empty data).
            if (data == null)
                data = new RestaurantData(restaurantID, startingMoney);

            // Adjust the money with 0 adjustment to update the UI only.
            AdjustMoney(0);
        }

        void Start()
        {
            // Save the index of the current scene to track the last scene visited.
            SaveSystem.SaveData<int>(SceneManager.GetActiveScene().buildIndex, "LastSceneIndex");

            // Fade out the screen at the start of the scene.
            screenFader.FadeOut();

            // Find all ObjectPile instances in the scene and categorize them based on StackType.
            var objectPiles = FindObjectsOfType<ObjectPile>(true);

            // Loop through all ObjectPile instances and add them to the appropriate list (TrashPiles, FoodPiles, or assign PackagePile).
            foreach (var pile in objectPiles)
            {
                if (pile.StackType == StackType.Log)
                    TrashPiles.Add(pile);
                else if (pile.StackType == StackType.Rock)
                    FoodPiles.Add(pile);
            }

            // Find the TrashBin object in the scene and assign it to TrashBin.
            TrashBin = FindObjectOfType<TrashBin>(true);

            // Find all ObjectStack instances in the scene and categorize them.
            var objectStacks = FindObjectsOfType<ObjectStack>(true);

            // Loop through all ObjectStack instances and assign them to the corresponding list or object (FoodStacks or PackageStack).
            foreach (var stack in objectStacks)
            {
                if (stack.StackType == StackType.Log)
                    FoodStacks.Add(stack);
                else if (stack.StackType == StackType.Rock)
                    PackageStack = stack;
            }

            // Initialize unlocked Unlockables based on the data (UnlockCount).
            for (int i = 0; i < unlockCount; i++)
            {
                unlockables[i].Unlock(false);
            }

            // Update the UnlockableBuyer UI to reflect the current unlockable state.
            UpdateUnlockableBuyer();

            SpawnEmployee();

            // Play background music once the scene has loaded.
            AudioManager.Instance.PlayBGM(backgroundMusic);
        }

        void SpawnEmployee()
        {
            var materialManagerList = FindObjectsOfType<MaterialManager>();
            // Loop through every value of StackType.
            foreach (StackType stackType in Enum.GetValues(typeof(StackType)))
            {
                MaterialManager materialManager = null;
                foreach (var manager in materialManagerList)
                {
                    if (manager.StackType == stackType)
                    {
                        materialManager = manager;
                        break;
                    }
                }
                // Optionally, skip the None type.
                if (stackType == StackType.None || materialManager == null)
                    continue;

                // Determine how many employees of this type already exist.
                int currentCount = FindObjectsOfType<EmployeeController>()
                                    .Count(e => e.StackType == stackType);

                // Calculate how many employees need to be spawned for this StackType.
                int employeeAmount = data.FindUpgrade(Upgrade.UpgradeType.EmployeeAmount, stackType).Level;
                int toSpawn = employeeAmount - currentCount;
                if (toSpawn <= 0)
                    continue;

                // For layout, use a fixed number of columns.
                int numberOfColumns = 4;
                for (int i = currentCount; i < employeeAmount; i++)
                {
                    int columnIndex = i % numberOfColumns; // 0～3
                    int rowIndex = (i / numberOfColumns) + 1;

                    // Get patrol points based on column.
                    MaterialManager.PatrolPoints patrol = materialManager.GetPatrolPointsForColumn(columnIndex + 1);
                    if (patrol == null)
                    {
                        Debug.LogWarning("Column " + (columnIndex + 1) + " のパトロール地点が取得できません。");
                        continue;
                    }

                    // Create temporary GameObjects to hold the patrol point positions,
                    // with names including the StackType for clarity.
                    GameObject pointAObj = new GameObject($"PatrolPointA_Column{columnIndex + 1}_{stackType}");
                    pointAObj.transform.position = patrol.pointA;
                    pointAObj.transform.parent = transform;

                    GameObject pointBObj = new GameObject($"PatrolPointB_Column{columnIndex + 1}_{stackType}");
                    pointBObj.transform.position = patrol.pointB;
                    pointBObj.transform.parent = transform;

                    // Instantiate the employee at the first patrol point.
                    EmployeeController employee = Instantiate(employeePrefab, pointAObj.transform.position, Quaternion.identity);
                    employee.SetPatrolPoints(pointAObj.transform, pointBObj.transform);
                    employee.Column = columnIndex + 1;
                    employee.Row = rowIndex;
                    // Assign the current StackType to the employee.
                    employee.StackType = stackType;
                }
            }
        }

        void Update()
        {
            // DEBUG: Please remove on build!
            if (SimpleInput.GetButtonDown("DebugMoney"))
            {
                AdjustMoney(10000);
                SaveSystem.SaveData<RestaurantData>(data, restaurantID);
                Debug.Log("Added 10,000 Money for debugging purposes. Please remove on build!");
            }
        }

        /// <summary>
        /// Returns the appropriate offset for a specific stack type.
        /// The offset determines the position adjustment (height) for the stack of objects (e.g., food, trash, or packages).
        /// </summary>
        /// <param name="stackType">The type of the stack (Food, Trash, Package, or None).</param>
        /// <returns>A float value representing the stack offset for the given stack type.</returns>
        public float GetStackOffset(StackType stackType) => stackType switch
        {
            StackType.Log => logOffset,
            StackType.Rock => rockOffset,
            StackType.None => 0f,
            _ => 0f
        };

        /// <summary>
        /// Purchases and unlocks the next available unlockable item.
        /// The method triggers the unlocking process, plays visual and audio effects, and updates the unlockable count.
        /// </summary>
        public void BuyUnlockable()
        {
            // Unlock the next unlockable item
            unlockables[unlockCount].Unlock();

            // Play the unlock particle effect at the unlockable's position
            unlockParticle.transform.position = unlockables[unlockCount].transform.position;
            unlockParticle.Play();

            // Play the magical sound effect for the unlock
            AudioManager.Instance.PlaySFX(AudioID.Magical);

            // Increment the unlock count and reset the paid amount for the next unlock
            unlockCount++;
            PaidAmount = 0;

            // Update the unlockable buyer UI
            UpdateUnlockableBuyer();

            cameraController.FocusOnPointAndReturn(unlockables[unlockCount].GetBuyingPoint());

            // Save the progress of the restaurant data
            SaveSystem.SaveData<RestaurantData>(data, restaurantID);
        }

        void UpdateUnlockableBuyer()
        {
            // Check if there are any unlockables in the scene
            if (unlockables?.Count(unlockable => unlockable != null) == 0)
            {
                // Log a warning if no unlockables are present
                Debug.LogWarning("There are no unlockables present in the scene! Please add the necessary unlockable items to proceed.");
                return;
            }

            // If there are still unlockables to purchase, update the UI for the next unlockable
            if (unlockCount < unlockables.Count)
            {
                var unlockable = unlockables[unlockCount];

                // Position the unlockable buyer at the correct location
                unlockableBuyer.transform.position = unlockable.GetBuyingPoint();

                // Calculate the price for the next unlockable
                int price = Mathf.RoundToInt(Mathf.Round(baseUnlockPrice * Mathf.Pow(unlockGrowthFactor, unlockCount)) / 5f) * 5;

                // Initialize the unlockable buyer UI with the unlockable and its price
                unlockableBuyer.Initialize(unlockable, price, PaidAmount);
            }
            else
            {
                // Mark the restaurant as fully unlocked if all unlockables have been bought
                data.IsUnlocked = true;

                // Hide the unlockable buyer UI
                unlockableBuyer.gameObject.SetActive(false);
            }

            // Calculate the progress based on how many unlockables have been purchased
            float progress = data.UnlockCount / (float)unlockables.Count;

            // Trigger the OnUnlock event to notify listeners about the unlock progress
            OnUnlock?.Invoke(progress);
        }

        public void AdjustMoney(int change)
        {
            data.Money += change;
            moneyDisplay.text = GetFormattedMoney(data.Money);
        }

        public long GetMoney()
        {
            return data.Money;
        }

        /// <summary>
        /// Formats the given amount of money as a human-readable string.
        /// Converts the value into a short-form format with appropriate suffixes for thousands, millions, billions, etc.
        /// </summary>
        /// <param name="money">The amount of money to format.</param>
        /// <returns>A string representing the money value in a readable format (e.g., "1.5k", "2.3m", etc.).</returns>
        public string GetFormattedMoney(long money)
        {
            if (money < 1000)
                return money.ToString(); // No suffix for values under 1000
            else if (money < 1000000)
                return (money / 1000f).ToString("0.##") + "k"; // Thousands (k)
            else if (money < 1000000000)
                return (money / 1000000f).ToString("0.##") + "m"; // Millions (m)
            else if (money < 1000000000000)
                return (money / 1000000000f).ToString("0.##") + "b"; // Billions (b)
            else
                return (money / 1000000000000f).ToString("0.##") + "t"; // Trillions (t)
        }

        /// <summary>
        /// Purchases an upgrade for the restaurant, deducting the required money from the player's balance and applying the corresponding upgrade.
        /// The upgrade could be related to employee speed, capacity, number of employees, or player stats like speed and capacity.
        /// </summary>
        /// <param name="upgrade">The upgrade to purchase. This could be an employee upgrade or a player upgrade.</param>
        public void PurchaseUpgrade(Upgrade.UpgradeType upgradeType, StackType stackType)
        {
            int price = GetUpgradePrice(upgradeType, stackType); // Calculate the price for the selected upgrade
            AdjustMoney(-price); // Deduct the price from the player's money

            data.UpgradeUpgrade(upgradeType, stackType);
            if (upgradeType == Upgrade.UpgradeType.EmployeeAmount)
            {
                SpawnEmployee();
            }

            AudioManager.Instance.PlaySFX(AudioID.Kaching);

            SaveSystem.SaveData<RestaurantData>(data, restaurantID);

            OnUpgrade?.Invoke();
        }

        public int GetUpgradePrice(Upgrade.UpgradeType upgradeType, StackType stackType)
        {
            int currentLevel = GetUpgradeLevel(upgradeType, stackType); // Get the current level of the selected upgrade
            float levelPrice = baseUpgradePrice * Mathf.Pow(upgradeGrowthFactor, currentLevel);
            float typePrice = levelPrice * MathF.Pow(5, (int)stackType - (int)StackType.Log);
            return Mathf.RoundToInt(Mathf.Round(typePrice) / 50f) * 50; // Calculate the price based on the upgrade's growth factor
        }

        public int GetUpgradeLevel(Upgrade.UpgradeType upgradeType, StackType stackType)
        {
            return data.FindUpgrade(upgradeType, stackType).Level;
        }

        /// <summary>
        /// Loads a different restaurant scene by saving the current state and transitioning to the specified scene.
        /// The scene is only loaded if the specified scene index differs from the current one.
        /// </summary>
        /// <param name="index">The index of the scene to load.</param>
        public void LoadRestaurant(int index)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (index == currentSceneIndex)
                return; // Avoid reloading the current scene

            // Fade out the current screen and load the new scene after saving the data
            screenFader.FadeIn(() =>
            {
                SaveSystem.SaveData<RestaurantData>(data, restaurantID); // Save current restaurant data
                SceneManager.LoadScene(index); // Load the specified scene
            });
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                SaveSystem.SaveData<RestaurantData>(data, restaurantID);
        }

        void OnDisable()
        {
            DOTween.KillAll();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (employeePoint == null)
                return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(employeePoint.position, employeeSpawnRadius);
        }
#endif
    }

    [System.Serializable]
    public class Upgrade
    {
        public enum UpgradeType
        {
            EmployeeSpeed, EmployeeCapacity, EmployeeAmount,
        }
        public UpgradeType upgradeType;
        public StackType StackType;
        public int Level;

        public Upgrade(UpgradeType type = UpgradeType.EmployeeSpeed, StackType stackType = StackType.None, int level = 0)
        {
            this.upgradeType = type;
            this.StackType = stackType;
            this.Level = level;
        }
    }
}
