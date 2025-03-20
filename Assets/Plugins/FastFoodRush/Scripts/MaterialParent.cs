using System.Collections.Generic;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// 素材（MaterialBase）を管理するクラス
    /// </summary>
    public class MaterialParent :Unlockable
    {
        [SerializeField] private Transform materialGrid;
        private List<GameObject> materialObjects = new List<GameObject>();
        [SerializeField] MaterialType materialType;
        public MaterialType MaterialType => materialType;
        private bool isInitialized = false; // 初期化済みかのフラグ

        protected override void Awake()
        {
            base.Awake();
            InitializeMaterialObjects();
        }

        /// <summary>
        /// 素材オブジェクトの初期化（遅延初期化にも対応）
        /// </summary>
        private void InitializeMaterialObjects()
        {
            if (isInitialized)
                return;

            isInitialized = true;

            // materialGrid 配下のすべての MaterialBase コンポーネントを取得
            var materialList = materialGrid.GetComponentsInChildren<MaterialProducer>();
            materialObjects.Clear();
            foreach (var material in materialList)
            {
                materialObjects.Add(material.gameObject);
            }

            UpdateStats();
        }

        protected override void UpdateStats()
        {
            base.UpdateStats();

            // 全素材を一度非アクティブ化
            foreach (var obj in materialObjects)
            {
                obj.SetActive(false);
            }

            // unlockLevel に応じた素材数をアクティブ化（例：1列につき4個）
            int materialCount = 4 * unlockLevel;
            for (int i = 0; i < materialCount && i < materialObjects.Count; i++)
            {
                materialObjects[i].SetActive(true);
            }

            UpdateEmployeePatrolPoints();
        }

        /// <summary>
        /// 指定した列番号に属するアクティブな素材から、先頭（最小 Row）と末尾（最大 Row）の位置を取得
        /// </summary>
        public PatrolPoints GetPatrolPointsForColumn(int column)
        {
            if (!isInitialized)
                InitializeMaterialObjects();

            List<MaterialProducer> materialsInColumn = new List<MaterialProducer>();
            foreach (var obj in materialObjects)
            {
                if (!obj.activeInHierarchy)
                    continue;

                MaterialProducer material = obj.GetComponent<MaterialProducer>();
                if (material == null)
                    continue;

                if (material.Column == column)
                    materialsInColumn.Add(material);
            }

            if (materialsInColumn.Count == 0)
                return null;

            materialsInColumn.Sort((a, b) => a.Row.CompareTo(b.Row));

            return new PatrolPoints
            {
                pointA = materialsInColumn[0].transform.position,
                pointB = materialsInColumn[materialsInColumn.Count - 1].transform.position
            };
        }

        /// <summary>
        /// シーン内のすべての EmployeeController の巡回地点を、各従業員の所属列に合わせて更新します。
        /// 一時的に PatrolPoint オブジェクトを生成して更新しています。
        /// </summary>
        private void UpdateEmployeePatrolPoints()
        {
            EmployeeController[] employees = FindObjectsOfType<EmployeeController>();
            foreach (var employee in employees)
            {
                if (employee.MaterialType != materialType)
                {
                    continue;
                }
                int col = employee.Column;
                PatrolPoints patrol = GetPatrolPointsForColumn(col);
                if (patrol == null)
                {
                    Debug.LogWarning("指定された列 " + col + " にアクティブな素材がありません。");
                    continue;
                }

                GameObject tempA = new GameObject("TempPatrolPointA_Column" + col);
                tempA.transform.position = patrol.pointA;
                GameObject tempB = new GameObject("TempPatrolPointB_Column" + col);
                tempB.transform.position = patrol.pointB;
                employee.SetPatrolPoints(tempA.transform, tempB.transform);

                // ※必要に応じて、生成した一時オブジェクトは Destroy() するか管理側で再利用してください。
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
