using System.Collections;

using DG.Tweening;

using UnityEngine;

using Random = UnityEngine.Random;

namespace CryingSnow.FastFoodRush
{
    public class Tree :Interactable
    {
        public int Row;
        public int Column;
        [SerializeField] private int treeHealth = 3;                // ���݂̗̑�
        [SerializeField] private int logCount = 3;                  // �̗͌������ɐ������郍�O��
        [SerializeField] private float decreaseInterval = 0.3f;     // �̗͂�����Ԋu
        [SerializeField] private GameObject treeModel;              // �؂̌����ڂ̃��f��
        [SerializeField] private float regrowDelay = 4f;            // �Đ��܂ł̑ҋ@����
        [SerializeField] private float growthDuration = 0.5f;         // �����ɂ����鎞��

        private float timer = 0f;
        private int initialHealth;
        private Vector3 initialScale;
        private bool isFallen = false;  // �؂��|��Ă��邩�ǂ���

        private void Start()
        {
            initialHealth = treeHealth;
            if (treeModel != null)
                initialScale = treeModel.transform.localScale;
        }

        private void OnTriggerStay(Collider other)
        {
            // Player�̏ꍇ�̏���
            if (other.CompareTag("Player") && !isFallen)
            {
                timer += Time.deltaTime;
                if (timer >= decreaseInterval)
                {
                    treeHealth--;
                    SpawnLogs();  // Player�p�̃��O����

                    if (treeHealth <= 0)
                    {
                        isFallen = true;
                        StartCoroutine(RegrowTree());
                    }
                    timer = 0f;
                }
            }
            // Employee�̏ꍇ�́ALogEmployeeController �𗘗p���ă��O�����W
            else if (other.CompareTag("Employee") && !isFallen)
            {
                timer += Time.deltaTime;
                if (timer >= decreaseInterval)
                {
                    treeHealth--;
                    SpawnLogsForLogEmployee(other);  // LogEmployeeController �p�̃��O����

                    if (treeHealth <= 0)
                    {
                        isFallen = true;
                        StartCoroutine(RegrowTree());
                    }
                    timer = 0f;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("Employee"))
            {
                timer = 0f;
            }
        }

        // Player�p�F���O�𐶐����ăW�����v�A�j���[�V���������s
        private void SpawnLogs()
        {
            for (int i = 0; i < logCount; i++)
            {
                SpawnLog(i);
            }
        }

        // Player�p�̃��O��������
        private void SpawnLog(int index)
        {
            var log = PoolManager.Instance.SpawnObject("Log");
            Vector3 startPos = transform.position + Vector3.up * index;
            log.transform.position = startPos;

            Vector3 randomXZ = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            float randomDistance = Random.Range(0.5f, 1f);
            Vector3 firstJumpTarget = startPos + randomXZ * randomDistance + Vector3.up * 5;

            Sequence seq = DOTween.Sequence()
                .Append(log.transform.DOJump(firstJumpTarget, 2f, 1, 0.5f))
                .OnComplete(() =>
                {
                    // player��Interactable���Ŏ擾�ł���O��
                    if (player != null && (player.Stack.StackType == StackType.None || player.Stack.StackType == StackType.Log))
                    {
                        if (player.Stack.Height < player.Capacity)
                        {
                            player.Stack.AddToStack(log.transform, StackType.Log);
                        }
                    }
                });
        }

        // LogEmployeeController �p�̃��O��������
        private void SpawnLogsForLogEmployee(Collider employeeCollider)
        {
            // LogEmployeeController ���擾
            LogEmployeeController logEmployee = employeeCollider.GetComponent<LogEmployeeController>();
            if (logEmployee == null)
                return;

            for (int i = 0; i < logCount; i++)
            {
                SpawnLogForLogEmployee(i, logEmployee);
            }
        }

        // LogEmployeeController �p�̌ʃ��O��������
        private void SpawnLogForLogEmployee(int index, LogEmployeeController logEmployee)
        {
            var log = PoolManager.Instance.SpawnObject("Log");
            Vector3 startPos = transform.position + Vector3.up * index;
            log.transform.position = startPos;

            Vector3 randomXZ = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            float randomDistance = Random.Range(0.5f, 1f);
            Vector3 firstJumpTarget = startPos + randomXZ * randomDistance + Vector3.up * 5;

            Sequence seq = DOTween.Sequence()
                .Append(log.transform.DOJump(firstJumpTarget, 2f, 1, 0.5f))
                .OnComplete(() =>
                {
                    // LogEmployeeController �� Stack �� Capacity �𗘗p���ă��O��ǉ�
                    if (logEmployee != null &&
                        (logEmployee.Stack.StackType == StackType.None || logEmployee.Stack.StackType == StackType.Log))
                    {
                        if (logEmployee.Stack.Height < logEmployee.Capacity)
                        {
                            logEmployee.Stack.AddToStack(log.transform, StackType.Log);
                        }
                    }
                });
        }

        // �؂̍Đ������F�؂̃��f�����\���ɂ��A��莞�Ԍ�ɏ������X�P�[�����琬��
        private IEnumerator RegrowTree()
        {
            if (treeModel != null)
                treeModel.SetActive(false);

            yield return new WaitForSeconds(regrowDelay);

            treeHealth = initialHealth;
            timer = 0f;
            if (treeModel != null)
            {
                treeModel.transform.localScale = Vector3.zero;
                treeModel.SetActive(true);
                treeModel.transform.DOScale(initialScale, growthDuration);
            }
            isFallen = false;
        }

        protected override void OnPlayerEnter() { }
        protected override void OnPlayerExit() { }
    }
}
