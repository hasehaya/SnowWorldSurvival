using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace CryingSnow.FastFoodRush
{
    public class UpgradeHandler :MonoBehaviour
    {
        [SerializeField, Tooltip("Type of upgrade handled by this component.")]
        private Upgrade.UpgradeType upgradeType;

        private MaterialType materialType;

        [SerializeField, Tooltip("Button used to purchase the upgrade.")]
        private Button upgradeButton;

        [SerializeField, Tooltip("Text label displaying the upgrade price.")]
        private TMP_Text priceLabel;

        [SerializeField, Tooltip("Visual indicators for the upgrade level.")]
        private Image[] indicators;

        private Color activeColor = new Color(1f, 0.5f, 0.25f, 1f);

        void Start()
        {
            // Initializes the upgrade button and subscribes to relevant events.
            upgradeButton.onClick.AddListener(() =>
                GameManager.Instance.PurchaseUpgrade(upgradeType, materialType)
            );

            GameManager.Instance.OnUpgrade += UpdateHandler;
        }

        void OnEnable()
        {
            // Updates the upgrade handler's state when enabled.
            UpdateHandler();
        }

        void UpdateHandler()
        {
            SetMaterialType(materialType);
        }

        public void SetMaterialType(MaterialType materialType)
        {
            this.materialType = materialType;
            int level = GameManager.Instance.GetUpgradeLevel(upgradeType, this.materialType);

            for (int i = 0; i < indicators.Length; i++)
            {
                indicators[i].color = i < level ? activeColor : Color.gray;
            }

            if (level < 5)
            {
                int price = GameManager.Instance.GetUpgradePrice(upgradeType, this.materialType);
                priceLabel.text = GameManager.Instance.GetFormattedMoney(price);

                bool hasEnoughMoney = GameManager.Instance.GetMoney() >= price;
                upgradeButton.interactable = hasEnoughMoney;
            }
            else
            {
                priceLabel.text = "MAX";
                upgradeButton.interactable = false;
            }
        }
    }
}
