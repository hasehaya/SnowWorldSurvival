using UnityEngine;

public class AdBlockBtn :MonoBehaviour
{
    [SerializeField] private GameObject popup;

    public void OpenPopup()
    {
        if (popup != null)
        {
            popup.SetActive(!popup.activeSelf);
        }
    }

    public void ClosePopup()
    {
        if (popup != null)
        {
            popup.SetActive(false);
        }
    }

    public void PurchaseAdBlock()
    {
        NonConsumableIAP.Instance.Purchase();
    }
}
