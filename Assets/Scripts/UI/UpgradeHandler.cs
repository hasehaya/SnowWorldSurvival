using System.Runtime.CompilerServices;
using TMPro;

using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;


public class UpgradeHandler :MonoBehaviour
{
    [SerializeField, Tooltip("Type of upgrade handled by this component.")]
    private Upgrade.UpgradeType upgradeType;

    private MaterialType materialType;

    [SerializeField, Tooltip("Button used to purchase the upgrade.")]
    private Button upgradeButton;

    [SerializeField]
    private Button adButton;

    [SerializeField, Tooltip("Text label displaying the upgrade price.")]
    private TMP_Text priceLabel;

    [SerializeField]
    private TMP_FontAsset jpFont;

    [SerializeField, Tooltip("Visual indicators for the upgrade level.")]
    private Image[] indicators;

    private Color activeColor = new Color(1f, 0.5f, 0.25f, 1f);

    void Start()
    {
        upgradeButton.onClick.AddListener(() =>
            GameManager.Instance.PurchaseUpgrade(upgradeType, materialType)
        );

        adButton.onClick.AddListener(() => GameManager.Instance.RequestUpgradeByAd(upgradeType, materialType)
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
            priceLabel.font = jpFont;

            bool hasEnoughMoney = GameManager.Instance.GetMoney() >= price;
            
            // AdMobRewardが読み込まれていない場合は広告ボタンを無効化
            bool isAdLoaded = AdMobReward.Instance != null && AdMobReward.Instance.IsAdLoaded;
            
            // EmployeeAmountのレベルをチェック
            int employeeAmountLevel = GameManager.Instance.GetUpgradeLevel(Upgrade.UpgradeType.EmployeeAmount, this.materialType);
            
            // EmployeeAmount以外のアップグレードの場合、EmployeeAmountが1以上でないと有効にしない
            if (upgradeType != Upgrade.UpgradeType.EmployeeAmount && employeeAmountLevel < 1)
            {
                upgradeButton.interactable = false;
                adButton.interactable = false;
                adButton.gameObject.SetActive(true);
            }
            else
            {
                upgradeButton.interactable = hasEnoughMoney;
                adButton.interactable = isAdLoaded; // 広告が読み込まれているかどうかに基づいて設定
                adButton.gameObject.SetActive(true);
            }
        }
        else
        {
            priceLabel.text = "MAX";
            priceLabel.font = jpFont;
            upgradeButton.interactable = false;
            adButton.gameObject.SetActive(false);
        }
    }
}
