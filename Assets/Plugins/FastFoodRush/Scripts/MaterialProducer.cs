using System.Collections;

using DG.Tweening;

using UnityEngine;

using Random = UnityEngine.Random;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// �ėp�I�ȑf�ރN���X�BLog �ȂǁA������f�ނ̐�����Đ��������܂Ƃ߂Ă��܂��B
    /// </summary>
    public class MaterialProducer :Interactable
    {
        // �p�g���[����z�u�ɗ��p���邽�߂̍s�E��ԍ�
        public int Row;
        public int Column;

        [Header("�f�ރp�����[�^")]
        [SerializeField] protected int materialHealth = 2;         // �f�ނ̗̑�
        [SerializeField] protected int resourceCount = 1;          // �̗͌������ɐ�������ő僊�\�[�X��
        [SerializeField] protected float decreaseInterval = 0.45f; // �̗͂�����Ԋu

        [Header("���f���E�Đ��ݒ�")]
        [SerializeField] protected GameObject materialModel;       // �f�ނ̌����ڂ̃��f��
        [SerializeField] protected float regrowDelay = 12f;        // �Đ��܂ł̑ҋ@����
        [SerializeField] protected float growthDuration = 0.5f;    // �����ɂ����鎞��

        [Header("�v�[���ݒ�")]
        [SerializeField] protected MaterialType materialType = MaterialType.Log;

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
                int remainingCapacity = player.Capacity - player.Stack.Count;
                if (remainingCapacity > 0)
                {
                    int spawnCount = Mathf.Min(resourceCount, remainingCapacity);
                    materialHealth--;

                    // �� HP �������ɉ��h�ꉉ�o��ǉ�
                    if (materialModel != null)
                    {
                        materialModel.transform.DOShakePosition(
                            duration: 0.2f,
                            strength: new Vector3(0.2f, 0f, 0f),
                            vibrato: 10,
                            randomness: 90,
                            snapping: false,
                            fadeOut: true
                        );
                    }

                    SpawnResourceForPlayer(spawnCount);
                }
            }
            // Employee �Ή�
            else if (other.CompareTag("Employee"))
            {
                EmployeeController employee = other.GetComponent<EmployeeController>();
                if (employee != null)
                {
                    int remainingCapacity = employee.Capacity - employee.Stack.Count;
                    if (remainingCapacity > 0)
                    {
                        int spawnCount = Mathf.Min(resourceCount, remainingCapacity);
                        materialHealth--;

                        // �� HP �������ɉ��h�ꉉ�o��ǉ�
                        if (materialModel != null)
                        {
                            materialModel.transform.DOShakePosition(
                                duration: 0.2f,
                                strength: new Vector3(0.2f, 0f, 0f),
                                vibrato: 10,
                                randomness: 90,
                                snapping: false,
                                fadeOut: true
                            );
                        }

                        SpawnResourceForEmployee(spawnCount, employee);
                    }
                }
            }

            // HP �� 0 �ȉ��ɂȂ����^�C�~���O�ŐU��������� RegrowMaterial
            if (materialHealth <= 0 && materialModel != null)
            {
                // Deplete �t���O���Z�b�g
                isDepleted = true;

                // �܂����h��A�j���[�V���������߂ĕt�^���āA�I����҂�
                //   �i���łɐU������������΁A�����l�ł� OK�B�h���ύX��������ΈႤ�l�ł��悢�j
                materialModel.transform.DOShakePosition(
                    duration: 0.2f,
                    strength: new Vector3(0.2f, 0f, 0f),
                    vibrato: 10,
                    randomness: 90,
                    snapping: false,
                    fadeOut: true
                )
                .OnComplete(() =>
                {
                    // �U��������ɍĐ��R���[�`�����N��
                    StartCoroutine(RegrowMaterial());
                });
            }

            timer = 0f;
        }


        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("Employee"))
            {
                timer = 0f;
            }
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
        /// ���ʂ̃��\�[�X���������BpoolKey (MaterialType) �𕶎���ɂ��� PoolManager �֓n���A�W�����v���o��� Stack �֒ǉ����܂��B
        /// </summary>
        void SpawnResource(int index, EmployeeController employee)
        {
            // ���v���C���[�܂��͏]�ƈ��� MaterialType �� None �� poolKey �ƈ�v���Ă��邩���m�F
            if (employee == null)
            {
                if (player == null ||
                    !(player.Stack.MaterialType == MaterialType.None || player.Stack.MaterialType == materialType) ||
                    player.Stack.Count >= player.Capacity)
                {
                    return;
                }

                VibrationManager.PatternVibration();
                AudioManager.Instance.PlaySFX(AudioID.Pop);
            }
            else
            {
                if (!(employee.Stack.MaterialType == MaterialType.None || employee.Stack.MaterialType == materialType) ||
                    employee.Stack.Count >= employee.Capacity)
                {
                    return;
                }
            }

            // PoolManager ���琶������ۂɁApoolKey �𕶎���֕ϊ�
            var resource = PoolManager.Instance.SpawnObject(materialType.ToString());
            Vector3 startPos = transform.position + Vector3.up * index;
            resource.transform.position = startPos;

            Vector3 randomXZ = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            float randomDistance = Random.Range(0.5f, 1f);
            Vector3 targetPos = startPos + randomXZ * randomDistance + Vector3.up * 5;

            if (employee == null)
            {
                player.Stack.AddToStack(resource.transform, materialType);
            }
            else
            {
                employee.Stack.AddToStack(resource.transform, materialType);
            }
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
