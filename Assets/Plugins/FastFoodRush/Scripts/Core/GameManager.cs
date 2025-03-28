using System;
using System.Collections.Generic;
using System.Linq;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CryingSnow.FastFoodRush
{
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
        [SerializeField, Tooltip("�]�ƈ��v���n�u")]
        private EmployeeController employeePrefab;

        [Header("User Interface")]
        [SerializeField, Tooltip("Text field displaying the current money.")]
        private TMP_Text moneyDisplay;

        [SerializeField, Tooltip("Screen fader for transitions between scenes.")]
        private ScreenFader screenFader;

        [Header("Effects")]
        [SerializeField, Tooltip("Background music to play in the restaurant.")]
        private AudioClip backgroundMusic;

        [Header("Other References")]
        [SerializeField]
        private CameraController cameraController;

        #region Reference Properties

        public float ElapsedTime => data?.ElapsedTime ?? 0f;

        public Canvas Canvas => canvas;

        public List<ObjectPile> TrashPiles { get; private set; } = new List<ObjectPile>();
        public TrashBin TrashBin { get; private set; }

        public List<ObjectPile> FoodPiles { get; private set; } = new List<ObjectPile>();
        public List<ObjectStack> FoodStacks { get; private set; } = new List<ObjectStack>();

        public ObjectPile PackagePile { get; private set; }
        public ObjectStack PackageStack { get; private set; }
        #endregion

        public event Action OnUpgrade;

        private SaveData data;
        private string restaurantID;

        void Awake()
        {
            // �V���O���g���̐ݒ�
            Instance = this;

            // ���݂̃V�[���������X�g����ID�Ƃ��ė��p
            restaurantID = SceneManager.GetActiveScene().name;

            // �Z�[�u�f�[�^�̃��[�h�i���݂��Ȃ��ꍇ�͏�����Ԃō쐬�j
            data = SaveSystem.LoadData<SaveData>(restaurantID);
            if (data == null)
                data = new SaveData(restaurantID, startingMoney);

            // UI�\���̂��߂̏����ʉݐݒ�
            AdjustMoney(0);
        }

        void Start()
        {
            // ���݂̃V�[���C���f�b�N�X��ۑ�
            SaveSystem.SaveData<int>(SceneManager.GetActiveScene().buildIndex, "LastSceneIndex");

            // �V�[���J�n���̃t�F�[�h�A�E�g����
            screenFader.FadeOut();

            // �V�[������ ObjectPile ���������ĕ���
            var objectPiles = FindObjectsOfType<ObjectPile>(true);
            foreach (var pile in objectPiles)
            {
                if (pile.MaterialType == MaterialType.Log)
                    TrashPiles.Add(pile);
                else if (pile.MaterialType == MaterialType.Rock)
                    FoodPiles.Add(pile);
            }

            // TrashBin �̌���
            TrashBin = FindObjectOfType<TrashBin>(true);

            // �V�[������ ObjectStack ���������ĕ���
            var objectStacks = FindObjectsOfType<ObjectStack>(true);
            foreach (var stack in objectStacks)
            {
                if (stack.MaterialType == MaterialType.Log)
                    FoodStacks.Add(stack);
                else if (stack.MaterialType == MaterialType.Rock)
                    PackageStack = stack;
            }

            // UnlockManager �̏������i�Z�[�u�f�[�^�ƃ��X�g����ID��n���j
            UnlockManager.Instance.InitializeUnlockManager(data, restaurantID);

            // �]�ƈ��̃X�|�[������
            SpawnEmployee();

            // BGM �̍Đ�
            AudioManager.Instance.PlayBGM(backgroundMusic);
        }

        void SpawnEmployee()
        {
            var materialManagerList = FindObjectsOfType<MaterialParent>(true);
            // MaterialType �̑S��
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
                // None �^�C�v�܂��͊Y������Ǘ��I�u�W�F�N�g��������΃X�L�b�v
                if (materialType == MaterialType.None || materialManager == null)
                    continue;

                // ���݂̏]�ƈ������擾
                int currentCount = FindObjectsOfType<EmployeeController>()
                                    .Count(e => e.MaterialType == materialType);
                // �Z�[�u�f�[�^����]�ƈ����̃A�b�v�O���[�h���x�����擾
                int employeeAmount = data.FindUpgrade(Upgrade.UpgradeType.EmployeeAmount, materialType).Level;
                int toSpawn = employeeAmount - currentCount;
                if (toSpawn <= 0)
                    continue;

                // ���C�A�E�g�p�ɌŒ�̗񐔂��w��
                int numberOfColumns = 4;
                for (int i = currentCount; i < employeeAmount; i++)
                {
                    int columnIndex = i % numberOfColumns;
                    int rowIndex = (i / numberOfColumns) + 1;

                    // �w��̗�ɑΉ�����p�g���[���n�_���擾
                    MaterialParent.PatrolPoints patrol = materialManager.GetPatrolPointsForColumn(columnIndex + 1);
                    if (patrol == null)
                    {
                        Debug.LogWarning("Column " + (columnIndex + 1) + " �̃p�g���[���n�_���擾�ł��܂���B");
                        continue;
                    }

                    // �p�g���[���n�_�p�̈ꎞ�I�u�W�F�N�g�𐶐�
                    GameObject pointAObj = new GameObject($"PatrolPointA_Column{columnIndex + 1}_{materialType}");
                    pointAObj.transform.position = patrol.pointA;
                    pointAObj.transform.parent = transform;

                    GameObject pointBObj = new GameObject($"PatrolPointB_Column{columnIndex + 1}_{materialType}");
                    pointBObj.transform.position = patrol.pointB;
                    pointBObj.transform.parent = transform;

                    // �]�ƈ��̐����Ə�����
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
            if (data != null)
            {
                data.ElapsedTime += Time.unscaledDeltaTime;
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
            data.Money += change;
            moneyDisplay.text = GetFormattedMoney(data.Money);
        }

        public long GetMoney() => data.Money;

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

            data.UpgradeUpgrade(upgradeType, materialType);
            if (upgradeType == Upgrade.UpgradeType.EmployeeAmount)
            {
                SpawnEmployee();
            }

            AudioManager.Instance.PlaySFX(AudioID.Kaching);
            SaveSystem.SaveData<SaveData>(data, restaurantID);
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
            return data.FindUpgrade(upgradeType, materialType).Level;
        }

        public void LoadRestaurant(int index)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (index == currentSceneIndex)
                return; // ���݂̃V�[���̍ēǂݍ��݂�h�~

            screenFader.FadeIn(() =>
            {
                SaveSystem.SaveData<SaveData>(data, restaurantID);
                SceneManager.LoadScene(index);
            });
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                SaveSystem.SaveData<SaveData>(data, restaurantID);
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
}
