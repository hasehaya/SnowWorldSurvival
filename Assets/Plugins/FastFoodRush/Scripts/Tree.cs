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
        [SerializeField] private int treeHealth = 3;                // 木の体力
        [SerializeField] private int logCount = 2;                   // 体力減少時に生成する最大ログ数
        [SerializeField] private float decreaseInterval = 0.3f;      // 体力が減る間隔
        [SerializeField] private GameObject treeModel;               // 木の見た目のモデル
        [SerializeField] private float regrowDelay = 4f;             // 再生までの待機時間
        [SerializeField] private float growthDuration = 0.5f;        // 成長にかかる時間

        private float timer = 0f;
        private int initialHealth;
        private Vector3 initialScale;
        private bool isFallen = false;  // 木が倒れているかどうか

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

            // Playerの場合
            if (other.CompareTag("Player"))
            {
                if (player == null)
                {
                    timer = 0f;
                    return;
                }

                int remainingCapacity = player.Capacity - player.Stack.Height;
                // 取得可能数が0の場合は体力を減らさず、ログも生成しない
                if (remainingCapacity > 0)
                {
                    // 対象の残り取得可能数の上限までログを生成
                    int logsToSpawn = Mathf.Min(logCount, remainingCapacity);
                    treeHealth--;
                    SpawnLogsForPlayer(logsToSpawn);
                }
            }
            // LogEmployeeControllerの場合
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
        /// Player用：指定された数だけログを生成
        /// </summary>
        private void SpawnLogsForPlayer(int logsToSpawn)
        {
            for (int i = 0; i < logsToSpawn; i++)
            {
                SpawnLog(i);
            }
        }

        /// <summary>
        /// LogEmployeeController用：指定された数だけログを生成
        /// </summary>
        private void SpawnLogsForLogEmployee(int logsToSpawn, EmployeeController logEmployee)
        {
            for (int i = 0; i < logsToSpawn; i++)
            {
                SpawnLogForLogEmployee(i, logEmployee);
            }
        }

        // Player用のログ生成処理
        private void SpawnLog(int index)
        {
            // 条件分岐を先頭で実施：playerが存在し、Stackの型がNoneまたはLogで、かつ容量に余裕があるかチェック
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
                    // この時点では条件が満たされている前提でスタックに追加
                    player.Stack.AddToStack(log.transform, StackType.Log);
                });
        }

        // LogEmployeeController用のログ生成処理
        private void SpawnLogForLogEmployee(int index, EmployeeController logEmployee)
        {
            // 条件分岐を先頭で実施：logEmployeeが存在し、Stackの型がNoneまたはLogで、かつ容量に余裕があるかチェック
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
                    // この時点では条件が満たされている前提でスタックに追加
                    logEmployee.Stack.AddToStack(log.transform, StackType.Log);
                });
        }


        // 木の再生処理：木のモデルを非表示にし、一定時間後に成長する
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
