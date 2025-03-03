using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class Workstation :Unlockable
    {
        [SerializeField, Tooltip("The working spots where the worker performs tasks")]
        private WorkingSpot[] workingSpots;  // �z��ōő�3�� WorkingSpot ��o�^

        [SerializeField, Tooltip("The worker objects assigned to this workstation")]
        private GameObject[] workers;        // �z��ōő�3�� Worker ��o�^

        // Worker �̉�����x���F1�ڂ� Level2�A2�ڂ� Level4�A3�ڂ� Level6
        private readonly int[] workerUnlockLevels = { 2, 4, 6 };

        /// <summary>
        /// ���[�J�[�����邩�𔻒肵�܂��B
        /// unlockLevel �� 2 �ȏ�Ȃ烏�[�J�[�����݂���Ɣ��肵�A
        /// ����ȊO�̏ꍇ�� WorkingSpot �� HasWorker �v���p�e�B���Q�Ƃ��܂��B
        /// </summary>
        protected bool hasWorker
        {
            get
            {
                if (unlockLevel >= workerUnlockLevels[0])
                    return true;
                foreach (var ws in workingSpots)
                {
                    if (ws != null && ws.HasWorker)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// ���[�N�X�e�[�V�����̃A�����b�N�����B
        /// Worker �� WorkingSpot �� unlockLevel �ɉ����Đ؂�ւ��܂��B
        /// Worker �� 1�ڂ� Level2�A2�ڂ� Level4�A3�ڂ� Level6 �ŗL���ɂȂ�܂��B
        /// </summary>
        /// <param name="animate">�A�j���[�V�������Đ�����ꍇ�� true</param>
        public override void Unlock(bool animate = true)
        {
            base.Unlock(animate);

            // workers �� workingSpots �̔z�񒷂���v���Ă���O��ł�
            for (int i = 0; i < workers.Length; i++)
            {
                if (workers[i] != null && i < workerUnlockLevels.Length)
                {
                    if (unlockLevel >= workerUnlockLevels[i])
                    {
                        // �Y�����[�J�[������F���[�J�[�\���A�Ή����� WorkingSpot ��\��
                        workers[i].SetActive(true);
                        if (i < workingSpots.Length && workingSpots[i] != null)
                        {
                            workingSpots[i].gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        // ����O�̏ꍇ�F���[�J�[��\���A�Ή����� WorkingSpot �\��
                        workers[i].SetActive(false);
                        if (i < workingSpots.Length && workingSpots[i] != null)
                        {
                            workingSpots[i].gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}
