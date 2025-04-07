using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;


public class CameraController :MonoBehaviour
{
    [SerializeField, Tooltip("The target object the camera follows.")]
    private Transform target;

    // Calculated offset from the target (set in Start)
    private Vector3 offset;

    // Flag to indicate if the camera is currently focusing on one or more points.
    private bool isFocusing = false;

    [SerializeField, Tooltip("How long the camera takes to move to the focus point.")]
    private float travelTime = 0.5f;

    [SerializeField, Tooltip("How long the camera stays focused before moving on.")]
    private float holdTime = 1.3f;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (isFocusing)
            return; // Stop following while focusing
                    // Default follow: maintain offset relative to the target
        transform.position = target.position + offset;
    }

    /// <summary>
    /// Focus on a single point and then return to the target.
    /// </summary>
    public void FocusOnPointAndReturn(Vector3 focusPoint)
    {
        if (isFocusing)
            return;

        StartCoroutine(FocusSequence(new List<Vector3> { focusPoint }));
    }

    /// <summary>
    /// Focus sequentially on multiple points and then return to the target.
    /// </summary>
    /// <param name="focusPoints">A list of world positions to focus on sequentially.</param>
    public void FocusOnPointsAndReturn(List<Vector3> focusPoints)
    {
        if (isFocusing)
            return;

        if (focusPoints == null || focusPoints.Count == 0)
            return;

        StartCoroutine(FocusSequence(focusPoints));
    }

    /// <summary>
    /// Coroutine that moves the camera sequentially to each focus point, holds, then returns to the target.
    /// </summary>
    /// <param name="focusPoints">List of world positions to visit.</param>
    private IEnumerator FocusSequence(List<Vector3> focusPoints)
    {
        isFocusing = true;

        // Sequentially focus on each point.
        foreach (Vector3 focusPoint in focusPoints)
        {
            // Calculate the camera's target position for focus.
            Vector3 focusTargetPos = focusPoint + offset;
            // Move to the focus point using DOTween.
            transform.DOMove(focusTargetPos, travelTime);
            yield return new WaitForSeconds(travelTime);

            // Hold at the focus point.
            yield return new WaitForSeconds(holdTime);
        }

        // After visiting all points, return to the target's current position.
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);
            Vector3 currentTargetPos = target.position + offset;
            transform.position = Vector3.Lerp(startPos, currentTargetPos, t);
            yield return null;
        }

        // Correct any slight discrepancies.
        transform.position = target.position + offset;
        isFocusing = false;
    }
}
