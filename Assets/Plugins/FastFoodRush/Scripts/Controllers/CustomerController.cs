using System.Collections;

using UnityEngine;
using UnityEngine.AI;

namespace CryingSnow.FastFoodRush
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerController :MonoBehaviour
    {
        // どのキューに並ぶかを示すプロパティ（CounterTable から設定されます）
        public int QueueIndex { get; set; }

        [SerializeField, Tooltip("Max number of orders a customer can place")]
        private int maxOrder = 5;

        [SerializeField, Tooltip("OrderInfoのプレハブ")]
        private OrderInfo orderInfoPrefab;

        [SerializeField, Tooltip("Reference to the customer's stack for carrying items")]
        private WobblingStack stack;

        [SerializeField, Tooltip("Target transform for the left hand IK")]
        private Transform leftHandTarget;

        [SerializeField, Tooltip("Target transform for the right hand IK")]
        private Transform rightHandTarget;

        public Vector3 ExitPoint { get; set; } // The exit point where the customer will leave after eating
        public bool HasOrder { get; private set; } // Whether the customer has placed an order
        public int OrderCount { get; private set; } // The number of items in the customer's order

        private Animator animator;
        private NavMeshAgent agent;
        private LayerMask entranceLayer;
        private float IK_Weight;

        // 生成した OrderInfo インスタンスを保持する変数
        private OrderInfo currentOrderInfo;

        void Awake()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            entranceLayer = 1 << LayerMask.NameToLayer("Entrance");
        }

        IEnumerator CheckEntrance()
        {
            RaycastHit hit;
            while (!Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 0.5f, entranceLayer, QueryTriggerInteraction.Collide))
            {
                yield return null;
            }
            var doors = hit.transform.GetComponentsInChildren<Door>();
            foreach (var door in doors)
            {
                door.OpenDoor(transform);
            }
            yield return new WaitForSeconds(1f);
            foreach (var door in doors)
            {
                door.CloseDoor();
            }
        }

        void Start()
        {
            StartCoroutine(CheckEntrance());
        }

        void Update()
        {
            animator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.1f);
        }

        /// <summary>
        /// Updates the customer's queue position.
        /// This method is called externally (by CounterTable) with the correct queue waiting point.
        /// If the customer is first in line, the order placement process is started.
        /// </summary>
        /// <param name="queuePoint">The target waiting point for this customer.</param>
        /// <param name="isFirst">True if this customer is first in its queue.</param>
        public void UpdateQueue(Transform queuePoint, bool isFirst)
        {
            agent.SetDestination(queuePoint.position);
            if (isFirst)
                StartCoroutine(PlaceOrder());
        }

        /// <summary>
        /// Waits until arrival at the queue point, then randomly determines the order count and displays order info.
        /// </summary>
        IEnumerator PlaceOrder()
        {
            yield return new WaitUntil(() => HasArrived());
            OrderCount = Random.Range(1, maxOrder + 1);
            HasOrder = true;

            // OrderInfo プレハブを生成し表示
            currentOrderInfo = Instantiate(orderInfoPrefab, RestaurantManager.Instance.Canvas.transform);
            currentOrderInfo.ShowInfo(transform, OrderCount);
        }

        /// <summary>
        /// Fills the customer's order by reducing the order count, adding the delivered food to the customer's stack,
        /// and updating the order info display. Once all orders are filled, the customer leaves.
        /// </summary>
        /// <param name="food">The food item delivered.</param>
        public void FillOrder(Transform food)
        {
            OrderCount--;
            stack.AddToStack(food, StackType.Food);

            // 既に生成している OrderInfo インスタンスを更新
            if (currentOrderInfo != null)
            {
                currentOrderInfo.ShowInfo(transform, OrderCount);
            }

            if (OrderCount <= 0)
            {
                // 注文完了時は OrderInfo を非表示
                if (currentOrderInfo != null)
                {
                    currentOrderInfo.HideInfo();
                }
                animator.SetTrigger("Leave");
                agent.SetDestination(ExitPoint);
                StartCoroutine(WalkToExit());
            }
        }

        IEnumerator WalkToExit()
        {
            StartCoroutine(CheckEntrance());
            yield return new WaitUntil(() => HasArrived());
            Destroy(currentOrderInfo);
            Destroy(gameObject);
        }

        private bool HasArrived()
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                        return true;
                }
            }
            return false;
        }

        void OnAnimatorIK()
        {
            IK_Weight = Mathf.MoveTowards(IK_Weight, Mathf.Clamp01(stack.Height), Time.deltaTime * 3.5f);

            if (leftHandTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, IK_Weight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, IK_Weight);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }

            if (rightHandTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, IK_Weight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, IK_Weight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }
        }

        /// <summary>
        /// OnDestroy で、CustomerController が破棄される際に自身の stack に残っている Log を PoolManager に返却します。
        /// </summary>
        void OnDestroy()
        {
            if (stack != null)
            {
                // ここでは stack.Count プロパティと RemoveFromStack() メソッドを利用して、すべての Log を返却します。
                while (stack.Count > 0)
                {
                    Transform log = stack.RemoveFromStack();
                    if (log != null)
                    {
                        PoolManager.Instance.ReturnObject(log.gameObject);
                    }
                }
            }
        }
    }
}
