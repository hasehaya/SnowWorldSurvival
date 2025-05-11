using System.Collections;

using DG.Tweening;

using UnityEngine;

public class MoneyPile :ObjectPile
{
    [SerializeField, Tooltip("Maximum number of money objects allowed in the pile.")]
    private int maxPile = 80;

    [SerializeField, Range(1, 8), Tooltip("Multiplier for the collection rate based on the number of objects in the pile.")]
    private int collectMultiplier = 2;

    [SerializeField, Tooltip("The value of each money object in the pile.")]
    private int moneyValue = 1;

    private bool isCollectingMoney; // Flag to indicate if the collection process is ongoing.
    private int collectRate => objects.Count > 8 ? collectMultiplier : 1; // Collection rate based on current pile size.

    protected override void Start()
    {
        // Start method intentionally left blank to prevent altering the stack height for money objects.
    }

    protected override void Update()
    {
        base.Update();
        // もしグローバルな金収集がアクティブなら、積んでいる金をすべてプレイヤーの所持金に加算する。
        if (GameManager.Instance.GlobalData.IsMoneyCollectionActive && objects.Count > 0)
        {
            int totalMoney = objects.Count * moneyValue;
            // スタック内のすべての金オブジェクトを返却し、隠し金もクリア
            while (objects.Count > 0)
            {
                PoolManager.Instance.ReturnObject(objects.Pop());
            }
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

        var moneyObj = PoolManager.Instance.SpawnObject("Money");
        moneyObj.transform.position = objects.Peek().transform.position;

        moneyObj.transform.DOJump(player.transform.position + Vector3.up * 2, 3f, 1, 0.5f)
            .OnComplete(() => PoolManager.Instance.ReturnObject(moneyObj));

        AudioManager.Instance.PlaySFX(AudioID.Money);
    }

    /// <summary>
    /// プレイヤーがトリガーエリアに入った際に収集プロセスを開始する。
    /// </summary>
    protected override void OnPlayerEnter()
    {
        StartCoroutine(CollectMoney());
    }

    /// <summary>
    /// コルーチン。金のオブジェクトを3個ずつ収集して、プレイヤーの所持金を増加させる。
    /// グローバル収集がアクティブでない場合にのみ、オブジェクト単位での収集処理を実施する。
    /// </summary>
    IEnumerator CollectMoney()
    {
        isCollectingMoney = true;

        // オブジェクトが存在する限り収集処理を継続
        while (player != null && objects.Count > 0)
        {
            // 常に3個ずつ収集する
            for (int i = 0; i < 4; i++)
            {
                if (objects.Count == 0)
                {
                    isCollectingMoney = false;
                    break;
                }

                // オブジェクトを取り除き、直接所持金に加算
                var removedMoney = objects.Pop(); // Remove the top money object from the pile.
                PoolManager.Instance.ReturnObject(removedMoney);
                GameManager.Instance.AdjustMoney(moneyValue);
            }

            yield return null;
        }

        isCollectingMoney = false;
    }

    /// <summary>
    /// 金を追加する際、グローバル収集がアクティブであれば即座に加算し、
    /// そうでなければ従来通り処理（容量内なら直接加算、満杯なら追加しない）します。
    /// 引数で金額を指定できます。
    /// </summary>
    /// <param name="amount">追加する金額（デフォルト: 1）</param>
    public void AddMoney(int amount = 1)
    {
        if (GameManager.Instance.GlobalData.IsMoneyCollectionActive)
        {
            // 収集がアクティブなら、新たな金は即座に所持金に加算（スポーンしない）
            GameManager.Instance.AdjustMoney(amount * moneyValue);
        }
        else
        {
            // グローバル収集が無効の場合、指定された金額分のオブジェクトを追加
            for (int i = 0; i < amount; i++)
            {
                if (objects.Count < maxPile)
                {
                    var moneyObj = PoolManager.Instance.SpawnObject("Money"); // Spawn a new money object.
                    AddObject(moneyObj);
                }
                else
                {
                    // maxPileに達したらループを抜ける
                    break;
                }
            }
        }

        if (!isCollectingMoney && player != null)
            StartCoroutine(CollectMoney());
    }

    /// <summary>
    /// お金の価値を設定します。
    /// </summary>
    /// <param name="value">設定する価値</param>
    public void SetMoneyValue(int value)
    {
        moneyValue = value;
    }
}
