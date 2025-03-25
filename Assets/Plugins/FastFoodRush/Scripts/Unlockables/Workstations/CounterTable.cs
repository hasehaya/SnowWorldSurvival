using System.Collections.Generic;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class CounterTable :Workstation
    {
        [SerializeField, Tooltip("Base time interval for customer spawn in seconds.")]
        private float baseInterval = 1.5f;

        [SerializeField, Tooltip("Base price of food served at the counter.")]
        private int sellPrice = 5;

        [SerializeField, Tooltip("Base stack capacity of the food stack.")]
        private int baseStack = 30;

        [SerializeField, Tooltip("Point where customers spawn.")]
        private Transform spawnPoint;

        [SerializeField, Tooltip("Point where customers exit after being served.")]
        private Transform despawnPoint;

        // �P��� QueuePoints ���g�p
        [SerializeField, Tooltip("Waypoints defining the customer queue.")]
        private Waypoints queuePoints;

        [SerializeField, Tooltip("Prefab for customer objects.")]
        private CustomerController customerPrefab;

        [SerializeField, Tooltip("Stack containing the food available for serving.")]
        private ObjectStack foodStack;

        [SerializeField, Tooltip("Pile where earned money is stored.")]
        private MoneyPile moneyPile;

        // �P��̌ڋq�Ǘ��L���[
        private Queue<CustomerController> customers = new Queue<CustomerController>();

        private float spawnInterval; // �ڋq�����Ԋu
        private float serveInterval; // �H���񋟊Ԋu
        private float spawnTimer;    // �ڋq�����p�^�C�}�[
        private float serveTimer;    // �H���񋟗p�^�C�}�[

        // �ő�ڋq���� unlockLevel �ɉ����đ�������
        private int maxCustomers => unlockLevel;

        void Start()
        {
            // �����������i�K�v�ɉ����� seating �Ȃǂ̏��������s���j
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
            serveInterval = baseInterval / unlockLevel;
            foodStack.MaxStack = baseStack + 10 * unlockLevel;
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

                // �ڋq����
                var newCustomer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
                newCustomer.ExitPoint = despawnPoint.position;
                customers.Enqueue(newCustomer);

                // �P��� QueuePoints ����ҋ@�ʒu�����蓖��
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

            // �ҋ@�ʒu�́AqueuePoints.GetPoint(index) �Ŏ擾
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
}
