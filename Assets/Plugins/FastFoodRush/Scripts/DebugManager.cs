using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DebugManager :MonoBehaviour
{
    // Inspector ���� UI GameObject ��ݒ�ł���悤�ɂ��܂��B
    [SerializeField, Tooltip("Escape �L�[�ŕ\������ UI GameObject")]
    private GameObject debugUI;

    void Start()
    {
        // �K�v�ɉ����ď�����ԂŔ�\���ɂ���ꍇ�͈ȉ��̂悤�ɐݒ�
        if (debugUI != null)
            debugUI.SetActive(false);
    }

    void Update()
    {
        // Escape �L�[�������ꂽ�� uiObject ���A�N�e�B�u�ɂ���
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (debugUI != null)
                debugUI.SetActive(true);
        }
    }
}
