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
        // popPoses 配列の中からランダムな順番に並べ替えたリストを作成
        List<Transform> posesList = new List<Transform>(popPoses);
        for (int i = 0; i < posesList.Count; i++)
        {
            Transform temp = posesList[i];
            int randomIndex = Random.Range(i, posesList.Count);
            posesList[i] = posesList[randomIndex];
            posesList[randomIndex] = temp;
        }

        // 選択する位置は全体の半分（切り捨て）
        int halfCount = posesList.Count / 2;

        // adObjects の数と比較し、配置可能な数に合わせる
        int count = Mathf.Min(halfCount, adObjects.Length);

        for (int i = 0; i < count; i++)
        {
            // それぞれの AdObject を選んだポジションに移動させる
            AdObject obj = adObjects[i];
            obj.transform.position = posesList[i].position;

            // 必要に応じて親子関係を設定
            // obj.transform.SetParent(posesList[i]);
        }
    }
}
