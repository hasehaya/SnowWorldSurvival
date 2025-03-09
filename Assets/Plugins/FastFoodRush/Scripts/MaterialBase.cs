using System.Collections;

using DG.Tweening;

using UnityEngine;

using Random = UnityEngine.Random;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// あらゆる素材の共通処理を実装する基底クラス
    /// </summary>
    public abstract class MaterialBase :Interactable
    {
        // 配置上の位置情報として Row, Column を保持（パトロール等に利用）
        public int Row;
        public int Column;

        [SerializeField] protected int materialHealth = 2;        // 素材の体力
        [SerializeField] protected int resourceCount = 1;         // 体力減少時に生成する最大リソース数
        [SerializeField] protected float decreaseInterval = 0.3f; // 体力が減る間隔

        protected float timer = 0f;
        protected int initialHealth;
        protected Vector3 initialScale;
        protected bool isDepleted = false; // 素材が使い果たされているか

        protected virtual void Start()
        {
            initialHealth = materialHealth;
            GameObject model = GetMaterialModel();
            if (model != null)
                initialScale = model.transform.localScale;
        }

        /// <summary>
        /// 派生クラスは、自身の見た目のモデルを返すよう実装する
        /// </summary>
        protected abstract GameObject GetMaterialModel();

        /// <summary>
        /// 派生クラスは、リソース生成に使用するプールのキーを返すよう実装する
        /// </summary>
        protected abstract string GetPoolKey();

        /// <summary>
        /// 再生処理に必要な待機時間（例：木なら regrowDelay）を返す
        /// </summary>
        protected abstract float GetRegrowDelay();

        /// <summary>
        /// 再成長にかかる時間を返す
        /// </summary>
        protected abstract float GetGrowthDuration();

        private void OnTriggerStay(Collider other)
        {
            if (isDepleted)
                return;

            timer += Time.deltaTime;
            if (timer < decreaseInterval)
                return;

            // Player 対応
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
            // Employee 対応
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
        /// 素材からリソース（例：ログなど）を生成する共通処理
        /// </summary>
        /// <param name="index">生成時のオフセット用インデックス</param>
        /// <param name="employee">
        /// 対象が Employee の場合はその参照。null の場合は Player 用として処理する。
        /// </param>
        protected virtual void SpawnResource(int index, EmployeeController employee)
        {
            // 生成条件のチェック（Player/Employee ごとに Stack の状態と容量を確認）
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
        /// 素材の再生処理。一定時間待機後、体力をリセットし、再び成長する。
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
