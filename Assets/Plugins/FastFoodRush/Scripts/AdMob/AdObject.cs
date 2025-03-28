using System.Collections;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    /// <summary>
    /// �v���C���[����莞�ԃG���A���ɂ���ƍL�������̃|�b�v�A�b�v��\������N���X
    /// </summary>
    public class AdObject :Interactable
    {
        [SerializeField, Tooltip("�|�b�v�A�b�v�\���܂ł̑ҋ@���ԁi�b�j")]
        private float timeToPopup = 3f;

        [SerializeField, Tooltip("�\������L���|�b�v�A�b�v�� GameObject")]
        private GameObject adPopup;

        private Coroutine adCoroutine;

        /// <summary>
        /// �v���C���[���G���A�ɓ������Ƃ��A��莞�Ԍ�Ƀ|�b�v�A�b�v�\�����J�n����
        /// </summary>
        protected override void OnPlayerEnter()
        {
            // ��莞�Ԍo�ߌ�ɍL���|�b�v�A�b�v��\������R���[�`�����J�n
            adCoroutine = StartCoroutine(WaitForAdPopup());
        }

        /// <summary>
        /// �v���C���[���G���A��ޏo�����Ƃ��A�|�b�v�A�b�v�ҋ@���̃R���[�`�����~����
        /// </summary>
        protected override void OnPlayerExit()
        {
            if (adCoroutine != null)
            {
                StopCoroutine(adCoroutine);
                adCoroutine = null;
            }
        }

        /// <summary>
        /// �w�肵�����ԑҋ@���A�v���C���[���܂��G���A���ɂ���ꍇ�ɍL���|�b�v�A�b�v��\������
        /// </summary>
        IEnumerator WaitForAdPopup()
        {
            yield return new WaitForSeconds(timeToPopup);
            if (player != null)
            {
                ShowAdPopup();
            }
        }

        /// <summary>
        /// �L�������̃|�b�v�A�b�v��\������
        /// </summary>
        void ShowAdPopup()
        {
            if (adPopup != null)
            {
                adPopup.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Ad Popup ���A�^�b�`����Ă��܂���B");
            }
        }
    }
}
