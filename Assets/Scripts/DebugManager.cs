using TMPro;

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections.Generic;

public class DebugManager :MonoBehaviour
{
    // Inspector から UI GameObject を設定できるようにします。
    [SerializeField, Tooltip("Escape キーで表示する UI GameObject")]
    private GameObject debugUI;

    [SerializeField]
    private TMP_Text elapsedTimeText;

    private string saveFileName;
    // ゲーム速度を変更するステップ値。例えば 0.5f ずつ変化させる
    float gameSpeedDegree = 0.5f;

    // スクリーンショットの解像度設定
    [Serializable]
    public class ScreenshotResolution
    {
        public string displayName; // ディスプレイ名（例：6.5インチ）
        public int width;          // 横幅
        public int height;         // 高さ
    }

    [SerializeField]
    private List<ScreenshotResolution> screenshotResolutions = new List<ScreenshotResolution>
    {
        new ScreenshotResolution { displayName = "6.5インチ", width = 1242, height = 2688 },
        new ScreenshotResolution { displayName = "6.7インチ", width = 1290, height = 2796 },
        new ScreenshotResolution { displayName = "6.9インチ", width = 1320, height = 2868 },
        new ScreenshotResolution { displayName = "13インチ", width = 2064, height = 2752 }
    };

    private void Awake()
    {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
        // リリースビルドではデバッグUIを無効化し、このコンポーネントを無効化
        if (debugUI != null)
        {
            debugUI.SetActive(false);
        }
        this.enabled = false;
#endif
    }

    private void Update()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        float elapsed = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : 0f;
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(elapsed);
        elapsedTimeText.text = $"{timeSpan:hh\\:mm\\:ss}";
#endif
    }

    public void ToggleDebugUI()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // debugUI が null でない場合、activeSelf を反転させる
        if (debugUI != null)
        {
            debugUI.SetActive(!debugUI.activeSelf);
        }
#endif
    }

    // すべての解像度でスクリーンショットを撮影
    public void CaptureScreenshotAllResolutions()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // 日時を使用してフォルダ名を生成
        string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string baseDirectoryPath = Path.Combine(Application.dataPath, "Screenshots", dateTime);
        Directory.CreateDirectory(baseDirectoryPath);

        StartCoroutine(CaptureScreenshotCoroutine(baseDirectoryPath));
#endif
    }

    private System.Collections.IEnumerator CaptureScreenshotCoroutine(string baseDirectoryPath)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // 元の解像度を保存
        int originalWidth = Screen.width;
        int originalHeight = Screen.height;

        foreach (var resolution in screenshotResolutions)
        {
            // 解像度を変更
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            
            // 解像度変更が適用されるのを待つ
            yield return new WaitForEndOfFrame();
            
            // スクリーンショットを撮影
            string filename = $"{resolution.displayName}_{resolution.width}x{resolution.height}.png";
            string fullPath = Path.Combine(baseDirectoryPath, filename);
            
            ScreenCapture.CaptureScreenshot(fullPath);
            Debug.Log($"Screenshot captured: {fullPath}");
            
            // 少し待機して次の解像度に変更
            yield return new WaitForSeconds(0.5f);
        }
        
        // 元の解像度に戻す
        Screen.SetResolution(originalWidth, originalHeight, Screen.fullScreen);
        
        Debug.Log($"All screenshots saved to {baseDirectoryPath}");
#else
        yield break;
#endif
    }

    public static void ClearData()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // セーブデータを削除
        for (int i = 1; i <= 7; i++)
        {
            var stageName = $"Stage{i}";
            SaveSystem.DeleteData(stageName);
        }
        SaveSystem.DeleteData("GlobalData");
        // 現在のシーンをリロード
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
    }

    public void SpeedUpGameSpeed()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // Time.timeScale を gameSpeedDegree 分だけ上げる
        Time.timeScale += gameSpeedDegree;
        Debug.Log("Game speed increased to: " + Time.timeScale);
#endif
    }

    public void SpeedDownGameSpeed()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // Time.timeScale を gameSpeedDegree 分だけ下げる。最低値は 0.5f に設定。
        Time.timeScale = Mathf.Max(0.5f, Time.timeScale - gameSpeedDegree);
        Debug.Log("Game speed decreased to: " + Time.timeScale);
#endif
    }

    public void AddMoney()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        GameManager.Instance.AdjustMoney(1000);
#endif
    }

    public void AddMoney20000()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        GameManager.Instance.AdjustMoney(20000);
#endif
    }

    public void AdBlock()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        GameManager.Instance.PurchaseAdBlock();
#endif
    }
}
