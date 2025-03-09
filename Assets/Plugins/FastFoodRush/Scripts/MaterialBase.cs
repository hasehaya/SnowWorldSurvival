using System.Collections;

using DG.Tweening;

using UnityEngine;

using Random = UnityEngine.Random;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// ������f�ނ̋��ʏ���������������N���X
    /// </summary>
    public abstract class MaterialBase :Interactable
    {
        // �z�u��̈ʒu���Ƃ��� Row, Column ��ێ��i�p�g���[�����ɗ��p�j
        public int Row;
        public int Column;

        [SerializeField] protected int materialHealth = 2;        // �f�ނ̗̑�
        [SerializeField] protected int resourceCount = 1;         // �̗͌������ɐ�������ő僊�\�[�X��
        [SerializeField] protected float decreaseInterval = 0.3f; // �̗͂�����Ԋu

        protected float timer = 0f;
        protected int initialHealth;
        protected Vector3 initialScale;
        protected bool isDepleted = false; // �f�ނ��g���ʂ�����Ă��邩

        protected virtual void Start()
        {
            initialHealth = materialHealth;
            GameObject model = GetMaterialModel();
            if (model != null)
                initialScale = model.transform.localScale;
        }

        /// <summary>
        /// �h���N���X�́A���g�̌����ڂ̃��f����Ԃ��悤��������
        /// </summary>
        protected abstract GameObject GetMaterialModel();

        /// <summary>
        /// �h���N���X�́A���\�[�X�����Ɏg�p����v�[���̃L�[��Ԃ��悤��������
        /// </summary>
        protected abstract string GetPoolKey();

        /// <summary>
        /// �Đ������ɕK�v�ȑҋ@���ԁi��F�؂Ȃ� regrowDelay�j��Ԃ�
        /// </summary>
        protected abstract float GetRegrowDelay();

        /// <summary>
        /// �Đ����ɂ����鎞�Ԃ�Ԃ�
        /// </summary>
        protected abstract float GetGrowthDuration();

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

        protected virtual void SpawnResourceForPlayer(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnResource(i, null);
            }
        }

        protected virtual void SpawnResourceForEmployee(int count, EmployeeController employee)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnResource(i, employee);
            }
        }

        /// <summary>
        /// �f�ނ��烊�\�[�X�i��F���O�Ȃǁj�𐶐����鋤�ʏ���
        /// </summary>
        /// <param name="index">�������̃I�t�Z�b�g�p�C���f�b�N�X</param>
        /// <param name="employee">
        /// �Ώۂ� Employee �̏ꍇ�͂��̎Q�ƁBnull �̏ꍇ�� Player �p�Ƃ��ď�������B
        /// </param>
        protected virtual void SpawnResource(int index, EmployeeController employee)
        {
            // ���������̃`�F�b�N�iPlayer/Employee ���Ƃ� Stack �̏�ԂƗe�ʂ��m�F�j
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

            string poolKey = GetPoolKey();
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
        /// �f�ނ̍Đ������B��莞�ԑҋ@��A�̗͂����Z�b�g���A�Ăѐ�������B
        /// </summary>
        protected virtual IEnumerator RegrowMaterial()
        {
            GameObject model = GetMaterialModel();
            if (model != null)
                model.SetActive(false);

            yield return new WaitForSeconds(GetRegrowDelay());

            materialHealth = initialHealth;
            timer = 0f;

            if (model != null)
            {
                model.transform.localScale = Vector3.zero;
                model.SetActive(true);
                model.transform.DOScale(initialScale, GetGrowthDuration());
            }
            isDepleted = false;
        }
    }
}
