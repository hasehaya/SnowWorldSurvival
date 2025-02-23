using System.Collections;

using DG.Tweening;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class Tree :Interactable
    {
        [SerializeField] private int treeHealth = 3;                // 現在の体力
        [SerializeField] private int logCount = 3;                  // 体力減少時に生成するログ数
        [SerializeField] private float decreaseInterval = 0.3f;     // 体力が減る間隔
        [SerializeField] private GameObject treeModel;              // 木の見た目のモデル
        [SerializeField] private float regrowDelay = 4f;            // 再生までの待機時間
        [SerializeField] private float growthDuration = 0.5f;         // 成長にかかる時間

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
            if (other.CompareTag("Player") && !isFallen)
            {
                timer += Time.deltaTime;
                if (timer >= decreaseInterval)
                {
                    treeHealth--;
                    SpawnLogs();

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
            if (other.CompareTag("Player"))
            {
                timer = 0f;
            }
        }

        // ログを生成してジャンプアニメーションを実行
        private void SpawnLogs()
        {
            for (int i = 0; i < logCount; i++)
            {
                SpawnLog(i);
            }
        }

        // ログを生成し、ランダム方向へジャンプ後、プレイヤーへジャンプして回収
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
                    if (player.Stack.StackType == StackType.None || player.Stack.StackType == StackType.Log)
                    {
                        if (player.Stack.Height < player.Capacity)
                        {
                            player.Stack.AddToStack(log.transform, StackType.Log);
                        }
                    }
                });
        }

        // 木の再生処理：木のモデルを非表示にし、一定時間後に小さいスケールから成長
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
