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
            Debug.Log("IAP������������Ă��܂���B");
            return;
        }

        Product product = storeController.products.WithID(nonConsumableProductId);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log("�w���J�n: " + product.definition.id);
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.Log("�w���Ώۂ̏��i�����݂��Ȃ����A�w���ł��܂���B");
        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("IAP������������Ă��܂���B");
            return;
        }

#if UNITY_IOS
        Debug.Log("iOS�p���V�[�g�����������J�n���܂��B");
        var apple = extensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions(result => {
            Debug.Log("���V�[�g�����̌���: " + result);
        });
#elif UNITY_ANDROID
        Debug.Log("Android�ł̓��V�[�g�����͕s�v�ł��B");
#endif
    }

    // IDetailedStoreListener �̎���

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAP�̏������ɐ������܂����B");
        storeController = controller;
        extensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("IAP�̏������Ɏ��s���܂���: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("IAP�̏������Ɏ��s���܂���: " + error + " - " + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (string.Equals(args.purchasedProduct.definition.id, nonConsumableProductId, StringComparison.Ordinal))
        {
            Debug.Log("�����^���i�̍w���ɐ������܂���: " + args.purchasedProduct.definition.id);
            // ���[�U�[�̋@�\����Ȃǂ̏������s��
            return PurchaseProcessingResult.Complete;
        }
        else
        {
            Debug.Log("�F���ł��Ȃ��w�����i: " + args.purchasedProduct.definition.id);
            return PurchaseProcessingResult.Complete;
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("�w�����s: " + product.definition.id + ", ���R: " + failureReason);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log("�w�����s: " + product.definition.id + ", ���R: " + failureDescription.reason + ", �ڍ�: " + failureDescription.message);
    }
}
