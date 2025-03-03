using System.Collections.Generic;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class TreeParent :Unlockable
    {
        [SerializeField] private Transform treeGrid;
        private List<GameObject> treeObjects = new List<GameObject>();
        private bool isInitialized = false; // �������ς݂��̃t���O

        protected override void Awake()
        {
            base.Awake();
            InitializeTreeObjects();
        }

        /// <summary>
        /// �c���[�I�u�W�F�N�g�̏������i�x���������ɂ��Ή��j
        /// </summary>
        private void InitializeTreeObjects()
        {
            if (isInitialized)
                return;

            isInitialized = true;

            // treeGrid �z���̂��ׂĂ� Tree �R���|�[�l���g���擾
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

            // LogEmployeeController �̏���n�_���X�V
            UpdateEmployeePatrolPoints();
        }

        /// <summary>
        /// �w�肵����ԍ��ɑ�����A�N�e�B�u�Ȗ؂���A�擪�i�ŏ� Row�j�Ɩ����i�ő� Row�j�̈ʒu���擾���܂��B
        /// �w���ɖ؂����݂��Ȃ���� null ��Ԃ��܂��B
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
        /// �V�[�����̂��ׂĂ� LogEmployeeController �̏���n�_���A�e�]�ƈ��̏�����ɍ��킹�čX�V���܂��B
        /// �ꎞ�I�� PatrolPoint �I�u�W�F�N�g�𐶐����čX�V���Ă��܂��i�K�v�ɉ����ĊǗ����@��ύX���Ă��������j�B
        /// </summary>
        private void UpdateEmployeePatrolPoints()
        {
            LogEmployeeController[] employees = FindObjectsOfType<LogEmployeeController>();
            foreach (var employee in employees)
            {
                int col = employee.Column; // 1�`�̒l
                PatrolPoints patrol = GetPatrolPointsForColumn(col);
                if (patrol == null)
                {
                    Debug.LogWarning("�w�肳�ꂽ�� " + col + " �ɃA�N�e�B�u�Ȗ؂�����܂���B");
                    continue;
                }

                // �ꎞ�I�� PatrolPoint �I�u�W�F�N�g�𐶐����čX�V
                GameObject tempA = new GameObject("TempPatrolPointA_Column" + col);
                tempA.transform.position = patrol.pointA;
                GameObject tempB = new GameObject("TempPatrolPointB_Column" + col);
                tempB.transform.position = patrol.pointB;
                employee.SetPatrolPoints(tempA.transform, tempB.transform);

                // ���K�v�ɉ����āA���������ꎞ�I�u�W�F�N�g�� Destroy() ���邩�A�Ǘ����ōė��p���Ă��������B
            }
        }

        /// <summary>
        /// �P��̗�ɂ�����p�g���[���n�_�i�擪�ƍŏI�s�j�̏���ێ�����N���X
        /// </summary>
        public class PatrolPoints
        {
            public Vector3 pointA;
            public Vector3 pointB;
        }
    }
}
