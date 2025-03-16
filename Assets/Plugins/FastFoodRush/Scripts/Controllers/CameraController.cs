using System.Collections;

using DG.Tweening;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class CameraController :MonoBehaviour
    {
        [SerializeField, Tooltip("The target object the camera follows.")]
        private Transform target;

        // �J�����Ǐ]�p�̃I�t�Z�b�g�iStart�Ŏ����v�Z�j
        private Vector3 offset;

        // �t�H�[�J�X�����ǂ������肷��t���O
        private bool isFocusing = false;

        // �t�H�[�J�X�J�n�O�̃J�����ʒu��ۑ�
        private Vector3 originalPosition;

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

            // �Ǐ]���[�h
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

            originalPosition = transform.position;
            Vector3 targetPos = focusPoint + offset;

            transform.DOMove(targetPos, travelTime);
            yield return new WaitForSeconds(travelTime);

            yield return new WaitForSeconds(holdTime);

            transform.DOMove(originalPosition, travelTime);
            yield return new WaitForSeconds(travelTime);

            isFocusing = false;
        }
    }
}
