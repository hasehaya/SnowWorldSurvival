using System;

[Serializable]
public class GlobalData
{
    public bool IsAdRemoved;
    public int StageId;

    public float PlayerSpeedRemainingSeconds;
    public float PlayerCapacityRemainingSeconds;
    public float MoneyCollectionRemainingSeconds;

    public bool IsPlayerSpeedActive => PlayerSpeedRemainingSeconds > 0f;
    public bool IsPlayerCapacityActive => PlayerCapacityRemainingSeconds > 0f;
    public bool IsMoneyCollectionActive => MoneyCollectionRemainingSeconds > 0f;
    public float SpeedUpRate()
    {
        if (StageId >= 6)
        {
            return 1.2f;
        }
        if (StageId >= 3)
        {
            return 1.1f;
        }
        return 1.0f;
    }

    public int CapacityUpCount()
    {
        if (StageId >= 5)
        {
            return 4;
        }
        if (StageId >= 2)
        {
            return 2;
        }
        return 0;
    }

    public float EarnedUpRate()
    {
        if (StageId >= 8)
        {
            return 1.5f;
        }
        if (StageId >= 7)
        {
            return 1.3f;
        }
        if (StageId >= 4)
        {
            return 1.15f;
        }
        return 1.0f;
    }

    public DateTime LastUpdateTime;

    public GlobalData()
    {
        IsAdRemoved = false;
        PlayerSpeedRemainingSeconds = 0f;
        PlayerCapacityRemainingSeconds = 0f;
        MoneyCollectionRemainingSeconds = 0f;
        LastUpdateTime = DateTime.Now;
    }
}
