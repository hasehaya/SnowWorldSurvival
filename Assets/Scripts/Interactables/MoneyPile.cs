using System.Collections;

using DG.Tweening;

using UnityEngine;

public class MoneyPile :ObjectPile
{
    [SerializeField, Tooltip("Maximum number of money objects allowed in the pile.")]
    private int maxPile = 120;

    [SerializeField, Range(1, 8), Tooltip("Multiplier for the collection rate based on the number of objects in the pile.")]
    private int collectMultiplier = 2;

    private int hiddenMoney; // The number of money that are hidden when the pile is full.
    private bool isCollectingMoney; // Flag to indicate if the collection process is ongoing.
    private int collectRate => objects.Count > 8 ? collectMultiplier : 1; // Collection rate based on current pile size.

    protected override void Start()
    {
        // Start method intentionally left blank to prevent altering the stack height for money objects.
    }

    private void Update()
    {
        // もしグローバルな金収集がアクティブなら、積んでいる金をすべてプレイヤーの所持金に加算する。
        if (GameManager.Instance.GlobalData.IsMoneyCollectionActive && (objects.Count > 0 || hiddenMoney > 0))
        {
            int totalMoney = objects.Count + hiddenMoney;
            // スタック内のすべての金オブジェクトを返却し、隠し金もクリア
            while (objects.Count > 0)
            {
                PoolManager.Instance.ReturnObject(objects.Pop());
            }
            hiddenMoney = 0;
            GameManager.Instance.AdjustMoney(totalMoney);
        }
    }

    /// <summary>
    /// (非アクティブ時のみ利用) ドロップ処理。コレクション中でなければ何もしない。
    /// グローバル収集がアクティブの際は Update() で一括収集しているため、Drop() は実行されません。
    /// </summary>
    protected override void Drop()
    {
        if (!isCollectingMoney)
            return; // 収集中でなければ何もしない。

        // グローバルな収集が無効な場合のみ、従来通りアニメーション付きの金オブジェクトのドロップ処理を行う
        if (!GameManager.Instance.GlobalData.IsMoneyCollectionActive)
        {
            var moneyObj = PoolManager.Instance.SpawnObject("Money");
            moneyObj.transform.position = objects.Peek().transform.position;

            moneyObj.transform.DOJump(player.transform.position + Vector3.up * 2, 3f, 1, 0.5f)
                .OnComplete(() => PoolManager.Instance.ReturnObject(moneyObj));

            AudioManager.Instance.PlaySFX(AudioID.Money);
        }
    }

    /// <summary>
    /// プレイヤーがトリガーエリアに入った際に収集プロセスを開始する。
    /// </summary>
    protected override void OnPlayerEnter()
    {
        StartCoroutine(CollectMoney());
    }

    /// <summary>
    /// コルーチン。金のオブジェクトを順次収集して、プレイヤーの所持金を増加させる。
    /// グローバル収集がアクティブでない場合にのみ、オブジェクト単位での収集処理を実施する。
    /// </summary>
    IEnumerator CollectMoney()
    {
        isCollectingMoney = true;

        // まずは隠し金を加算
        GameManager.Instance.AdjustMoney(hiddenMoney);
        hiddenMoney = 0;

        // オブジェクトが存在する限り収集処理を継続
        while (player != null && objects.Count > 0)
        {
            for (int i = 0; i < collectRate; i++)
            {
                if (objects.Count == 0)
                {
                    isCollectingMoney = false;
                    break;
                }

                // オブジェクトを取り除き、直接所持金に加算
                objects.Pop();
                GameManager.Instance.AdjustMoney(1);
            }

            if (collectRate > 1)
                yield return null;
            else
                yield return new WaitForSeconds(0.03f);
        }

        isCollectingMoney = false;
    }

    /// <summary>
    /// 金を追加する際、グローバル収集がアクティブであれば即座に加算し、
    /// そうでなければ従来通り処理（容量内なら直接加算、満杯なら hiddenMoney に蓄える）します。
    /// また、金オブジェクトをスポーンさせずに、直接調整処理を行います。
    /// </summary>
    public void AddMoney()
    {
        if (GameManager.Instance.GlobalData.IsMoneyCollectionActive)
        {
            // 収集がアクティブなら、新たな金は即座に所持金に加算（スポーンしない）
            GameManager.Instance.AdjustMoney(1);
        }
        else
        {
            if (objects.Count < maxPile)
            {
                var moneyObj = PoolManager.Instance.SpawnObject("Money"); // Spawn a new money object.
                AddObject(moneyObj);
            }
            else
            {
                // 既に最大数に達している場合は hiddenMoney に加算
                hiddenMoney++;
            }
        }

        if (!isCollectingMoney && player != null)
            StartCoroutine(CollectMoney());
    }
}
