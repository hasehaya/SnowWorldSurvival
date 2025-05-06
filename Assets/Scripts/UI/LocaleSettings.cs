using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

/// <summary>
/// 言語設定を管理するためのスタティッククラス
/// GameManagerに依存せずに言語設定を保存・読み込みます
/// </summary>
public static class LocaleSettings
{
    private const string LOCALE_CODE_KEY = "SelectedLocaleCode";
    
    /// <summary>
    /// 言語設定を保存します
    /// </summary>
    /// <param name="localeCode">保存する言語コード（例: "en", "ja"）</param>
    public static void SaveLocale(string localeCode)
    {
        if (string.IsNullOrEmpty(localeCode))
            return;
            
        PlayerPrefs.SetString(LOCALE_CODE_KEY, localeCode);
        PlayerPrefs.Save();
        Debug.Log($"言語設定 {localeCode} を保存しました。");
    }
    
    /// <summary>
    /// 保存されている言語コードを取得します
    /// </summary>
    /// <returns>保存されている言語コード。存在しない場合は空文字列</returns>
    public static string GetSavedLocaleCode()
    {
        return PlayerPrefs.GetString(LOCALE_CODE_KEY, string.Empty);
    }
    
    /// <summary>
    /// 保存されている言語設定を適用します
    /// </summary>
    public static IEnumerator ApplySavedLocale()
    {
        string savedLocaleCode = GetSavedLocaleCode();
        
        if (string.IsNullOrEmpty(savedLocaleCode))
            yield break;
            
        // LocalizationSettingsの初期化を待つ
        yield return LocalizationSettings.InitializationOperation;
        
        // 保存されていた言語コードに対応するロケールを探す
        var savedLocale = LocalizationSettings.AvailableLocales.GetLocale(savedLocaleCode);
        if (savedLocale != null)
        {
            LocalizationSettings.SelectedLocale = savedLocale;
            Debug.Log($"保存されていた言語設定 {savedLocaleCode} を適用しました。");
        }
    }
} 