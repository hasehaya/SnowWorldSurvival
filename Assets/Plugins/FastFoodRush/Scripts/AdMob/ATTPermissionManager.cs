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
        // 1�b�ҋ@�i�N������ɕ\������Ȃ����ۂւ̑΍�j
        yield return new WaitForSeconds(1.0f);

        // ���_�C�A���O�\��
        ATTrackingStatusBinding.RequestAuthorizationTracking();
    }
#endif
}