using UnityEngine;

public static class VibrationManager
{
    // ------------------------------------------------------------------------
    // iOS�����ݒ� (SystemSoundID �ł̃n�v�e�B�N�X�Ăяo��)
    // ------------------------------------------------------------------------
#if UNITY_IOS && !UNITY_EDITOR
    // iOS�l�C�e�B�u�֐��̃C���|�[�g
    [DllImport("__Internal")]
    private static extern void _playSystemSound(int systemSoundID);
#endif

    /// <summary>
    /// iOS�Ńn�v�e�B�N�X�p��SystemSound���Đ�
    /// 1519/1520/1521/1522 �Ȃǂŋ��x���ނ��ς��
    /// </summary>
    private static void PlaySystemSound(int systemSoundID)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _playSystemSound(systemSoundID);
#endif
    }

    // ------------------------------------------------------------------------
    // Android�����ݒ�
    // ------------------------------------------------------------------------
#if UNITY_ANDROID && !UNITY_EDITOR
    private static readonly AndroidJavaClass UnityPlayer =
        new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    private static readonly AndroidJavaObject CurrentActivity =
        UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    private static readonly AndroidJavaObject Vibrator =
        CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

    /// <summary>
    /// Android�̃o�[�W�����`�F�b�N (API 26�ȏォ�ǂ���)
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
    /// API26�ȏ�̒[���� VibrationEffect���g���ĐU��������
    /// </summary>
    private static void VibrateOneShot(long milliseconds, int amplitude = -1)
    {
        // amplitude = -1 �Ńf�t�H���g���x
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
            // API26�����̏ꍇ
            Vibrator.Call("vibrate", milliseconds);
        }
    }
#endif

    // ------------------------------------------------------------------------
    // ���ʃ��\�b�h�Q
    // ------------------------------------------------------------------------

    /// <summary>
    /// �u�Z�߂̃o�C�u���[�V�����v�����s (���\�~���b���x)
    /// </summary>
    public static void ShortVibration()
    {
        if (!SystemInfo.supportsVibration)
            return;

#if UNITY_IOS && !UNITY_EDITOR
        // iOS�̏ꍇ: 1519(�y��) �� Taptic Engine �t�B�[�h�o�b�N���Đ�
        PlaySystemSound(1519);

#elif UNITY_ANDROID && !UNITY_EDITOR
        // Android�̏ꍇ: 20ms�قǂ̒Z���U��
        VibrateOneShot(20, -1);

#else
        // ���̑�(�G�f�B�^�܂�)�̏ꍇ�̓t�H�[���o�b�N
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// �u�����x�`��Ⓑ�߂̃o�C�u���[�V�����v�����s (��: 100ms)
    /// </summary>
    public static void MediumVibration()
    {
        if (!SystemInfo.supportsVibration)
            return;

#if UNITY_IOS && !UNITY_EDITOR
        // 1520 �܂��� 1521, 1522 �ȂǕʂ�SystemSoundID�ŋ��x��ς�����
        // (�����������ȈႢ�Ȃ̂Ŏ��@�ŗv�m�F)
        PlaySystemSound(1520);

#elif UNITY_ANDROID && !UNITY_EDITOR
        VibrateOneShot(100, -1);

#else
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// �C�ӂ̃~���b�U��������
    /// (iOS�ׂ͍������䂪������߃t�H�[���o�b�N�I�ɗ��p)
    /// </summary>
    public static void Vibrate(long milliseconds)
    {
        if (!SystemInfo.supportsVibration)
            return;

#if UNITY_IOS && !UNITY_EDITOR
        // iOS�̏ꍇ�A�ׂ������䂪�W��API�ł͂ł��Ȃ����߈ꗥ�t�H�[���o�b�N
        Handheld.Vibrate();
#elif UNITY_ANDROID && !UNITY_EDITOR
        VibrateOneShot(milliseconds, -1);
#else
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// �u�|�R�|�R�v�����U���ȂǁA�p�^�[���U�������s����T���v��
    /// (�Z���U�� �� �Ԋu �� �Z���U�� �� �Ԋu ...)
    /// </summary>
    public static void PatternVibration()
    {
        if (!SystemInfo.supportsVibration)
            return;

#if UNITY_IOS && !UNITY_EDITOR
    // iOS��1�x�����SystemSound�����Đ��ł��Ȃ����߁A������A���Ŗ炷�ꍇ��
    // �R���[�`�����ŕ����� PlaySystemSound(1519) ���ĂԕK�v������܂��B
    // ��Ƃ��Ă͉��L�̂悤�Ȃ���:
    // ShortVibration(); // 1���
    // yield return new WaitForSeconds(0.075f);
    // ShortVibration(); // 2���
    // �Ƃ����������Ŏ������܂��B
    PlaySystemSound(1519);

#elif UNITY_ANDROID && !UNITY_EDITOR
    // Android�̏ꍇ�� createWaveform �Ńp�^�[���U�������s�\
    if (IsAndroidOreoOrHigher())
    {
        using (var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
        {
            // ��: �ҋ@0ms �� 15ms�U�� �� 60ms�ҋ@ �� 15ms�U��
            long[] pattern = { 0, 15, 60, 15 };
            // -1 �̓��s�[�g�Ȃ�, 0�ȏ�Ȃ�p�^�[�����̃C���f�b�N�X�ɖ߂��ă��s�[�g
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
        // API26�����̏ꍇ
        long[] pattern = { 0, 15, 60, 15 };
        // ��2�����Ń��s�[�g�̃C���f�b�N�X�w��(-1�Ń��s�[�g�Ȃ�)
        Vibrator.Call("vibrate", pattern, -1);
    }
#else
        // �G�f�B�^�₻�̑��v���b�g�t�H�[���ł̓t�H�[���o�b�N
        Handheld.Vibrate();
#endif
    }

}
