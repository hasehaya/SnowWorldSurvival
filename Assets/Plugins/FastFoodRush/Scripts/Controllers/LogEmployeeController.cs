using System.Collections;

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

        private NavMeshAgent agent;
        private Animator animator;
        private Vector3 currentTarget;
        private float IK_Weight;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            if (pointA != null)
            {
                currentTarget = pointA.position;
                agent.SetDestination(currentTarget);
            }
        }

        void Update()
        {
            animator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.1f);

            // �ړI�n�ɓ��������� A �� B �����݂ɐ؂�ւ���
            if (HasArrived())
            {
                if (currentTarget == pointA.position && pointB != null)
                    currentTarget = pointB.position;
                else if (currentTarget == pointB.position && pointA != null)
                    currentTarget = pointA.position;

                agent.SetDestination(currentTarget);
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

        // �ォ��O������p�g���[���n�_��ݒ�ł���悤�A�v���p�e�B���p��
        public void SetPatrolPoints(Transform a, Transform b)
        {
            pointA = a;
            pointB = b;
            // �����ʒu�� A �Ƃ���
            if (pointA != null)
            {
                currentTarget = pointA.position;
                agent.SetDestination(currentTarget);
            }
        }
    }
}
