using System.Collections;

using UnityEngine;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class ATTPermissionManager :MonoBehaviour
{
#if UNITY_IOS
    private IEnumerator Start()
    {
        // 1秒待機（起動直後に表示されない現象への対策）
        yield return new WaitForSeconds(1.0f);

        // 許可ダイアログ表示
        ATTrackingStatusBinding.RequestAuthorizationTracking();
    }
#endif
}