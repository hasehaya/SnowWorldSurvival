using System.Collections;

using DG.Tweening;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class CameraController :MonoBehaviour
    {
        [SerializeField, Tooltip("The target object the camera follows.")]
        private Transform target;

        // カメラ追従用のオフセット（Start で自動計算）
        private Vector3 offset;

        // フォーカス中かどうか判定するフラグ
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
                return; // フォーカス中は追従を止める

            // 追従モード（プレイヤー位置＋オフセットへ移動）
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

            // --- 1. フォーカス地点へ移動 ---
            Vector3 focusTargetPos = focusPoint + offset;
            transform.DOMove(focusTargetPos, travelTime);
            yield return new WaitForSeconds(travelTime);

            // --- 2. しばらくホールド ---
            yield return new WaitForSeconds(holdTime);

            // --- 3. プレイヤーの最新位置に向かって戻る ---
            // 戻り開始時点のカメラ位置
            Vector3 startPos = transform.position;
            float elapsed = 0f;
            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / travelTime);
                // プレイヤーの“今”の位置 + offset を参照しながらカメラを補間
                Vector3 currentTargetPos = target.position + offset;
                transform.position = Vector3.Lerp(startPos, currentTargetPos, t);

                yield return null;
            }

            // 最後に補間しきれなかった差分を補正（誤差対策）
            transform.position = target.position + offset;

            isFocusing = false;
        }
    }
}
