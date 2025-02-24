using System.Collections.Generic;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class TreeParent :Unlockable
    {
        [SerializeField] private Transform treeGrid;
        private List<GameObject> treeObjects = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();

            // treeGrid 配下のすべての Tree コンポーネントを取得
            var treeList = treeGrid.GetComponentsInChildren<Tree>();
            foreach (var tree in treeList)
            {
                treeObjects.Add(tree.gameObject);
            }
        }

        protected override void UpdateStats()
        {
            base.UpdateStats();

            foreach (var tree in treeObjects)
            {
                tree.SetActive(false);
            }

            var treeCount = 4 * unlockLevel;
            for (int i = 0; i < treeCount; i++)
            {
                var tree = treeObjects[i];
                tree.SetActive(true);
            }
        }

        /// <summary>
        /// グリッド上の木を、ローカル座標の X 値（列）ごとにグループ化し、
        /// 各列の先頭（Row1）と最終行（MaxRow）のワールド座標を返します。
        /// </summary>
        public List<PatrolPoints> GetPatrolPointsPerColumn()
        {
            List<PatrolPoints> patrolPointsList = new List<PatrolPoints>();

            // X 座標がほぼ同じものを同じ列とみなす（小数点第2位まで）
            Dictionary<float, List<Transform>> columns = new Dictionary<float, List<Transform>>();
            foreach (var treeObj in treeObjects)
            {
                Transform t = treeObj.transform;
                float xKey = Mathf.Round(t.localPosition.x * 100f) / 100f;
                if (!columns.ContainsKey(xKey))
                    columns[xKey] = new List<Transform>();
                columns[xKey].Add(t);
            }

            // 各列ごとに、Z 座標でソートして先頭と末尾を取得
            foreach (var kvp in columns)
            {
                var list = kvp.Value;
                list.Sort((a, b) => a.localPosition.z.CompareTo(b.localPosition.z));
                if (list.Count > 0)
                {
                    PatrolPoints points = new PatrolPoints();
                    points.pointA = list[0].position;                     // Row1
                    points.pointB = list[list.Count - 1].position;          // MaxRow
                    patrolPointsList.Add(points);
                }
            }

            return patrolPointsList;
        }

        /// <summary>
        /// 単一の列におけるパトロール地点（先頭と最終行）の情報を保持するクラス
        /// </summary>
        public class PatrolPoints
        {
            public Vector3 pointA;
            public Vector3 pointB;
        }
    }
}
