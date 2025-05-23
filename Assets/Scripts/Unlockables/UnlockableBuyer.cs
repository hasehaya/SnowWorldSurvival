﻿using System.Collections;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.UI;


public class UnlockableBuyer :Interactable
{
    private float payingInterval = 0.03f;
    private float payingTime = 1.65f;
    private float delayBeforePay = 0.6f;

    [SerializeField, Tooltip("The UI image used to represent the payment progress.")]
    private Image progressFill;

    [SerializeField, Tooltip("The label displaying the remaining price for the unlockable.")]
    private TMP_Text priceLabel;

    [SerializeField]
    private Image contentIcon;

    private long playerMoney => GameManager.Instance.GetMoney();  // The current amount of money the player has

    private Unlockable unlockable;  // The unlockable object that can be bought
    private int unlockPrice;       // The price required to unlock the unlockable
    private int paidAmount;        // The amount the player has already paid

    // Added field to store the MaterialType this buyer is associated with.
    private MaterialType buyerMaterial;

    private Coroutine payCoroutine;  // Reference to the currently running coroutine

    /// <summary>
    /// Initializes the UnlockableBuyer with the provided unlockable, price details, and associated MaterialType.
    /// </summary>
    /// <param name="unlockable">The unlockable object to be bought</param>
    /// <param name="unlockPrice">The price of the unlockable</param>
    /// <param name="paidAmount">The amount already paid towards the unlockable</param>
    /// <param name="buyerMaterial">The MaterialType that this buyer belongs to</param>
    public void Initialize(Unlockable unlockable, int unlockPrice, int paidAmount, MaterialType buyerMaterial)
    {
        this.unlockable = unlockable;
        this.unlockPrice = unlockPrice;
        this.paidAmount = paidAmount;
        this.buyerMaterial = buyerMaterial;
        contentIcon.sprite = unlockable.ContentIcon;

        UpdatePayment(0);  // Update the payment progress display
    }

    /// <summary>
    /// Updates the payment progress and the remaining price label.
    /// </summary>
    /// <param name="amount">The amount to add to the paid amount</param>
    void UpdatePayment(int amount)
    {
        paidAmount += amount;
        progressFill.fillAmount = (float)paidAmount / unlockPrice;  // Update the progress bar
        priceLabel.text = GameManager.Instance.GetFormattedMoney(unlockPrice - paidAmount);  // Update the price label
    }

    /// <summary>
    /// Triggered when the player enters the interactable area.
    /// Starts a delayed coroutine to begin the payment process.
    /// </summary>
    protected override void OnPlayerEnter()
    {
        // Start a coroutine that delays payment start by delayBeforePay seconds
        payCoroutine = StartCoroutine(DelayedPay());
    }

    /// <summary>
    /// Triggered when the player exits the interactable area.
    /// Stops the payment coroutine to reset the timer.
    /// </summary>
    protected override void OnPlayerExit()
    {
        if (payCoroutine != null)
        {
            StopCoroutine(payCoroutine);
            payCoroutine = null;
        }
    }

    /// <summary>
    /// Coroutine that waits for a delay before starting the payment process.
    /// If the player is still in range after the delay, the payment process begins.
    /// </summary>
    IEnumerator DelayedPay()
    {
        yield return new WaitForSeconds(delayBeforePay);
        // Ensure the player is still in range before starting payment
        if (player != null)
        {
            yield return StartCoroutine(Pay());
        }
    }

    /// <summary>
    /// Coroutine that handles the process of paying for the unlockable item.
    /// </summary>
    IEnumerator Pay()
    {
        // Continue paying while the player is inside the trigger, the unlockable is not fully paid, and the player has money
        while (player != null && paidAmount < unlockPrice && playerMoney > 0)
        {
            float paymentRate = unlockPrice * payingInterval / payingTime;  // Calculate the rate of payment per interval
            paymentRate = Mathf.Min(playerMoney, paymentRate);  // Ensure payment does not exceed the player's money
            int payment = Mathf.Max(1, Mathf.RoundToInt(paymentRate));  // Ensure a minimum payment of 1

            UpdatePayment(payment);  // Update the progress
            GameManager.Instance.AdjustMoney(-payment);  // Deduct the payment from the player's money

            PlayMoneyAnimation();  // Play the money animation

            // If the total amount paid is equal to or greater than the unlock price, complete the purchase
            if (paidAmount >= unlockPrice)
            {
                // Call BuyUnlockable with the associated MaterialType.
                UnlockManager.Instance.BuyUnlockable(buyerMaterial);
            }

            yield return new WaitForSeconds(payingInterval);  // Wait for the next payment interval
        }
    }

    /// <summary>
    /// Plays the animation for money moving from the player to the unlockable.
    /// </summary>
    void PlayMoneyAnimation()
    {
        var moneyObj = PoolManager.Instance.SpawnObject("Money");  // Spawn a money object
        moneyObj.transform.position = player.transform.position + Vector3.up * 2;  // Position it above the player
        moneyObj.transform.DOJump(transform.position, 3f, 1, 0.5f)  // Animate the money moving to the unlockable
            .OnComplete(() => PoolManager.Instance.ReturnObject(moneyObj));  // Return the object to the pool after animation

        AudioManager.Instance.PlaySFX(AudioID.Money);  // Play the money sound effect
    }
}
