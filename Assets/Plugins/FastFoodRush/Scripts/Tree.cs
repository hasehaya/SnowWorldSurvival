using System.Collections;

using DG.Tweening;

using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class Tree :Interactable
    {
        [SerializeField] private int treeHealth = 3;                // ���݂̗̑�
        [SerializeField] private int logCount = 3;                  // �̗͌������ɐ������郍�O��
        [SerializeField] private float decreaseInterval = 0.3f;     // �̗͂�����Ԋu
        [SerializeField] private GameObject treeModel;              // �؂̌����ڂ̃��f��
        [SerializeField] private float regrowDelay = 4f;            // �Đ��܂ł̑ҋ@����
        [SerializeField] private float growthDuration = 0.5f;         // �����ɂ����鎞��

        private float timer = 0f;
        private int initialHealth;
        private Vector3 initialScale;
        private bool isFallen = false;  // �؂��|��Ă��邩�ǂ���

        private void Start()
        {
            initialHealth = treeHealth;
            if (treeModel != null)
                initialScale = treeModel.transform.localScale;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && !isFallen)
            {
                timer += Time.deltaTime;
                if (timer >= decreaseInterval)
                {
                    treeHealth--;
                    SpawnLogs();

                    if (treeHealth <= 0)
                    {
                        isFallen = true;
                        StartCoroutine(RegrowTree());
                    }
                    timer = 0f;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                timer = 0f;
            }
        }

        // ���O�𐶐����ăW�����v�A�j���[�V���������s
        private void SpawnLogs()
        {
            for (int i = 0; i < logCount; i++)
            {
                SpawnLog(i);
            }
        }

        // ���O�𐶐����A�����_�������փW�����v��A�v���C���[�փW�����v���ĉ��
        private void SpawnLog(int index)
        {
            var log = PoolManager.Instance.SpawnObject("Log");
            Vector3 startPos = transform.position + Vector3.up * index;
            log.transform.position = startPos;

            Vector3 randomXZ = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            float randomDistance = Random.Range(0.5f, 1f);
            Vector3 firstJumpTarget = startPos + randomXZ * randomDistance + Vector3.up * 5;

            Sequence seq = DOTween.Sequence()
                .Append(log.transform.DOJump(firstJumpTarget, 2f, 1, 0.5f))
                .OnComplete(() =>
                {
                    if (player.Stack.StackType == StackType.None || player.Stack.StackType == StackType.Log)
                    {
                        if (player.Stack.Height < player.Capacity)
                        {
                            player.Stack.AddToStack(log.transform, StackType.Log);
                        }
                    }
                });
        }

        // �؂̍Đ������F�؂̃��f�����\���ɂ��A��莞�Ԍ�ɏ������X�P�[�����琬��
        private IEnumerator RegrowTree()
        {
            if (treeModel != null)
                treeModel.SetActive(false);

            yield return new WaitForSeconds(regrowDelay);

            treeHealth = initialHealth;
            timer = 0f;
            if (treeModel != null)
            {
                treeModel.transform.localScale = Vector3.zero;
                treeModel.SetActive(true);
                treeModel.transform.DOScale(initialScale, growthDuration);
            }
            isFallen = false;
        }

        protected override void OnPlayerEnter() { }
        protected override void OnPlayerExit() { }
    }
}
