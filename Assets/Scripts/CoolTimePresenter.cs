using UnityEngine;
using System;

public class CoolTimePresenter :MonoBehaviour
{
    // Assumes an array of three CoolTimeView elements in the following order:
    // 0: PlayerSpeed, 1: PlayerCapacity, 2: MoneyCollection.
    [SerializeField]
    private CoolTimeView[] coolTimes = new CoolTimeView[3];

    private GlobalData globalData;

    private const float TotalDuration = 180f;
    
    // 効果終了時のイベント
    public static event Action<RewardType> OnEffectEnded;

    private void Start()
    {
        globalData = GameManager.Instance.GlobalData;
    }

    private void Update()
    {
        // Decrement the cooldown timers.
        float dt = Time.deltaTime;

        if (globalData.PlayerSpeedRemainingSeconds > 0f)
        {
            float prevValue = globalData.PlayerSpeedRemainingSeconds;
            globalData.PlayerSpeedRemainingSeconds = Mathf.Max(0f, globalData.PlayerSpeedRemainingSeconds - dt);
            UpdateCoolTimeView(globalData.PlayerSpeedRemainingSeconds, coolTimes[0]);
            
            // 効果が終了したときにイベントを発火
            if (prevValue > 0f && globalData.PlayerSpeedRemainingSeconds <= 0f)
            {
                OnEffectEnded?.Invoke(RewardType.PlayerSpeed);
            }
        }
        else
        {
            UpdateCoolTimeView(0f, coolTimes[0]);
        }

        if (globalData.PlayerCapacityRemainingSeconds > 0f)
        {
            float prevValue = globalData.PlayerCapacityRemainingSeconds;
            globalData.PlayerCapacityRemainingSeconds = Mathf.Max(0f, globalData.PlayerCapacityRemainingSeconds - dt);
            UpdateCoolTimeView(globalData.PlayerCapacityRemainingSeconds, coolTimes[1]);
            
            // 効果が終了したときにイベントを発火
            if (prevValue > 0f && globalData.PlayerCapacityRemainingSeconds <= 0f)
            {
                OnEffectEnded?.Invoke(RewardType.PlayerCapacity);
            }
        }
        else
        {
            UpdateCoolTimeView(0f, coolTimes[1]);
        }

        if (globalData.MoneyCollectionRemainingSeconds > 0f)
        {
            float prevValue = globalData.MoneyCollectionRemainingSeconds;
            globalData.MoneyCollectionRemainingSeconds = Mathf.Max(0f, globalData.MoneyCollectionRemainingSeconds - dt);
            UpdateCoolTimeView(globalData.MoneyCollectionRemainingSeconds, coolTimes[2]);
            
            // 効果が終了したときにイベントを発火
            if (prevValue > 0f && globalData.MoneyCollectionRemainingSeconds <= 0f)
            {
                OnEffectEnded?.Invoke(RewardType.MoneyCollection);
            }
        }
        else
        {
            UpdateCoolTimeView(0f, coolTimes[2]);
        }
    }

    private void UpdateCoolTimeView(float remainingTime, CoolTimeView view)
    {
        if (remainingTime <= 0f)
        {
            view.gameObject.SetActive(false);
        }
        else
        {
            float rate = TotalDuration > 0f ? remainingTime / TotalDuration : 0f;
            view.gameObject.SetActive(true);
            view.Reload(remainingTime, rate);
        }
    }
}
