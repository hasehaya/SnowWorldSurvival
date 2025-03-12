using System.Collections;

using DG.Tweening;

using UnityEngine;

using Random = UnityEngine.Random;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// �ėp�I�ȑf�ރN���X�BLog �ȂǁA������f�ނ̐�����Đ��������܂Ƃ߂Ă��܂��B
    /// </summary>
    public class MaterialBase :Interactable
    {
        // �p�g���[����z�u�ɗ��p���邽�߂̍s�E��ԍ�
        public int Row;
        public int Column;

        [Header("�f�ރp�����[�^")]
        [SerializeField] protected int materialHealth = 2;         // �f�ނ̗̑�
        [SerializeField] protected int resourceCount = 1;          // �̗͌������ɐ�������ő僊�\�[�X��
        [SerializeField] protected float decreaseInterval = 0.45f;  // �̗͂�����Ԋu

        [Header("���f���E�Đ��ݒ�")]
        [SerializeField] protected GameObject materialModel;       // �f�ނ̌����ڂ̃��f��
        [SerializeField] protected float regrowDelay = 12f;           // �Đ��܂ł̑ҋ@����
        [SerializeField] protected float growthDuration = 0.5f;      // �����ɂ����鎞��

        [Header("�v�[���ݒ�")]
        [SerializeField] protected string poolKey = "Log";         // �������郊�\�[�X�̃v�[���L�[

        protected float timer = 0f;
        protected int initialHealth;
        protected Vector3 initialScale;
        protected bool isDepleted = false; // �f�ނ��g���ʂ����ꂽ���ǂ���

        void Start()
        {
            initialHealth = materialHealth;
            if (materialModel != null)
            {
                initialScale = materialModel.transform.localScale;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isDepleted)
                return;

            timer += Time.deltaTime;
            if (timer < decreaseInterval)
                return;

            // Player �Ή�
            if (other.CompareTag("Player"))
            {
                if (player == null)
                {
                    timer = 0f;
                    return;
                }
                int remainingCapacity = player.Capacity - player.Stack.Height;
                if (remainingCapacity > 0)
                {
                    int spawnCount = Mathf.Min(resourceCount, remainingCapacity);
                    materialHealth--;
                    SpawnResourceForPlayer(spawnCount);
                }
            }
            // Employee �Ή�
            else if (other.CompareTag("Employee"))
            {
                EmployeeController employee = other.GetComponent<EmployeeController>();
                if (employee != null)
                {
                    int remainingCapacity = employee.Capacity - employee.Stack.Height;
                    if (remainingCapacity > 0)
                    {
                        int spawnCount = Mathf.Min(resourceCount, remainingCapacity);
                        materialHealth--;
                        SpawnResourceForEmployee(spawnCount, employee);
                    }
                }
            }

            if (materialHealth <= 0)
            {
                isDepleted = true;
                StartCoroutine(RegrowMaterial());
            }
            timer = 0f;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("Employee"))
                timer = 0f;
        }

        /// <summary>
        /// Player �p�Ƀ��\�[�X�𐶐����܂��B
        /// </summary>
        void SpawnResourceForPlayer(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnResource(i, null);
            }
        }

        /// <summary>
        /// Employee �p�Ƀ��\�[�X�𐶐����܂��B
        /// </summary>
        void SpawnResourceForEmployee(int count, EmployeeController employee)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnResource(i, employee);
            }
        }

        /// <summary>
        /// ���ʂ̃��\�[�X���������BPoolManager �� poolKey �𗘗p���ă��\�[�X�𐶐����A�W�����v���o��� Stack �֒ǉ����܂��B
        /// </summary>
        void SpawnResource(int index, EmployeeController employee)
        {
            if (employee == null)
            {
                if (player == null ||
                    !(player.Stack.StackType == StackType.None || player.Stack.StackType == StackType.Log) ||
                    player.Stack.Height >= player.Capacity)
                {
                    return;
                }
            }
            else
            {
                if (!(employee.Stack.StackType == StackType.None || employee.Stack.StackType == StackType.Log) ||
                    employee.Stack.Height >= employee.Capacity)
                {
                    return;
                }
            }

            var resource = PoolManager.Instance.SpawnObject(poolKey);
            Vector3 startPos = transform.position + Vector3.up * index;
            resource.transform.position = startPos;

            Vector3 randomXZ = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            float randomDistance = Random.Range(0.5f, 1f);
            Vector3 targetPos = startPos + randomXZ * randomDistance + Vector3.up * 5;

            Sequence seq = DOTween.Sequence()
                .Append(resource.transform.DOJump(targetPos, 2f, 1, 0.5f))
                .OnComplete(() =>
                {
                    if (employee == null)
                    {
                        player.Stack.AddToStack(resource.transform, StackType.Log);
                    }
                    else
                    {
                        employee.Stack.AddToStack(resource.transform, StackType.Log);
                    }
                });
        }

        /// <summary>
        /// �f�ނ̍Đ������B��莞�ԑҋ@��A�̗͂����Z�b�g���Ăѐ��������܂��B
        /// </summary>
        IEnumerator RegrowMaterial()
        {
            if (materialModel != null)
                materialModel.SetActive(false);

            yield return new WaitForSeconds(regrowDelay);

            materialHealth = initialHealth;
            timer = 0f;

            if (materialModel != null)
            {
                materialModel.transform.localScale = Vector3.zero;
                materialModel.SetActive(true);
                materialModel.transform.DOScale(initialScale, growthDuration);
            }
            isDepleted = false;
        }
    }
}
