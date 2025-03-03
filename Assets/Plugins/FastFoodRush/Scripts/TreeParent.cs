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

            isInitialized = true;

            // treeGrid 配下のすべての Tree コンポーネントを取得
            var treeList = treeGrid.GetComponentsInChildren<Tree>();
            treeObjects.Clear();
            foreach (var tree in treeList)
            {
                treeObjects.Add(tree.gameObject);
            }

            UpdateStats();
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

            // LogEmployeeController の巡回地点を更新
            UpdateEmployeePatrolPoints();
        }

        /// <summary>
        /// 指定した列番号に属するアクティブな木から、先頭（最小 Row）と末尾（最大 Row）の位置を取得します。
        /// 指定列に木が存在しなければ null を返します。
        /// </summary>
        public PatrolPoints GetPatrolPointsForColumn(int column)
        {
            if (!isInitialized)
                InitializeTreeObjects();

            List<Tree> treesInColumn = new List<Tree>();
            foreach (var treeObj in treeObjects)
            {
                if (!treeObj.activeInHierarchy)
                    continue;
                Tree treeComponent = treeObj.GetComponent<Tree>();
                if (treeComponent == null)
                    continue;
                if (treeComponent.Column == column)
                    treesInColumn.Add(treeComponent);
            }
            if (treesInColumn.Count == 0)
                return null;

            treesInColumn.Sort((a, b) => a.Row.CompareTo(b.Row));

            return new PatrolPoints
            {
                pointA = treesInColumn[0].transform.position,
                pointB = treesInColumn[treesInColumn.Count - 1].transform.position
            };
        }

        /// <summary>
        /// シーン内のすべての LogEmployeeController の巡回地点を、各従業員の所属列に合わせて更新します。
        /// 一時的に PatrolPoint オブジェクトを生成して更新しています（必要に応じて管理方法を変更してください）。
        /// </summary>
        private void UpdateEmployeePatrolPoints()
        {
            LogEmployeeController[] employees = FindObjectsOfType<LogEmployeeController>();
            foreach (var employee in employees)
            {
                int col = employee.Column; // 1～の値
                PatrolPoints patrol = GetPatrolPointsForColumn(col);
                if (patrol == null)
                {
                    Debug.LogWarning("指定された列 " + col + " にアクティブな木がありません。");
                    continue;
                }

                // 一時的な PatrolPoint オブジェクトを生成して更新
                GameObject tempA = new GameObject("TempPatrolPointA_Column" + col);
                tempA.transform.position = patrol.pointA;
                GameObject tempB = new GameObject("TempPatrolPointB_Column" + col);
                tempB.transform.position = patrol.pointB;
                employee.SetPatrolPoints(tempA.transform, tempB.transform);

                // ※必要に応じて、生成した一時オブジェクトは Destroy() するか、管理側で再利用してください。
            }
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
