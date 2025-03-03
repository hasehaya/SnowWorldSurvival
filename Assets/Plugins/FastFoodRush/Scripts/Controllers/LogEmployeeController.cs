using System.Collections;

using DG.Tweening;

using UnityEngine;
using UnityEngine.AI;

namespace CryingSnow.FastFoodRush
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class LogEmployeeController :MonoBehaviour
    {
        // 追加：所属する行・列情報。RestaurantManager で設定されます。
        public int Row { get; set; }
        public int Column { get; set; }

        private Transform pointA;
        private Transform pointB;

        [SerializeField, Tooltip("左手のIKターゲット")]
        private Transform leftHandTarget;
        [SerializeField, Tooltip("右手のIKターゲット")]
        private Transform rightHandTarget;

        // 従業員自身のスタック管理用コンポーネント
        [SerializeField, Tooltip("従業員のスタック管理用コンポーネント")]
        private WobblingStack stack;
        public WobblingStack Stack => stack;

        // 基本的な動作速度と容量。アップグレードによって加算されます。
        [SerializeField, Tooltip("Log従業員の基本移動速度")]
        private float baseSpeed = 2.5f;
        [SerializeField, Tooltip("Log従業員の基本スタック容量")]
        private int baseCapacity = 3;

        // LogEmployee のスタック容量は UpdateStats で更新
        [SerializeField, Tooltip("Log従業員のスタックの容量 (アップグレードで増加)")]
        private int capacity = 3;
        public int Capacity => capacity;

        // ログを預ける先。LogStack は ObjectStack のインスタンス
        [SerializeField, Tooltip("ログを預ける LogStack (ObjectStack) の参照")]
        public ObjectStack logStack;

        private NavMeshAgent agent;
        private Animator animator;
        private Vector3 currentTarget;
        private float IK_Weight;

        // ログ預け中かどうかのフラグ
        private bool isTransferringLogs = false;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            // 巡回地点が設定されている場合、初期目的地を A 地点に設定
            if (pointA != null)
            {
                currentTarget = pointA.position;
                agent.SetDestination(currentTarget);
            }
            // RestaurantManager のアップグレードイベントに購読
            RestaurantManager.Instance.OnUpgrade += UpdateStats;
            UpdateStats();
        }

        void Update()
        {
            if (!isTransferringLogs)
            {
                animator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.1f);

                if (HasArrived())
                {
                    if (currentTarget == pointA.position && pointB != null)
                        currentTarget = pointB.position;
                    else if (currentTarget == pointB.position && pointA != null)
                        currentTarget = pointA.position;

                    agent.SetDestination(currentTarget);
                }

                if (stack.Height >= Capacity && !isTransferringLogs)
                {
                    StartCoroutine(TransferLogsToLogStack());
                }
            }
        }

        /// <summary>
        /// アップグレードの効果を反映して、移動速度とスタック容量を更新する。
        /// </summary>
        void UpdateStats()
        {
            // EmployeeSpeed アップグレードレベルに応じた移動速度の加算（例：0.1fずつ加算）
            float speedLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.EmployeeSpeed);
            agent.speed = baseSpeed + (speedLevel * 0.1f);

            // EmployeeCapacity アップグレードレベルに応じたスタック容量の加算
            int capacityLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.EmployeeCapacity);
            capacity = baseCapacity + 3 * capacityLevel;
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
            IK_Weight = Mathf.MoveTowards(IK_Weight, 1f, Time.deltaTime * 3.5f);
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

        // AnimationEvent "OnStep" 用（足音イベント不要なら空実装）
        public void OnStep()
        {
            // LogEmployee は足音イベント処理が不要ならここは空実装でOK
        }

        /// <summary>
        /// 外部からパトロール地点を設定するメソッド
        /// </summary>
        public void SetPatrolPoints(Transform a, Transform b)
        {
            pointA = a;
            pointB = b;
            if (pointA != null)
            {
                currentTarget = pointA.position;
                agent.SetDestination(currentTarget);
            }
        }

        /// <summary>
        /// スタックが満杯になった際、LogStack へログを預ける処理。
        /// 転送が完了すると巡回フェーズは必ず A 地点から再開する。
        /// </summary>
        private IEnumerator TransferLogsToLogStack()
        {
            isTransferringLogs = true;

            agent.SetDestination(logStack.transform.position);
            yield return new WaitUntil(() => HasArrived());

            while (stack.Height > 0)
            {
                if (logStack.IsFull)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                var log = stack.RemoveFromStack();
                logStack.AddToStack(log.gameObject);
                yield return new WaitForSeconds(0.03f);
            }

            if (pointA != null)
            {
                currentTarget = pointA.position;
                agent.SetDestination(currentTarget);
            }
            isTransferringLogs = false;
        }
    }
}
