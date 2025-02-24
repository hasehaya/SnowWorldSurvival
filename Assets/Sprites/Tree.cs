using DG.Tweening;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class Tree :Interactable
    {
        [SerializeField] private int treeHealth = 5;
        [SerializeField] private int logCount = 3;
        [SerializeField] private float decreaseInterval = 1f;
        private float timer = 0f;

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                timer += Time.deltaTime;
                if (timer >= decreaseInterval)
                {
                    treeHealth--;
                    SpawnLogs();

                    if (treeHealth <= 0)
                    {
                        Destroy(gameObject);
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

        // ログを生成し、ランダム方向へジャンプ後、1秒後にプレイヤーへジャンプして回収
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
                .Append(log.transform.DOJump(player.transform.position + Vector3.up * 2f, 3f, 1, 0.5f))
                .OnComplete(() => PoolManager.Instance.ReturnObject(log));
        }

        protected override void OnPlayerEnter() { }
        protected override void OnPlayerExit() { }
    }
}
