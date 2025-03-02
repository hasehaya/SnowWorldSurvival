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
        private Transform pointA;
        private Transform pointB;

        [SerializeField, Tooltip("左手のIKターゲット")]
        private Transform leftHandTarget;
        [SerializeField, Tooltip("右手のIKターゲット")]
        private Transform rightHandTarget;

        // 従業員自身のスタック管理（※WobblingStack など任意のスタック管理クラス）
        [SerializeField, Tooltip("従業員のスタック管理用コンポーネント")]
        private WobblingStack stack;
        public WobblingStack Stack => stack;

        // 従業員のスタックの容量
        [SerializeField, Tooltip("従業員のスタックの容量")]
        private int capacity = 3;
        public int Capacity => capacity;

        // ログを預ける先。LogStackは ObjectStack のインスタンスを指します。
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
            // 巡回地点が設定されている場合、初期目的地を設定
            if (pointA != null)
            {
                currentTarget = pointA.position;
                agent.SetDestination(currentTarget);
            }
        }

        void Update()
        {
            // 巡回フェーズ中のみ移動処理を行う
            if (!isTransferringLogs)
            {
                animator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.1f);

                // 巡回地点に到着しているか確認し、到着していれば次の目的地を設定
                if (HasArrived())
                {
                    if (currentTarget == pointA.position && pointB != null)
                        currentTarget = pointB.position;
                    else if (currentTarget == pointB.position && pointA != null)
                        currentTarget = pointA.position;

                    agent.SetDestination(currentTarget);
                }

                // 自身のスタックがいっぱいになった場合、ログ預け処理を開始
                // ※ここでは stack.Height を用いて現在の積み上がり数をチェックしています
                if (stack.Height >= Capacity && !isTransferringLogs)
                {
                    StartCoroutine(TransferLogsToLogStack());
                }
            }
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

        // AnimationEvent "OnStep" 用（足音イベントが不要なら空実装）
        public void OnStep()
        {
            // LogEmployee の場合、足音イベントの処理が不要ならここは空実装で問題ありません
        }

        // 巡回地点を外部から設定するメソッド
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
        /// 従業員のスタックがいっぱいになった際に、指定された LogStack (ObjectStack) へログを預ける処理。
        /// スタックが空になるか、LogStack が満杯になるまで転送し、転送完了後は巡回フェーズに戻る。
        /// </summary>
        private IEnumerator TransferLogsToLogStack()
        {
            isTransferringLogs = true;

            // LogStack の位置へ目的地を変更
            agent.SetDestination(logStack.transform.position);
            // 到着するまで待機
            yield return new WaitUntil(() => HasArrived());

            // 転送処理：自身のスタックから1個ずつ LogStack へ転送
            while (stack.Height > 0)
            {
                if (!logStack.IsFull)
                {
                    // 自身のスタックからログを1つ取り出す
                    var log = stack.RemoveFromStack();
                    // ObjectStack の AddToStack は GameObject を引数に取るため、log.gameObject を渡す
                    logStack.AddToStack(log.gameObject);
                    // 転送間隔をシミュレーションするための短い待機
                    yield return new WaitForSeconds(0.03f);
                }
                else
                {
                    // LogStack が満杯の場合は転送を中断
                    break;
                }
            }

            // 転送が完了したら、再び巡回フェーズへ戻る
            agent.SetDestination(currentTarget);
            isTransferringLogs = false;
        }
    }
}
