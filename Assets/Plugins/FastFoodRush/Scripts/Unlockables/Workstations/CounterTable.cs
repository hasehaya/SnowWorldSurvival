using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class CounterTable :Workstation
    {
        [SerializeField, Tooltip("Base time interval for customer spawn in seconds.")]
        private float baseInterval = 1.5f;

        [SerializeField, Tooltip("Base price of food served at the counter.")]
        private int basePrice = 5;

        [SerializeField, Tooltip("Rate of price increase per profit upgrade.")]
        private float priceIncrementRate = 1.25f;

        [SerializeField, Tooltip("Base stack capacity of the food stack.")]
        private int baseStack = 30;

        [SerializeField, Tooltip("Point where customers spawn.")]
        private Transform spawnPoint;

        [SerializeField, Tooltip("Point where customers exit after being served.")]
        private Transform despawnPoint;

        // ������ QueuePoints ���g����悤�ɔz��ɕύX
        [SerializeField, Tooltip("Waypoints defining the customer queues for each working spot.")]
        private Waypoints[] queuePoints;

        [SerializeField, Tooltip("Prefab for customer objects.")]
        private CustomerController customerPrefab;

        [SerializeField, Tooltip("Stack containing the food available for serving.")]
        private ObjectStack foodStack;

        [SerializeField, Tooltip("Pile where earned money is stored.")]
        private MoneyPile moneyPile;

        // �ڋq�Ǘ��L���[
        private Queue<CustomerController> customers = new Queue<CustomerController>();

        // ���Ɋ��蓖�Ă� Queue (queuePoints �z��̃C���f�b�N�X) �����E���h���r���ōX�V
        private int nextQueueIndex = 0;

        private float spawnInterval; // �ڋq�����Ԋu
        private float serveInterval; // �H���񋟊Ԋu
        private int sellPrice;       // ���݂̐H�����i
        private float spawnTimer;    // �ڋq�����p�^�C�}�[
        private float serveTimer;    // �H���񋟗p�^�C�}�[

        // �ő�ڋq���� unlockLevel �ɉ����đ�������
        // �����ł͊�{�l 10 �ɑ΂��āAunlockLevel ���� 2 �l�ǉ������ł�
        private int maxCustomers => 10 + unlockLevel * 2;

        void Start()
        {
            // Initializes seatings or other initializations as needed.
        }

        void Update()
        {
            HandleCustomerSpawn();
            HandleFoodServing();
        }

        /// <summary>
        /// Updates stats such as spawn interval, serve interval, and stack capacity.
        /// </summary>
        protected override void UpdateStats()
        {
            spawnInterval = (baseInterval * 3) - unlockLevel;
            serveInterval = baseInterval / unlockLevel;
            foodStack.MaxStack = baseStack + 10 * unlockLevel;

            int profitLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.Profit);
            sellPrice = Mathf.RoundToInt(Mathf.Pow(priceIncrementRate, profitLevel) * basePrice);
        }

        /// <summary>
        /// Spawns a new customer at the spawn point if conditions allow.
        /// </summary>
        void HandleCustomerSpawn()
        {
            spawnTimer += Time.deltaTime;

            // �ő�ڋq���� unlockLevel �ɉ����đ���
            if (spawnTimer >= spawnInterval && customers.Count < maxCustomers)
            {
                spawnTimer = 0f;

                var newCustomer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
                newCustomer.ExitPoint = despawnPoint.position;

                newCustomer.QueueIndex = nextQueueIndex;
                nextQueueIndex = (nextQueueIndex + 1) % queuePoints.Length;

                customers.Enqueue(newCustomer);
                AssignQueuePoint(newCustomer);
            }
        }

        /// <summary>
        /// Assigns a customer to a specific queue point based on its QueueIndex and its order in that queue.
        /// </summary>
        void AssignQueuePoint(CustomerController customer)
        {
            if (queuePoints == null || queuePoints.Length == 0)
            {
                Debug.LogWarning("QueuePoints array is not assigned.");
                return;
            }
            int qIndex = customer.QueueIndex; // 0�` (queuePoints.Length - 1)
            int row = customers.Where(c => c.QueueIndex == qIndex).Count() - 1;
            Transform queuePoint = queuePoints[qIndex].GetPoint(row);
            bool isFirst = (row == 0);
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
                    CollectPayment();
                }
                if (customers.Peek().OrderCount == 0)
                {
                    var servedCustomer = customers.Dequeue();
                    UpdateQueuePositions();
                }
            }
        }

        void CollectPayment()
        {
            for (int i = 0; i < sellPrice; i++)
            {
                moneyPile.AddMoney();
            }
        }

        /// <summary>
        /// Updates the queue positions for all customers after a customer is served.
        /// </summary>
        void UpdateQueuePositions()
        {
            foreach (int qIndex in Enumerable.Range(0, queuePoints.Length))
            {
                var group = customers.Where(c => c.QueueIndex == qIndex).ToList();
                for (int i = 0; i < group.Count; i++)
                {
                    Transform queuePoint = queuePoints[qIndex].GetPoint(i);
                    bool isFirst = (i == 0);
                    group[i].UpdateQueue(queuePoint, isFirst);
                }
            }
        }
    }
}
