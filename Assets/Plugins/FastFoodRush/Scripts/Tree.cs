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
        [SerializeField] private int treeHealth = 3;                // �؂̗̑�
        [SerializeField] private int logCount = 2;                   // �̗͌������ɐ�������ő働�O��
        [SerializeField] private float decreaseInterval = 0.3f;      // �̗͂�����Ԋu
        [SerializeField] private GameObject treeModel;               // �؂̌����ڂ̃��f��
        [SerializeField] private float regrowDelay = 4f;             // �Đ��܂ł̑ҋ@����
        [SerializeField] private float growthDuration = 0.5f;        // �����ɂ����鎞��

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
            if (isFallen)
                return;

            timer += Time.deltaTime;
            if (timer < decreaseInterval)
                return;

            // Player�̏ꍇ
            if (other.CompareTag("Player"))
            {
                if (player == null)
                {
                    timer = 0f;
                    return;
                }

                int remainingCapacity = player.Capacity - player.Stack.Height;
                // �擾�\����0�̏ꍇ�̗͑͂����炳���A���O���������Ȃ�
                if (remainingCapacity > 0)
                {
                    // �Ώۂ̎c��擾�\���̏���܂Ń��O�𐶐�
                    int logsToSpawn = Mathf.Min(logCount, remainingCapacity);
                    treeHealth--;
                    SpawnLogsForPlayer(logsToSpawn);
                }
            }
            // LogEmployeeController�̏ꍇ
            else if (other.CompareTag("Employee"))
            {
                EmployeeController logEmployee = other.GetComponent<EmployeeController>();
                if (logEmployee != null)
                {
                    int remainingCapacity = logEmployee.Capacity - logEmployee.Stack.Height;
                    if (remainingCapacity > 0)
                    {
                        int logsToSpawn = Mathf.Min(logCount, remainingCapacity);
                        treeHealth--;
                        SpawnLogsForLogEmployee(logsToSpawn, logEmployee);
                    }
                }
            }

            if (treeHealth <= 0)
            {
                isFallen = true;
                StartCoroutine(RegrowTree());
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
        /// Player�p�F�w�肳�ꂽ���������O�𐶐�
        /// </summary>
        private void SpawnLogsForPlayer(int logsToSpawn)
        {
            for (int i = 0; i < logsToSpawn; i++)
            {
                SpawnLog(i);
            }
        }

        /// <summary>
        /// LogEmployeeController�p�F�w�肳�ꂽ���������O�𐶐�
        /// </summary>
        private void SpawnLogsForLogEmployee(int logsToSpawn, EmployeeController logEmployee)
        {
            for (int i = 0; i < logsToSpawn; i++)
            {
                SpawnLogForLogEmployee(i, logEmployee);
            }
        }

        // Player�p�̃��O��������
        private void SpawnLog(int index)
        {
            // ���������擪�Ŏ��{�Fplayer�����݂��AStack�̌^��None�܂���Log�ŁA���e�ʂɗ]�T�����邩�`�F�b�N
            if (player == null ||
                !(player.Stack.StackType == StackType.None || player.Stack.StackType == StackType.Log) ||
                player.Stack.Height >= player.Capacity)
            {
                return;
            }

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
                    // ���̎��_�ł͏�������������Ă���O��ŃX�^�b�N�ɒǉ�
                    player.Stack.AddToStack(log.transform, StackType.Log);
                });
        }

        // LogEmployeeController�p�̃��O��������
        private void SpawnLogForLogEmployee(int index, EmployeeController logEmployee)
        {
            // ���������擪�Ŏ��{�FlogEmployee�����݂��AStack�̌^��None�܂���Log�ŁA���e�ʂɗ]�T�����邩�`�F�b�N
            if (logEmployee == null ||
                !(logEmployee.Stack.StackType == StackType.None || logEmployee.Stack.StackType == StackType.Log) ||
                logEmployee.Stack.Height >= logEmployee.Capacity)
            {
                return;
            }

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
                    // ���̎��_�ł͏�������������Ă���O��ŃX�^�b�N�ɒǉ�
                    logEmployee.Stack.AddToStack(log.transform, StackType.Log);
                });
        }


        // �؂̍Đ������F�؂̃��f�����\���ɂ��A��莞�Ԍ�ɐ�������
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
