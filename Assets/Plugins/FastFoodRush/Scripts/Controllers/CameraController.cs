using System.Collections;

using DG.Tweening;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class CameraController :MonoBehaviour
    {
        [SerializeField, Tooltip("The target object the camera follows.")]
        private Transform target;

        // カメラ追従用のオフセット（Startで自動計算）
        private Vector3 offset;

        // フォーカス中かどうか判定するフラグ
        private bool isFocusing = false;

        // フォーカス開始前のカメラ位置を保存
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
                return; // フォーカス中は追従を止める

            // 追従モード
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
