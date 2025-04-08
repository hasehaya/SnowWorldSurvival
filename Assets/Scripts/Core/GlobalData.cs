using System;

[Serializable]
public class GlobalData
{
    public bool IsAdRemoved { get; set; }

    // �c��b���𒼐ڐݒ�E�ۑ����A��Ԃ̓v���p�e�B�Ŕ���
    public float PlayerSpeedRemainingSeconds { get; set; }
    public float PlayerCapacityRemainingSeconds { get; set; }
    public float MoneyCollectionRemainingSeconds { get; set; }

    // �c��b����0���傫����Ίe���ʂ͗L���Ɣ���
    public bool IsPlayerSpeedActive => PlayerSpeedRemainingSeconds > 0f;
    public bool IsPlayerCapacityActive => PlayerCapacityRemainingSeconds > 0f;
    public bool IsMoneyCollectionActive => MoneyCollectionRemainingSeconds > 0f;

    // �Ō�ɍX�V�i�܂��͈ꎞ��~���O�j�̎�����ۑ����邽�߂̃v���p�e�B
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
