using System;

[Serializable]
public class GlobalData
{
    public bool IsAdRemoved { get; set; }

    // 残り秒数を直接設定・保存し、状態はプロパティで判定
    public float PlayerSpeedRemainingSeconds { get; set; }
    public float PlayerCapacityRemainingSeconds { get; set; }
    public float MoneyCollectionRemainingSeconds { get; set; }

    // 残り秒数が0より大きければ各効果は有効と判定
    public bool IsPlayerSpeedActive => PlayerSpeedRemainingSeconds > 0f;
    public bool IsPlayerCapacityActive => PlayerCapacityRemainingSeconds > 0f;
    public bool IsMoneyCollectionActive => MoneyCollectionRemainingSeconds > 0f;

    // 最後に更新（または一時停止直前）の時刻を保存するためのプロパティ
    public DateTime LastUpdateTime { get; set; }

    public GlobalData()
    {
        IsAdRemoved = false;
        PlayerSpeedRemainingSeconds = 0f;
        PlayerCapacityRemainingSeconds = 0f;
        MoneyCollectionRemainingSeconds = 0f;
        LastUpdateTime = DateTime.Now;
    }
}
