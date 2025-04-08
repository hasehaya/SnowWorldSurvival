using System;

using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

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

    public void Purchase()
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

    /// <summary>
    /// iOS�p�̕��������i���[�U�[����ŌĂяo���Ă��������j
    /// </summary>
    public void RestorePurchasesForIOS()
    {
#if UNITY_IOS
        if (!IsInitialized())
        {
            Debug.Log("IAP������������Ă��܂���B");
            return;
        }
        Debug.Log("iOS�p���V�[�g�����������J�n���܂��B");
        var apple = extensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions(result =>
        {
            Debug.Log("���V�[�g�����̌���: " + result);
            if(result)
            {
                CheckForPurchase();
            }
        });
#else
        Debug.Log("���̋@�\��iOS�̂ݗL���ł��B");
#endif
    }

    /// <summary>
    /// Android�̏ꍇ�A�������������Ɏ����I�ɍw����Ԃ��`�F�b�N���܂��B
    /// </summary>
    private void AutoRestoreForAndroid()
    {
#if UNITY_ANDROID
        if (!IsInitialized())
        {
            Debug.Log("IAP������������Ă��܂���B");
            return;
        }
        Debug.Log("Android�p�����w����Ԋm�F���J�n���܂��B");
        CheckForPurchase();
#endif
    }

    /// <summary>
    /// �w���ς݂��ǂ������e���i����`�F�b�N���āA���w���Ȃ�@�\������������s
    /// </summary>
    public void CheckForPurchase()
    {
        foreach (var product in storeController.products.all)
        {
            if (product.definition.id == nonConsumableProductId && product.hasReceipt)
            {
                Debug.Log("���ɍw���ς݂̏��i��������܂���: " + product.definition.id);
                GameManager.Instance.PurchaseAdBlock();
                break;
            }
        }
    }

    // IDetailedStoreListener �̎���

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAP�̏������ɐ������܂����B");
        storeController = controller;
        extensionProvider = extensions;

        // Android�̏ꍇ�͏������������Ɏ����I�ɍw����Ԃ��`�F�b�N
#if UNITY_ANDROID
        AutoRestoreForAndroid();
#endif
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
            GameManager.Instance.PurchaseAdBlock();
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
