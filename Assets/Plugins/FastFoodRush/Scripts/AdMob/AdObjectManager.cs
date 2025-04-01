using System.Collections.Generic;

using UnityEngine;

public class AdObjectManager :MonoBehaviour
{
    [SerializeField]
    private Transform[] popPoses;

    [SerializeField]
    private AdObject[] adObjects;

    private void Start()
    {
        // popPoses �z��̒����烉���_���ȏ��Ԃɕ��בւ������X�g���쐬
        List<Transform> posesList = new List<Transform>(popPoses);
        for (int i = 0; i < posesList.Count; i++)
        {
            Transform temp = posesList[i];
            int randomIndex = Random.Range(i, posesList.Count);
            posesList[i] = posesList[randomIndex];
            posesList[randomIndex] = temp;
        }

        // �I������ʒu�͑S�̂̔����i�؂�̂āj
        int halfCount = posesList.Count / 2;

        // adObjects �̐��Ɣ�r���A�z�u�\�Ȑ��ɍ��킹��
        int count = Mathf.Min(halfCount, adObjects.Length);

        for (int i = 0; i < count; i++)
        {
            // ���ꂼ��� AdObject ��I�񂾃|�W�V�����Ɉړ�������
            AdObject obj = adObjects[i];
            obj.transform.position = posesList[i].position;

            // �K�v�ɉ����Đe�q�֌W��ݒ�
            // obj.transform.SetParent(posesList[i]);
        }
    }
}
