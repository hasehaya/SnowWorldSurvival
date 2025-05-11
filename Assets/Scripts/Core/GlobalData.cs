using System;

[Serializable]
public class GlobalData
{
    public bool IsAdRemoved;
    public int StageId;

    public float PlayerSpeedRemainingSeconds;
    public float PlayerCapacityRemainingSeconds;
    public float MoneyCollectionRemainingSeconds;

    // 全ステージの合計プレイ時間
    public float TotalElapsedTime;

    public bool IsPlayerSpeedActive => PlayerSpeedRemainingSeconds > 0f;
    public bool IsPlayerCapacityActive => PlayerCapacityRemainingSeconds > 0f;
    public bool IsMoneyCollectionActive => MoneyCollectionRemainingSeconds > 0f;
    public float SpeedUpRate()
    {
        if (StageId >= 3)
        {
            return 1.25f;
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
        if (StageId >= 6)
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
        TotalElapsedTime = 0f;
        LastUpdateTime = DateTime.Now;
    }
}
