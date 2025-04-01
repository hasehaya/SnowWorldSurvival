using System.Collections;

using UnityEngine;


/// <summary>
/// プレイヤーが一定時間エリア内にいると広告視聴のポップアップを表示するクラス
/// </summary>
public class AdObject :Interactable
{
    [SerializeField, Tooltip("表示する広告ポップアップの GameObject")]
    private GameObject adPopup;

    [SerializeField]
    private RewardType rewardType = RewardType.None;

    private float timeToPopup = 1.5f;
    private Coroutine adCoroutine;

    private void Start()
    {
        CloseAdPopup();
        Debug.Assert(rewardType != RewardType.None);
    }

    /// <summary>
    /// プレイヤーがエリアに入ったとき、一定時間後にポップアップ表示を開始する
    /// </summary>
    protected override void OnPlayerEnter()
    {
        // 一定時間経過後に広告ポップアップを表示するコルーチンを開始
        adCoroutine = StartCoroutine(WaitForAdPopup());
    }

    /// <summary>
    /// プレイヤーがエリアを退出したとき、ポップアップ待機中のコルーチンを停止する
    /// </summary>
    protected override void OnPlayerExit()
    {
        if (adCoroutine != null)
        {
            StopCoroutine(adCoroutine);
            adCoroutine = null;
        }
    }

    /// <summary>
    /// 指定した時間待機し、プレイヤーがまだエリア内にいる場合に広告ポップアップを表示する
    /// </summary>
    IEnumerator WaitForAdPopup()
    {
        yield return new WaitForSeconds(timeToPopup);
        if (player != null)
        {
            ShowAdPopup();
        }
    }

    /// <summary>
    /// 広告視聴のポップアップを表示する
    /// </summary>
    void ShowAdPopup()
    {
        if (adPopup != null)
        {
            adPopup.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Ad Popup がアタッチされていません。");
        }
    }

    public void CloseAdPopup()
    {
        if (adPopup != null)
        {
            adPopup.SetActive(false);
        }
    }

    public void ShowAd()
    {
        AdMobReward.Instance.ShowAdMobReward(rewardType);
    }
}
