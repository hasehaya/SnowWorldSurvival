using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class Workstation :Unlockable
    {
        [SerializeField, Tooltip("The working spots where the worker performs tasks")]
        private WorkingSpot[] workingSpots;  // 配列で最大3つの WorkingSpot を登録

        [SerializeField, Tooltip("The worker objects assigned to this workstation")]
        private GameObject[] workers;        // 配列で最大3つの Worker を登録

        // Worker の解放レベル：1つ目は Level2、2つ目は Level4、3つ目は Level6
        private readonly int[] workerUnlockLevels = { 2, 4, 6 };

        /// <summary>
        /// ワーカーがいるかを判定します。
        /// unlockLevel が 2 以上ならワーカーが存在すると判定し、
        /// それ以外の場合は WorkingSpot の HasWorker プロパティを参照します。
        /// </summary>
        protected bool hasWorker
        {
            get
            {
                if (unlockLevel >= workerUnlockLevels[0])
                    return true;
                foreach (var ws in workingSpots)
                {
                    if (ws != null && ws.HasWorker)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// ワークステーションのアンロック処理。
        /// Worker と WorkingSpot を unlockLevel に応じて切り替えます。
        /// Worker は 1つ目は Level2、2つ目は Level4、3つ目は Level6 で有効になります。
        /// </summary>
        /// <param name="animate">アニメーションを再生する場合は true</param>
        public override void Unlock(bool animate = true)
        {
            base.Unlock(animate);

            // workers と workingSpots の配列長が一致している前提です
            for (int i = 0; i < workers.Length; i++)
            {
                if (workers[i] != null && i < workerUnlockLevels.Length)
                {
                    if (unlockLevel >= workerUnlockLevels[i])
                    {
                        // 該当ワーカーを解放：ワーカー表示、対応する WorkingSpot 非表示
                        workers[i].SetActive(true);
                        if (i < workingSpots.Length && workingSpots[i] != null)
                        {
                            workingSpots[i].gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        // 解放前の場合：ワーカー非表示、対応する WorkingSpot 表示
                        workers[i].SetActive(false);
                        if (i < workingSpots.Length && workingSpots[i] != null)
                        {
                            workingSpots[i].gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}
