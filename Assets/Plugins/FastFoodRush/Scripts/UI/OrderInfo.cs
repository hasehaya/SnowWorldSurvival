using TMPro;

using UnityEngine;
using UnityEngine.UI;


public class OrderInfo :MonoBehaviour
{
    public Image IconImage;

    [SerializeField, Tooltip("The text component that shows the amount for the order.")]
    private TMP_Text amountText;

    [SerializeField, Tooltip("The offset from the displayer's position to place the order info on screen.")]
    private Vector3 displayOffset = new Vector3(0f, 2.5f, 0f);

    private Transform displayer;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (displayer == null)
            return;

        // Calculate the display position of the order info in screen space
        var displayPosition = displayer.position + displayOffset;
        transform.position = mainCamera.WorldToScreenPoint(displayPosition); // Set the position of the UI element on screen
    }

    public void ShowInfo(Transform displayer, int amount)
    {
        gameObject.SetActive(true);
        this.displayer = displayer;
        amountText.text = amount.ToString();
    }

    public void HideInfo()
    {
        gameObject.SetActive(false); // Disable the order info UI element
        displayer = null; // Reset the displayer reference
    }
}
