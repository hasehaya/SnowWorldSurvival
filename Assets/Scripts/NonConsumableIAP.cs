using System;

using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using TMPro;

public class NonConsumableIAP :MonoBehaviour, IDetailedStoreListener
{
    private static NonConsumableIAP instance;
    public static NonConsumableIAP Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NonConsumableIAP>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("NonConsumableIAP");
                    instance = obj.AddComponent<NonConsumableIAP>();
                }
            }
            return instance;
        }
    }
    private IStoreController storeController;
    private IExtensionProvider extensionProvider;
    [SerializeField] private TMP_Text priceText;

    static string nonConsumableProductId = "SnowWorldSurvival_AdBlock";

    void Start()
    {
        InitializePurchasing();
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAPの初期化に成功しました。");
        storeController = controller;
        extensionProvider = extensions;

        // 対象商品の Product を取得
        Product product = storeController.products.WithID(nonConsumableProductId);
        if (product != null && product.hasReceipt)
        {
            // metadata から表示用価格を取得
            string localizedPriceText = product.metadata.localizedPriceString;    // 例: "¥980" / "$6.99" / "€6,99"
            string currencyCode = product.metadata.isoCurrencyCode;         // 例: "JPY" / "USD" / "EUR"

            // 取得した文字列を UI にセット
            priceText.text = localizedPriceText + " (" + currencyCode + ")";
            Debug.Log($"表示価格: {localizedPriceText} ({currencyCode})");
        }

        // Androidの場合は初期化完了時に自動的に購入状態をチェック
#if UNITY_ANDROID
        AutoRestoreForAndroid();
#endif
    }


    public void InitializePurchasing()
    {
        if (IsInitialized())
            return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(nonConsumableProductId, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return (storeController != null && extensionProvider != null);
    }

    public void Purchase()
    {
        if (!IsInitialized())
        {
            Debug.Log("IAPが初期化されていません。");
            return;
        }

        Product product = storeController.products.WithID(nonConsumableProductId);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log("購入開始: " + product.definition.id);
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.Log("購入対象の商品が存在しないか、購入できません。");
        }
    }

    /// <summary>
    /// iOS用の復元処理（ユーザー操作で呼び出してください）
    /// </summary>
    public void RestorePurchasesForIOS()
    {
#if UNITY_IOS
        if (!IsInitialized())
        {
            Debug.Log("IAPが初期化されていません。");
            return;
        }
        Debug.Log("iOS用レシート復元処理を開始します。");
        var apple = extensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions(result =>
        {
            Debug.Log("レシート復元の結果: " + result);
            if(result)
            {
                CheckForPurchase();
            }
        });
#else
        Debug.Log("この機能はiOSのみ有効です。");
#endif
    }

    /// <summary>
    /// Androidの場合、初期化完了時に自動的に購入状態をチェックします。
    /// </summary>
    private void AutoRestoreForAndroid()
    {
#if UNITY_ANDROID
        if (!IsInitialized())
        {
            Debug.Log("IAPが初期化されていません。");
            return;
        }
        Debug.Log("Android用自動購入状態確認を開始します。");
        CheckForPurchase();
#endif
    }

    /// <summary>
    /// 購入済みかどうかを各商品からチェックして、既購入なら機能解放処理を実行
    /// </summary>
    public void CheckForPurchase()
    {
        foreach (var product in storeController.products.all)
        {
            if (product.definition.id == nonConsumableProductId && product.hasReceipt)
            {
                Debug.Log("既に購入済みの商品が見つかりました: " + product.definition.id);
                GameManager.Instance.PurchaseAdBlock();
                break;
            }
        }
    }

    // IDetailedStoreListener の実装

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("IAPの初期化に失敗しました: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("IAPの初期化に失敗しました: " + error + " - " + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (string.Equals(args.purchasedProduct.definition.id, nonConsumableProductId, StringComparison.Ordinal))
        {
            Debug.Log("非消費型商品の購入に成功しました: " + args.purchasedProduct.definition.id);
            GameManager.Instance.PurchaseAdBlock();
            return PurchaseProcessingResult.Complete;
        }
        else
        {
            Debug.Log("認識できない購入商品: " + args.purchasedProduct.definition.id);
            return PurchaseProcessingResult.Complete;
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("購入失敗: " + product.definition.id + ", 理由: " + failureReason);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log("購入失敗: " + product.definition.id + ", 理由: " + failureDescription.reason + ", 詳細: " + failureDescription.message);
    }
}
