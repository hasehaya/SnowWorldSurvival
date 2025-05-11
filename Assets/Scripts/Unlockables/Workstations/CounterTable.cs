using System.Collections.Generic;

using UnityEngine;


public class CounterTable :Workstation
{
    public CustomerController CustomerPrefab;
    private Sprite orderIconSprite;

    private float baseInterval = 1.4f;
    private int sellPrice = 5;
    private int baseStack = 40;

    [SerializeField, Tooltip("Point where customers spawn.")]
    private Transform spawnPoint;

    [SerializeField, Tooltip("Point where customers exit after being served.")]
    private Transform despawnPoint;

    // 単一の QueuePoints を使用
    [SerializeField, Tooltip("Waypoints defining the customer queue.")]
    private Waypoints queuePoints;

    [SerializeField, Tooltip("Stack containing the food available for serving.")]
    private ObjectStack foodStack;

    [SerializeField, Tooltip("Pile where earned money is stored.")]
    private MoneyPile moneyPile;

    // 単一の顧客管理キュー
    private Queue<CustomerController> customers = new Queue<CustomerController>();

    private float spawnInterval; // 顧客生成間隔
    private float serveInterval; // 食事提供間隔
    private float spawnTimer;    // 顧客生成用タイマー
    private float serveTimer;    // 食事提供用タイマー

    // 最大顧客数は unlockLevel に応じて増加する
    private int maxCustomers => unlockLevel + 1;

    private const int moneyValue = 5;

    void Start()
    {
        // 初期化処理（必要に応じて seating などの初期化を行う）
    }

    void Update()
    {
        HandleCustomerSpawn();
        HandleFoodServing();
    }

    /// <summary>
    /// Updates stats such as spawn interval, serve interval, and food stack capacity.
    /// </summary>
    protected override void UpdateStats()
    {
        spawnInterval = (baseInterval * 3) - unlockLevel;
        serveInterval = baseInterval / (unlockLevel + 1);
        foodStack.MaxStack = baseStack + 10 * unlockLevel;
    }

    /// <summary>
    /// Sets the sell price for this counter table.
    /// </summary>
    /// <param name="price">The new sell price</param>
    public void SetSellPrice(int price)
    {
        sellPrice = price;
        moneyPile.SetMoneyValue(sellPrice / moneyValue);
    }

    public void SetOrderIconSprite(Sprite sprite)
    {
        orderIconSprite = sprite;
    }

    /// <summary>
    /// Spawns a new customer at the spawn point if conditions allow.
    /// </summary>
    void HandleCustomerSpawn()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval && customers.Count < maxCustomers)
        {
            spawnTimer = 0f;

            // 顧客生成
            var newCustomer = Instantiate(CustomerPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // カウンターテーブルに設定されたスプライトを顧客に設定
            if (orderIconSprite != null)
            {
                newCustomer.SetOrderIconSprite(orderIconSprite);
            }
            
            newCustomer.ExitPoint = despawnPoint.position;
            customers.Enqueue(newCustomer);

            // 単一の QueuePoints から待機位置を割り当て
            AssignQueuePoint(newCustomer, customers.Count - 1);
        }
    }

    /// <summary>
    /// Assigns a customer to a queue point based on their index in the queue.
    /// </summary>
    void AssignQueuePoint(CustomerController customer, int index)
    {
        if (queuePoints == null)
        {
            Debug.LogWarning("QueuePoints is not assigned.");
            return;
        }

        // 待機位置は、queuePoints.GetPoint(index) で取得
        Transform queuePoint = queuePoints.GetPoint(index);
        bool isFirst = (index == 0);
        customer.UpdateQueue(queuePoint, isFirst);
    }

    /// <summary>
    /// Handles food serving for the first customer in the queue.
    /// </summary>
    void HandleFoodServing()
    {
        if (customers.Count == 0 || !customers.Peek().HasOrder)
            return;

        serveTimer += Time.deltaTime;
        if (serveTimer >= serveInterval)
        {
            serveTimer = 0f;
            if (hasWorker && foodStack.Count > 0 && customers.Peek().OrderCount > 0)
            {
                var food = foodStack.RemoveFromStack();
                customers.Peek().FillOrder(food);
            moneyPile.AddMoney(moneyValue);
            }
            if (customers.Peek().OrderCount == 0)
            {
                var servedCustomer = customers.Dequeue();
                UpdateQueuePositions();
            }
        }
    }

    /// <summary>
    /// Updates the queue positions for all customers after one is served.
    /// </summary>
    void UpdateQueuePositions()
    {
        int index = 0;
        foreach (var customer in customers)
        {
            AssignQueuePoint(customer, index);
            index++;
        }
    }
}
