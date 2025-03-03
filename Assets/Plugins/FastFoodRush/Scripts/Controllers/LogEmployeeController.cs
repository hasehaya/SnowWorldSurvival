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
        // �ǉ��F��������s�E����BRestaurantManager �Őݒ肳��܂��B
        public int Row { get; set; }
        public int Column { get; set; }

        private Transform pointA;
        private Transform pointB;

        [SerializeField, Tooltip("�����IK�^�[�Q�b�g")]
        private Transform leftHandTarget;
        [SerializeField, Tooltip("�E���IK�^�[�Q�b�g")]
        private Transform rightHandTarget;

        // �]�ƈ����g�̃X�^�b�N�Ǘ��p�R���|�[�l���g
        [SerializeField, Tooltip("�]�ƈ��̃X�^�b�N�Ǘ��p�R���|�[�l���g")]
        private WobblingStack stack;
        public WobblingStack Stack => stack;

        // ��{�I�ȓ��쑬�x�Ɨe�ʁB�A�b�v�O���[�h�ɂ���ĉ��Z����܂��B
        [SerializeField, Tooltip("Log�]�ƈ��̊�{�ړ����x")]
        private float baseSpeed = 2.5f;
        [SerializeField, Tooltip("Log�]�ƈ��̊�{�X�^�b�N�e��")]
        private int baseCapacity = 3;

        // LogEmployee �̃X�^�b�N�e�ʂ� UpdateStats �ōX�V
        [SerializeField, Tooltip("Log�]�ƈ��̃X�^�b�N�̗e�� (�A�b�v�O���[�h�ő���)")]
        private int capacity = 3;
        public int Capacity => capacity;

        // ���O��a�����BLogStack �� ObjectStack �̃C���X�^���X
        [SerializeField, Tooltip("���O��a���� LogStack (ObjectStack) �̎Q��")]
        public ObjectStack logStack;

        private NavMeshAgent agent;
        private Animator animator;
        private Vector3 currentTarget;
        private float IK_Weight;

        // ���O�a�������ǂ����̃t���O
        private bool isTransferringLogs = false;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            // ����n�_���ݒ肳��Ă���ꍇ�A�����ړI�n�� A �n�_�ɐݒ�
            if (pointA != null)
            {
                currentTarget = pointA.position;
                agent.SetDestination(currentTarget);
            }
            // RestaurantManager �̃A�b�v�O���[�h�C�x���g�ɍw��
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
        /// �A�b�v�O���[�h�̌��ʂ𔽉f���āA�ړ����x�ƃX�^�b�N�e�ʂ��X�V����B
        /// </summary>
        void UpdateStats()
        {
            // EmployeeSpeed �A�b�v�O���[�h���x���ɉ������ړ����x�̉��Z�i��F0.1f�����Z�j
            float speedLevel = RestaurantManager.Instance.GetUpgradeLevel(Upgrade.EmployeeSpeed);
            agent.speed = baseSpeed + (speedLevel * 0.1f);

            // EmployeeCapacity �A�b�v�O���[�h���x���ɉ������X�^�b�N�e�ʂ̉��Z
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

        // AnimationEvent "OnStep" �p�i�����C�x���g�s�v�Ȃ������j
        public void OnStep()
        {
            // LogEmployee �͑����C�x���g�������s�v�Ȃ炱���͋������OK
        }

        /// <summary>
        /// �O������p�g���[���n�_��ݒ肷�郁�\�b�h
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
        /// �X�^�b�N�����t�ɂȂ����ہALogStack �փ��O��a���鏈���B
        /// �]������������Ə���t�F�[�Y�͕K�� A �n�_����ĊJ����B
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
