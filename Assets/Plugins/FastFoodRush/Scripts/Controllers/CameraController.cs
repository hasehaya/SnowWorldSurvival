using System.Collections;

using DG.Tweening;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class CameraController :MonoBehaviour
    {
        [SerializeField, Tooltip("The target object the camera follows.")]
        private Transform target;

        // �J�����Ǐ]�p�̃I�t�Z�b�g�iStart �Ŏ����v�Z�j
        private Vector3 offset;

        // �t�H�[�J�X�����ǂ������肷��t���O
        private bool isFocusing = false;

        [SerializeField, Tooltip("How long the camera takes to move to the focus point.")]
        private float travelTime = 0.5f;

        [SerializeField, Tooltip("How long the camera stays focused before returning.")]
        private float holdTime = 1.3f;

        void Start()
        {
            offset = transform.position - target.position;
        }

        void LateUpdate()
        {
            if (isFocusing)
                return; // �t�H�[�J�X���͒Ǐ]���~�߂�

            // �Ǐ]���[�h�i�v���C���[�ʒu�{�I�t�Z�b�g�ֈړ��j
            transform.position = target.position + offset;
        }

        public void FocusOnPointAndReturn(Vector3 focusPoint)
        {
            if (isFocusing)
                return;

            StartCoroutine(FocusRoutine(focusPoint));
        }

        private IEnumerator FocusRoutine(Vector3 focusPoint)
        {
            isFocusing = true;

            // --- 1. �t�H�[�J�X�n�_�ֈړ� ---
            Vector3 focusTargetPos = focusPoint + offset;
            transform.DOMove(focusTargetPos, travelTime);
            yield return new WaitForSeconds(travelTime);

            // --- 2. ���΂炭�z�[���h ---
            yield return new WaitForSeconds(holdTime);

            // --- 3. �v���C���[�̍ŐV�ʒu�Ɍ������Ė߂� ---
            // �߂�J�n���_�̃J�����ʒu
            Vector3 startPos = transform.position;
            float elapsed = 0f;
            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / travelTime);
                // �v���C���[�́g���h�̈ʒu + offset ���Q�Ƃ��Ȃ���J��������
                Vector3 currentTargetPos = target.position + offset;
                transform.position = Vector3.Lerp(startPos, currentTargetPos, t);

                yield return null;
            }

            // �Ō�ɕ�Ԃ�����Ȃ�����������␳�i�덷�΍�j
            transform.position = target.position + offset;

            isFocusing = false;
        }
    }
}
