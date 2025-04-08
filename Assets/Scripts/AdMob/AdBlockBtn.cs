using UnityEngine;

public class AdBlockBtn :MonoBehaviour
{
    [SerializeField] private GameObject popup;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            gameObject.SetActive(false);
        }
    }

    public void OpenPopup()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            gameObject.SetActive(false);
            return;
        }
        if (popup != null)
        {
            popup.SetActive(!popup.activeSelf);
            NonConsumableIAP.Instance.CheckForPurchase();
        }
    }

    public void ClosePopup()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            gameObject.SetActive(false);
        }
        if (popup != null)
        {
            popup.SetActive(false);
        }
    }

    public void PurchaseAdBlock()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAdBlocked())
        {
            gameObject.SetActive(false);
            ClosePopup();
            return;
        }
        NonConsumableIAP.Instance.Purchase();
    }
}
