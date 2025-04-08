using System;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

/// <summary>
/// TMP_FontAsset �p�� LocalizedAsset ��\���N���X
/// </summary>
[Serializable]
public class LocalizedTMPFontAsset :LocalizedAsset<TMP_FontAsset> { }

/// <summary>
/// TMP_FontAsset �p�� UnityEvent
/// </summary>
[Serializable]
public class UnityEventTMPFont :UnityEvent<TMP_FontAsset> { }

/// <summary>
/// TMP_FontAsset ��؂�ւ��邽�߂� LocalizedAssetEvent �R���|�[�l���g
/// </summary>
[AddComponentMenu("Localization/Asset/" + nameof(LocalizedTmpFontEvent))]
public class LocalizedTmpFontEvent :LocalizedAssetEvent<TMP_FontAsset, LocalizedTMPFontAsset, UnityEventTMPFont> { }
