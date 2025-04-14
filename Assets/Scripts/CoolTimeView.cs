using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class CoolTimeView :MonoBehaviour
{
    [SerializeField] private Image gauge;
    [SerializeField] private TextMeshProUGUI time;

    public void Reload(float time, float rate)
    {
        gauge.fillAmount = rate;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        this.time.text = $"{minutes:00}:{seconds:00}";
    }
}
