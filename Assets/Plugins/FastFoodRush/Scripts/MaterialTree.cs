using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// MaterialBase を継承した木用の素材クラス
    /// </summary>
    public class MaterialTree :MaterialBase
    {
        [SerializeField] private GameObject treeModel;      // 木の見た目のモデル
        [SerializeField] private float regrowDelay = 12f;      // 再生待機時間
        [SerializeField] private float growthDuration = 0.5f; // 成長にかかる時間

        protected override GameObject GetMaterialModel()
        {
            return treeModel;
        }

        protected override string GetPoolKey()
        {
            // 生成するリソースのプールキー（例として "Log" を指定）
            return "Log";
        }

        protected override float GetRegrowDelay()
        {
            return regrowDelay;
        }

        protected override float GetGrowthDuration()
        {
            return growthDuration;
        }
    }
}
