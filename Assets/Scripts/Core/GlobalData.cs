using System;

[Serializable]
public class GlobalData
{
    public bool IsAdRemoved { get; set; }

    public GlobalData()
    {
        IsAdRemoved = false; // �����l�B�L�������폜�̏ꍇ�B
    }
}
