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

            // treeGrid �z���̂��ׂĂ� Tree �R���|�[�l���g���擾
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
        /// �O���b�h��̖؂��A���[�J�����W�� X �l�i��j���ƂɃO���[�v�����A
        /// �e��̐擪�iRow1�j�ƍŏI�s�iMaxRow�j�̃��[���h���W��Ԃ��܂��B
        /// </summary>
        public List<PatrolPoints> GetPatrolPointsPerColumn()
        {
            // �x���������F�܂�����������Ă��Ȃ���Ύ��{
            if (!isInitialized)
            {
                InitializeTreeObjects();
            }

            List<PatrolPoints> patrolPointsList = new List<PatrolPoints>();

            // Column �v���p�e�B�ŃO���[�v������i�L�[�͐����j�ATree �R���|�[�l���g�ŃO���[�v��
            Dictionary<int, List<Tree>> columns = new Dictionary<int, List<Tree>>();
            foreach (var treeObj in treeObjects)
            {
                // �A�N�e�B�u�Ȗ؂̂ݑΏۂƂ���
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

            // �e�񂲂ƂɁARow �v���p�e�B�Ń\�[�g���Đ擪�Ɩ������擾
            foreach (var kvp in columns)
            {
                var treeList = kvp.Value;
                if (treeList.Count == 0)
                    continue;

                treeList.Sort((a, b) => a.Row.CompareTo(b.Row));

                PatrolPoints points = new PatrolPoints
                {
                    pointA = treeList[0].transform.position,                  // �ŏ��� Row �̖؂̈ʒu
                    pointB = treeList[treeList.Count - 1].transform.position    // �ő�� Row �̖؂̈ʒu
                };
                patrolPointsList.Add(points);
            }

            return patrolPointsList;
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
