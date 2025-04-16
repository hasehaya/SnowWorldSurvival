using UnityEngine;

public class AdManager :MonoBehaviour
{
    private static AdManager instance;
    public static AdManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AdManager");
                    instance = obj.AddComponent<AdManager>();
                }
            }
            return instance;
        }
    }
    [HideInInspector] public float CoolTime = 0f;

    private void Start()
    {
        AdMobReward.Instance.OnRewardReceived += OnRewardReceived;
    }

    private void OnDisable()
    {
        AdMobReward.Instance.OnRewardReceived -= OnRewardReceived;
    }

    private void Update()
    {
        if (CoolTime > 0f)
        {
            CoolTime -= Time.deltaTime;
        }
        else
        {
            CoolTime = 0f;
        }
    }

    private void OnRewardReceived(RewardType type)
    {
        if (GameManager.Instance.IsAdBlocked())
        {
            CoolTime = 60f;
        }
        else
        {
            CoolTime = 0f;
        }
    }
}
