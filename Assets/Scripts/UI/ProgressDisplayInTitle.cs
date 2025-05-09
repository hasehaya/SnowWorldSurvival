﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressDisplayInTitle : MonoBehaviour
{
    [SerializeField] private Gradient progressGradient;
    [SerializeField] private SlicedFilledImage progressFill;
    [SerializeField] private TMP_Text progressText;

    public void UpdateProgress(float progress)
    {
        // Update fill color based on gradient evaluation
        progressFill.color = progressGradient.Evaluate(progress);

        // Update fill amount of the progress bar
        progressFill.fillAmount = progress;

        // Update progress text with percentage format
        progressText.text = $"{progress * 100:0.##}%";
    }
}
