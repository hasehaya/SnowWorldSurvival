using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// MaterialBase ���p�������ؗp�̑f�ރN���X
    /// </summary>
    public class MaterialTree :MaterialBase
    {
        [SerializeField] private GameObject treeModel;      // �؂̌����ڂ̃��f��
        [SerializeField] private float regrowDelay = 12f;      // �Đ��ҋ@����
        [SerializeField] private float growthDuration = 0.5f; // �����ɂ����鎞��

        protected override GameObject GetMaterialModel()
        {
            return treeModel;
        }

        protected override string GetPoolKey()
        {
            // �������郊�\�[�X�̃v�[���L�[�i��Ƃ��� "Log" ���w��j
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
