using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageBtn : MonoBehaviour
{
    [SerializeField]
    private Button languageBtn;

    [SerializeField]
    private Language language;

    [SerializeField]
    private Image selectedImage;

    private void Start()
    {
        // ボタンクリック時に言語切り替えを実行
        languageBtn.onClick.AddListener(() => StartCoroutine(ChangeLocaleCoroutine(language)));
        
        // 初期状態の設定
        StartCoroutine(InitializeSelectedState());
    }
    
    private IEnumerator InitializeSelectedState()
    {
        // LocalizationSettings の初期化を待つ
        yield return LocalizationSettings.InitializationOperation;
        
        // 現在の選択状態を更新
        UpdateSelectedState();
        
        // 言語変更イベントをリッスン
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }
    
    private void OnDestroy()
    {
        // イベントリスナーを解除
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
    
    private void OnLocaleChanged(Locale locale)
    {
        // 言語が変更された時に選択状態を更新
        UpdateSelectedState();
    }
    
    private void UpdateSelectedState()
    {
        if (selectedImage != null)
        {
            string currentLocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code;
            string buttonLocaleCode = GetLocaleCode(language);
            
            // 現在のロケールとボタンのロケールが一致するかチェック
            selectedImage.gameObject.SetActive(currentLocaleCode == buttonLocaleCode);
        }
    }

    private IEnumerator ChangeLocaleCoroutine(Language language)
    {
        // LocalizationSettings の初期化を待つ
        yield return LocalizationSettings.InitializationOperation;

        // enum に対応するロケールコードを取得
        string localeCode = GetLocaleCode(language);

        // AvailableLocales から該当ロケールを探す
        var newLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (newLocale != null)
        {
            LocalizationSettings.SelectedLocale = newLocale;
            Debug.Log($"Locale を {localeCode} に切り替えました。");
            
            // 言語設定をGameManagerを通じて保存
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveLocaleSettings(localeCode);
            }
        }
        else
        {
            Debug.LogWarning($"Locale '{localeCode}' が見つかりませんでした。");
        }
    }

    private string GetLocaleCode(Language language)
    {
        switch (language)
        {
            case Language.English:
                return "en";
            case Language.Korean:
                return "ko";
            case Language.Japanese:
                return "ja";
            case Language.ChineseSimplified:
                return "zh-CN";
            case Language.ChineseTraditional:
                return "zh-TW";
            default:
                return LocalizationSettings.SelectedLocale.Identifier.Code;
        }
    }
}

public enum Language
{
    English = 0,
    Korean = 1,
    Japanese = 2,
    ChineseSimplified = 3,
    ChineseTraditional = 4
}
