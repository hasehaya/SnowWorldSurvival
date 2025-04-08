using System;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

/// <summary>
/// TMP_FontAsset 用の LocalizedAsset を表すクラス
/// </summary>
[Serializable]
public class LocalizedTMPFontAsset :LocalizedAsset<TMP_FontAsset> { }

/// <summary>
/// TMP_FontAsset 用の UnityEvent
/// </summary>
[Serializable]
public class UnityEventTMPFont :UnityEvent<TMP_FontAsset> { }

/// <summary>
/// TMP_FontAsset を切り替えるための LocalizedAssetEvent コンポーネント
/// </summary>
[AddComponentMenu("Localization/Asset/" + nameof(LocalizedTmpFontEvent))]
public class LocalizedTmpFontEvent :LocalizedAssetEvent<TMP_FontAsset, LocalizedTMPFontAsset, UnityEventTMPFont> { }
