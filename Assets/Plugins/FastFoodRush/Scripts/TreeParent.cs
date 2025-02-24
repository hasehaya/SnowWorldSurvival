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

            // treeGrid �z���̂��ׂĂ� Tree �R���|�[�l���g���擾
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
        /// �O���b�h��̖؂��A���[�J�����W�� X �l�i��j���ƂɃO���[�v�����A
        /// �e��̐擪�iRow1�j�ƍŏI�s�iMaxRow�j�̃��[���h���W��Ԃ��܂��B
        /// </summary>
        public List<PatrolPoints> GetPatrolPointsPerColumn()
        {
            List<PatrolPoints> patrolPointsList = new List<PatrolPoints>();

            // X ���W���قړ������̂𓯂���Ƃ݂Ȃ��i�����_��2�ʂ܂Łj
            Dictionary<float, List<Transform>> columns = new Dictionary<float, List<Transform>>();
            foreach (var treeObj in treeObjects)
            {
                Transform t = treeObj.transform;
                float xKey = Mathf.Round(t.localPosition.x * 100f) / 100f;
                if (!columns.ContainsKey(xKey))
                    columns[xKey] = new List<Transform>();
                columns[xKey].Add(t);
            }

            // �e�񂲂ƂɁAZ ���W�Ń\�[�g���Đ擪�Ɩ������擾
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
        /// �P��̗�ɂ�����p�g���[���n�_�i�擪�ƍŏI�s�j�̏���ێ�����N���X
        /// </summary>
        public class PatrolPoints
        {
            public Vector3 pointA;
            public Vector3 pointB;
        }
    }
}
