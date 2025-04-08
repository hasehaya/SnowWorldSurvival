using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class AdCoolTime :MonoBehaviour
{
    [SerializeField] private Button btn;
    [SerializeField] private TextMeshProUGUI btnText;

    private void Start()
    {
        StartCoroutine(UpdateCoolTimeCoroutine());
    }

    private IEnumerator UpdateCoolTimeCoroutine()
    {
        while (true)
        {
            var coolTime = AdManager.Instance.CoolTime;
            bool isActive = coolTime <= 0;
            btn.interactable = isActive;
            btnText.text = isActive ? "–³—¿" : $"{coolTime}";
            yield return new WaitForSeconds(1f);
        }
    }
}
