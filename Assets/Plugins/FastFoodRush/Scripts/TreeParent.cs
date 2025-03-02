using System.Collections.Generic;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class TreeParent :Unlockable
    {
        [SerializeField] private Transform treeGrid;
        private List<GameObject> treeObjects = new List<GameObject>();
        private bool isInitialized = false; // 初期化済みかのフラグ

        protected override void Awake()
        {
            base.Awake();
            InitializeTreeObjects();
        }

        /// <summary>
        /// ツリーオブジェクトの初期化（遅延初期化にも対応）
        /// </summary>
        private void InitializeTreeObjects()
        {
            if (isInitialized)
                return;

            // treeGrid 配下のすべての Tree コンポーネントを取得
            var treeList = treeGrid.GetComponentsInChildren<Tree>();
            foreach (var tree in treeList)
            {
                treeObjects.Add(tree.gameObject);
            }
            isInitialized = true;
        }

        protected override void UpdateStats()
        {
            base.UpdateStats();

            foreach (var tree in treeObjects)
            {
                tree.SetActive(false);
            }

            var treeCount = 4 * unlockLevel;
            for (int i = 0; i < treeCount && i < treeObjects.Count; i++)
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
            // 遅延初期化：まだ初期化されていなければ実施
            if (!isInitialized)
            {
                InitializeTreeObjects();
            }

            List<PatrolPoints> patrolPointsList = new List<PatrolPoints>();

            // Column プロパティでグループ化する（キーは整数）、Tree コンポーネントでグループ化
            Dictionary<int, List<Tree>> columns = new Dictionary<int, List<Tree>>();
            foreach (var treeObj in treeObjects)
            {
                // アクティブな木のみ対象とする
                if (!treeObj.activeInHierarchy)
                    continue;

                Tree treeComponent = treeObj.GetComponent<Tree>();
                if (treeComponent == null)
                    continue;

                int column = treeComponent.Column;
                if (!columns.ContainsKey(column))
                    columns[column] = new List<Tree>();

                columns[column].Add(treeComponent);
            }

            // 各列ごとに、Row プロパティでソートして先頭と末尾を取得
            foreach (var kvp in columns)
            {
                var treeList = kvp.Value;
                if (treeList.Count == 0)
                    continue;

                treeList.Sort((a, b) => a.Row.CompareTo(b.Row));

                PatrolPoints points = new PatrolPoints
                {
                    pointA = treeList[0].transform.position,                  // 最小の Row の木の位置
                    pointB = treeList[treeList.Count - 1].transform.position    // 最大の Row の木の位置
                };
                patrolPointsList.Add(points);
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
