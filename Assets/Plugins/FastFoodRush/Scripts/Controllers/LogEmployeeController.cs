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

        [SerializeField, Tooltip("�����IK�^�[�Q�b�g")]
        private Transform leftHandTarget;
        [SerializeField, Tooltip("�E���IK�^�[�Q�b�g")]
        private Transform rightHandTarget;

        // �]�ƈ����g�̃X�^�b�N�Ǘ��i��WobblingStack �ȂǔC�ӂ̃X�^�b�N�Ǘ��N���X�j
        [SerializeField, Tooltip("�]�ƈ��̃X�^�b�N�Ǘ��p�R���|�[�l���g")]
        private WobblingStack stack;
        public WobblingStack Stack => stack;

        // �]�ƈ��̃X�^�b�N�̗e��
        [SerializeField, Tooltip("�]�ƈ��̃X�^�b�N�̗e��")]
        private int capacity = 3;
        public int Capacity => capacity;

        // ���O��a�����BLogStack�� ObjectStack �̃C���X�^���X���w���܂��B
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
            // ����n�_���ݒ肳��Ă���ꍇ�A�����ړI�n��ݒ�
            if (pointA != null)
            {
                currentTarget = pointA.position;
                agent.SetDestination(currentTarget);
            }
        }

        void Update()
        {
            // ����t�F�[�Y���݈̂ړ��������s��
            if (!isTransferringLogs)
            {
                animator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.1f);

                // ����n�_�ɓ������Ă��邩�m�F���A�������Ă���Ύ��̖ړI�n��ݒ�
                if (HasArrived())
                {
                    if (currentTarget == pointA.position && pointB != null)
                        currentTarget = pointB.position;
                    else if (currentTarget == pointB.position && pointA != null)
                        currentTarget = pointA.position;

                    agent.SetDestination(currentTarget);
                }

                // ���g�̃X�^�b�N�������ς��ɂȂ����ꍇ�A���O�a���������J�n
                // �������ł� stack.Height ��p���Č��݂̐ςݏオ�萔���`�F�b�N���Ă��܂�
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

        // AnimationEvent "OnStep" �p�i�����C�x���g���s�v�Ȃ������j
        public void OnStep()
        {
            // LogEmployee �̏ꍇ�A�����C�x���g�̏������s�v�Ȃ炱���͋�����Ŗ�肠��܂���
        }

        // ����n�_���O������ݒ肷�郁�\�b�h
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
        /// �]�ƈ��̃X�^�b�N�������ς��ɂȂ����ۂɁA�w�肳�ꂽ LogStack (ObjectStack) �փ��O��a���鏈���B
        /// �X�^�b�N����ɂȂ邩�ALogStack �����t�ɂȂ�܂œ]�����A�]��������͏���t�F�[�Y�ɖ߂�B
        /// </summary>
        private IEnumerator TransferLogsToLogStack()
        {
            isTransferringLogs = true;

            // LogStack �̈ʒu�֖ړI�n��ύX
            agent.SetDestination(logStack.transform.position);
            // ��������܂őҋ@
            yield return new WaitUntil(() => HasArrived());

            // �]�������F���g�̃X�^�b�N����1���� LogStack �֓]��
            while (stack.Height > 0)
            {
                if (!logStack.IsFull)
                {
                    // ���g�̃X�^�b�N���烍�O��1���o��
                    var log = stack.RemoveFromStack();
                    // ObjectStack �� AddToStack �� GameObject �������Ɏ�邽�߁Alog.gameObject ��n��
                    logStack.AddToStack(log.gameObject);
                    // �]���Ԋu���V�~�����[�V�������邽�߂̒Z���ҋ@
                    yield return new WaitForSeconds(0.03f);
                }
                else
                {
                    // LogStack �����t�̏ꍇ�͓]���𒆒f
                    break;
                }
            }

            // �]��������������A�Ăя���t�F�[�Y�֖߂�
            agent.SetDestination(currentTarget);
            isTransferringLogs = false;
        }
    }
}
