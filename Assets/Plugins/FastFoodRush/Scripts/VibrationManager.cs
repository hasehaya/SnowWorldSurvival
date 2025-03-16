using UnityEngine;

public static class VibrationManager
{
    // ------------------------------------------------------------------------
    // iOS向け設定 (SystemSoundID でのハプティクス呼び出し)
    // ------------------------------------------------------------------------
#if UNITY_IOS && !UNITY_EDITOR
    // iOSネイティブ関数のインポート
    [DllImport("__Internal")]
    private static extern void _playSystemSound(int systemSoundID);
#endif

    /// <summary>
    /// iOSでハプティクス用のSystemSoundを再生
    /// 1519/1520/1521/1522 などで強度や種類が変わる
    /// </summary>
    private static void PlaySystemSound(int systemSoundID)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _playSystemSound(systemSoundID);
#endif
    }

    // ------------------------------------------------------------------------
    // Android向け設定
    // ------------------------------------------------------------------------
#if UNITY_ANDROID && !UNITY_EDITOR
    private static readonly AndroidJavaClass UnityPlayer =
        new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    private static readonly AndroidJavaObject CurrentActivity =
        UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    private static readonly AndroidJavaObject Vibrator =
        CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

    /// <summary>
    /// Androidのバージョンチェック (API 26以上かどうか)
    /// </summary>
    private static bool IsAndroidOreoOrHigher()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            int sdkInt = version.GetStatic<int>("SDK_INT");
            return (sdkInt >= 26);
        }
    }

    /// <summary>
    /// API26以上の端末で VibrationEffectを使って振動させる
    /// </summary>
    private static void VibrateOneShot(long milliseconds, int amplitude = -1)
    {
        // amplitude = -1 でデフォルト強度
        if (IsAndroidOreoOrHigher())
        {
            using (var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
            {
                var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                    "createOneShot",
                    milliseconds,
                    amplitude
                );
                Vibrator.Call("vibrate", effect);
            }
        }
        else
        {
            // API26未満の場合
            Vibrator.Call("vibrate", milliseconds);
        }
    }
#endif

    // ------------------------------------------------------------------------
    // 共通メソッド群
    // ------------------------------------------------------------------------

    /// <summary>
    /// 「短めのバイブレーション」を実行 (数十ミリ秒程度)
    /// </summary>
    public static void ShortVibration()
    {
        if (!SystemInfo.supportsVibration)
            return;

#if UNITY_IOS && !UNITY_EDITOR
        // iOSの場合: 1519(軽め) の Taptic Engine フィードバックを再生
        PlaySystemSound(1519);

#elif UNITY_ANDROID && !UNITY_EDITOR
        // Androidの場合: 20msほどの短い振動
        VibrateOneShot(20, -1);

#else
        // その他(エディタ含む)の場合はフォールバック
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// 「中程度～やや長めのバイブレーション」を実行 (例: 100ms)
    /// </summary>
    public static void MediumVibration()
    {
        if (!SystemInfo.supportsVibration)
            return;

#if UNITY_IOS && !UNITY_EDITOR
        // 1520 または 1521, 1522 など別のSystemSoundIDで強度を変えられる
        // (ただし微妙な違いなので実機で要確認)
        PlaySystemSound(1520);

#elif UNITY_ANDROID && !UNITY_EDITOR
        VibrateOneShot(100, -1);

#else
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// 任意のミリ秒振動させる
    /// (iOSは細かい制御が難しいためフォールバック的に利用)
    /// </summary>
    public static void Vibrate(long milliseconds)
    {
        if (!SystemInfo.supportsVibration)
            return;

#if UNITY_IOS && !UNITY_EDITOR
        // iOSの場合、細かい制御が標準APIではできないため一律フォールバック
        Handheld.Vibrate();
#elif UNITY_ANDROID && !UNITY_EDITOR
        VibrateOneShot(milliseconds, -1);
#else
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// 「ポコポコ」した振動など、パターン振動を実行するサンプル
    /// (短い振動 → 間隔 → 短い振動 → 間隔 ...)
    /// </summary>
    public static void PatternVibration()
    {
        if (!SystemInfo.supportsVibration)
            return;

#if UNITY_IOS && !UNITY_EDITOR
    // iOSは1度きりのSystemSoundしか再生できないため、複数回連続で鳴らす場合は
    // コルーチン等で複数回 PlaySystemSound(1519) を呼ぶ必要があります。
    // 例としては下記のようなもの:
    // ShortVibration(); // 1回目
    // yield return new WaitForSeconds(0.075f);
    // ShortVibration(); // 2回目
    // といった感じで実装します。
    PlaySystemSound(1519);

#elif UNITY_ANDROID && !UNITY_EDITOR
    // Androidの場合は createWaveform でパターン振動を実行可能
    if (IsAndroidOreoOrHigher())
    {
        using (var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
        {
            // 例: 待機0ms → 15ms振動 → 60ms待機 → 15ms振動
            long[] pattern = { 0, 15, 60, 15 };
            // -1 はリピートなし, 0以上ならパターン内のインデックスに戻ってリピート
            var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                "createWaveform",
                pattern,
                -1
            );
            Vibrator.Call("vibrate", effect);
        }
    }
    else
    {
        // API26未満の場合
        long[] pattern = { 0, 15, 60, 15 };
        // 第2引数でリピートのインデックス指定(-1でリピートなし)
        Vibrator.Call("vibrate", pattern, -1);
    }
#else
        // エディタやその他プラットフォームではフォールバック
        Handheld.Vibrate();
#endif
    }

}
