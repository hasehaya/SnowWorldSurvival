using CryingSnow.FastFoodRush;

using UnityEngine;

public class Tree :MonoBehaviour
{
    [SerializeField] private int treeHealth = 5;      // ツリーの初期ヘルス
    [SerializeField] private int logCount = 3;        // ヘルスが1減るごとに生成するログ数
    [SerializeField] private float decreaseInterval = 3f; // ヘルスが減る間隔(秒)
    [SerializeField] private float upwardForce = 5f;  // ログを上に跳ね上げる力

    private float timer = 0f;

    private void OnTriggerStay(Collider other)
    {
        // プレイヤーがトリガー内にいる場合のみ処理
        if (other.CompareTag("Player"))
        {
            // トリガー内にいる間、経過時間を加算
            timer += Time.deltaTime;

            // 一定時間を超えたらヘルスを減らし、ログ生成
            if (timer >= decreaseInterval)
            {
                // ヘルスを1減らす
                treeHealth--;

                // ログを生成
                SpawnLogs();

                // ヘルスがなくなったら木を破壊
                if (treeHealth <= 0)
                {
                    Destroy(gameObject);
                }

                // タイマーをリセット
                timer = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // プレイヤーがトリガーから出たらタイマーをリセット(触れていない間は減らないようにする)
        if (other.CompareTag("Player"))
        {
            timer = 0f;
        }
    }

    /// <summary>
    /// logCount 分のログを生成して、上に跳ね上げる
    /// </summary>
    private void SpawnLogs()
    {
        for (int i = 0; i < logCount; i++)
        {
            // PoolManager を使っている場合の例 (なければ Instantiate でもOK)
            var log = PoolManager.Instance.SpawnObject("Log");

            // 少しずつ高さをずらして生成
            log.transform.position = transform.position + Vector3.up * (0.5f * i);

            var rb = log.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // プールされたオブジェクトのため、念のため速度をリセット
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // 上方向に瞬間的な力を加える => 跳ねてその後落下
                rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
            }
        }
    }
}
