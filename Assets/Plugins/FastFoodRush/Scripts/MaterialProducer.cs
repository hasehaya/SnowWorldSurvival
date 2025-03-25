using System.Collections;

using DG.Tweening;

using UnityEngine;

using Random = UnityEngine.Random;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// 汎用的な素材クラス。Log など、あらゆる素材の生成や再生処理をまとめています。
    /// </summary>
    public class MaterialProducer :Interactable
    {
        // パトロールや配置に利用するための行・列番号
        public int Row;
        public int Column;

        [Header("素材パラメータ")]
        [SerializeField] protected int materialHealth = 2;         // 素材の体力
        [SerializeField] protected int resourceCount = 1;          // 体力減少時に生成する最大リソース数
        [SerializeField] protected float decreaseInterval = 0.45f; // 体力が減る間隔

        [Header("モデル・再生設定")]
        [SerializeField] protected GameObject materialModel;       // 素材の見た目のモデル
        [SerializeField] protected float regrowDelay = 12f;        // 再生までの待機時間
        [SerializeField] protected float growthDuration = 0.5f;    // 成長にかかる時間

        [Header("プール設定")]
        [SerializeField] protected MaterialType materialType = MaterialType.Log;

        protected float timer = 0f;
        protected int initialHealth;
        protected Vector3 initialScale;
        protected bool isDepleted = false; // 素材が使い果たされたかどうか

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

            // Player 対応
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

                    // ★ HP 減少時に横揺れ演出を追加
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
            // Employee 対応
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

                        // ★ HP 減少時に横揺れ演出を追加
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

            // HP が 0 以下になったタイミングで振動完了後に RegrowMaterial
            if (materialHealth <= 0 && materialModel != null)
            {
                // Deplete フラグをセット
                isDepleted = true;

                // まず横揺れアニメーションを改めて付与して、終了を待つ
                //   （すでに振動させたければ、同じ値でも OK。揺れを変更したければ違う値でもよい）
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
                    // 振動完了後に再生コルーチンを起動
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
        /// Player 用にリソースを生成します。
        /// </summary>
        void SpawnResourceForPlayer(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnResource(i, null);
            }
        }

        /// <summary>
        /// Employee 用にリソースを生成します。
        /// </summary>
        void SpawnResourceForEmployee(int count, EmployeeController employee)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnResource(i, employee);
            }
        }

        /// <summary>
        /// 共通のリソース生成処理。poolKey (MaterialType) を文字列にして PoolManager へ渡し、ジャンプ演出後に Stack へ追加します。
        /// </summary>
        void SpawnResource(int index, EmployeeController employee)
        {
            // ※プレイヤーまたは従業員の MaterialType が None か poolKey と一致しているかを確認
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

            // PoolManager から生成する際に、poolKey を文字列へ変換
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
        /// 素材の再生処理。一定時間待機後、体力をリセットし再び成長させます。
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
