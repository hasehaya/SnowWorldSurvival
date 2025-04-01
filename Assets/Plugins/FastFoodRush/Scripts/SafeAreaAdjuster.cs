using UnityEngine;

public class SafeAreaAdjuster :MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea(Screen.safeArea);
    }

    private void Update()
    {
        // safeAreaが変わったときのみ更新
        if (Screen.safeArea != lastSafeArea)
        {
            ApplySafeArea(Screen.safeArea);
            lastSafeArea = Screen.safeArea;
        }
    }

    private void ApplySafeArea(Rect area)
    {
        // YのMaxに200ピクセルのマージンを設定（上側の余白を確保）
        area.yMax -= 200;

        Vector2 anchorMin = area.position;
        Vector2 anchorMax = area.position + area.size;

        anchorMin.x /= Screen.width;
        anchorMax.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }
}