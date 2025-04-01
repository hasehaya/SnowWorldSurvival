using System;

using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class NonConsumableIAP :MonoBehaviour, IDetailedStoreListener
{
    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    public string nonConsumableProductId = "com.example.myapp.nonconsumable";

    void Start()
    {
        InitializePurchasing();
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

    public void BuyNonConsumable()
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

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("IAPが初期化されていません。");
            return;
        }

#if UNITY_IOS
        Debug.Log("iOS用レシート復元処理を開始します。");
        var apple = extensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions(result => {
            Debug.Log("レシート復元の結果: " + result);
        });
#elif UNITY_ANDROID
        Debug.Log("Androidではレシート復元は不要です。");
#endif
    }

    // IDetailedStoreListener の実装

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAPの初期化に成功しました。");
        storeController = controller;
        extensionProvider = extensions;
    }

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
            // ユーザーの機能解放などの処理を行う
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
