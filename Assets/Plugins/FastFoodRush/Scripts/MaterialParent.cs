using System.Collections.Generic;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// �f�ށiMaterialBase�j���Ǘ�����N���X
    /// </summary>
    public class MaterialParent :Unlockable
    {
        [SerializeField] private Transform materialGrid;
        private List<GameObject> materialObjects = new List<GameObject>();
        [SerializeField] MaterialType materialType;
        public MaterialType MaterialType => materialType;
        private bool isInitialized = false; // �������ς݂��̃t���O

        protected override void Awake()
        {
            base.Awake();
            InitializeMaterialObjects();
        }

        /// <summary>
        /// �f�ރI�u�W�F�N�g�̏������i�x���������ɂ��Ή��j
        /// </summary>
        private void InitializeMaterialObjects()
        {
            if (isInitialized)
                return;

            isInitialized = true;

            // materialGrid �z���̂��ׂĂ� MaterialBase �R���|�[�l���g���擾
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

            // �S�f�ނ���x��A�N�e�B�u��
            foreach (var obj in materialObjects)
            {
                obj.SetActive(false);
            }

            // unlockLevel �ɉ������f�ސ����A�N�e�B�u���i��F1��ɂ�4�j
            int materialCount = 4 * unlockLevel;
            for (int i = 0; i < materialCount && i < materialObjects.Count; i++)
            {
                materialObjects[i].SetActive(true);
            }

            UpdateEmployeePatrolPoints();
        }

        /// <summary>
        /// �w�肵����ԍ��ɑ�����A�N�e�B�u�ȑf�ނ���A�擪�i�ŏ� Row�j�Ɩ����i�ő� Row�j�̈ʒu���擾
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
        /// �V�[�����̂��ׂĂ� EmployeeController �̏���n�_���A�e�]�ƈ��̏�����ɍ��킹�čX�V���܂��B
        /// �ꎞ�I�� PatrolPoint �I�u�W�F�N�g�𐶐����čX�V���Ă��܂��B
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
                    Debug.LogWarning("�w�肳�ꂽ�� " + col + " �ɃA�N�e�B�u�ȑf�ނ�����܂���B");
                    continue;
                }

                GameObject tempA = new GameObject("TempPatrolPointA_Column" + col);
                tempA.transform.position = patrol.pointA;
                GameObject tempB = new GameObject("TempPatrolPointB_Column" + col);
                tempB.transform.position = patrol.pointB;
                employee.SetPatrolPoints(tempA.transform, tempB.transform);

                // ���K�v�ɉ����āA���������ꎞ�I�u�W�F�N�g�� Destroy() ���邩�Ǘ����ōė��p���Ă��������B
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
