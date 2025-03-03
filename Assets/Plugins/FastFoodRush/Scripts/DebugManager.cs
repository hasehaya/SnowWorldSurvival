using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DebugManager :MonoBehaviour
{
    // Inspector から UI GameObject を設定できるようにします。
    [SerializeField, Tooltip("Escape キーで表示する UI GameObject")]
    private GameObject debugUI;

    void Start()
    {
        // 必要に応じて初期状態で非表示にする場合は以下のように設定
        if (debugUI != null)
            debugUI.SetActive(false);
    }

    void Update()
    {
        // Escape キーが押されたら uiObject をアクティブにする
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (debugUI != null)
                debugUI.SetActive(true);
        }
    }
}
