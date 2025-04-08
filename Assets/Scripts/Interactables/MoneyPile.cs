using System.Collections;

using DG.Tweening;

using UnityEngine;

public class MoneyPile :ObjectPile
{
    [SerializeField, Tooltip("Maximum number of money objects allowed in the pile.")]
    private int maxPile = 120;

    [SerializeField, Range(1, 8), Tooltip("Multiplier for the collection rate based on the number of objects in the pile.")]
    private int collectMultiplier = 2;

    private int hiddenMoney; // The number of money that are hidden when the pile is full.
    private bool isCollectingMoney; // Flag to indicate if the collection process is ongoing.
    private int collectRate => objects.Count > 8 ? collectMultiplier : 1; // Collection rate based on current pile size.

    protected override void Start()
    {
        // Start method intentionally left blank to prevent altering the stack height for money objects.
    }

    private void Update()
    {
        // �����O���[�o���ȋ����W���A�N�e�B�u�Ȃ�A�ς�ł���������ׂăv���C���[�̏������ɉ��Z����B
        if (GameManager.Instance.GlobalData.IsMoneyCollectionActive && (objects.Count > 0 || hiddenMoney > 0))
        {
            int totalMoney = objects.Count + hiddenMoney;
            // �X�^�b�N���̂��ׂĂ̋��I�u�W�F�N�g��ԋp���A�B�������N���A
            while (objects.Count > 0)
            {
                PoolManager.Instance.ReturnObject(objects.Pop());
            }
            hiddenMoney = 0;
            GameManager.Instance.AdjustMoney(totalMoney);
        }
    }

    /// <summary>
    /// (��A�N�e�B�u���̂ݗ��p) �h���b�v�����B�R���N�V�������łȂ���Ή������Ȃ��B
    /// �O���[�o�����W���A�N�e�B�u�̍ۂ� Update() �ňꊇ���W���Ă��邽�߁ADrop() �͎��s����܂���B
    /// </summary>
    protected override void Drop()
    {
        if (!isCollectingMoney)
            return; // ���W���łȂ���Ή������Ȃ��B

        // �O���[�o���Ȏ��W�������ȏꍇ�̂݁A�]���ʂ�A�j���[�V�����t���̋��I�u�W�F�N�g�̃h���b�v�������s��
        if (!GameManager.Instance.GlobalData.IsMoneyCollectionActive)
        {
            var moneyObj = PoolManager.Instance.SpawnObject("Money");
            moneyObj.transform.position = objects.Peek().transform.position;

            moneyObj.transform.DOJump(player.transform.position + Vector3.up * 2, 3f, 1, 0.5f)
                .OnComplete(() => PoolManager.Instance.ReturnObject(moneyObj));

            AudioManager.Instance.PlaySFX(AudioID.Money);
        }
    }

    /// <summary>
    /// �v���C���[���g���K�[�G���A�ɓ������ۂɎ��W�v���Z�X���J�n����B
    /// </summary>
    protected override void OnPlayerEnter()
    {
        StartCoroutine(CollectMoney());
    }

    /// <summary>
    /// �R���[�`���B���̃I�u�W�F�N�g���������W���āA�v���C���[�̏������𑝉�������B
    /// �O���[�o�����W���A�N�e�B�u�łȂ��ꍇ�ɂ̂݁A�I�u�W�F�N�g�P�ʂł̎��W���������{����B
    /// </summary>
    IEnumerator CollectMoney()
    {
        isCollectingMoney = true;

        // �܂��͉B���������Z
        GameManager.Instance.AdjustMoney(hiddenMoney);
        hiddenMoney = 0;

        // �I�u�W�F�N�g�����݂��������W�������p��
        while (player != null && objects.Count > 0)
        {
            for (int i = 0; i < collectRate; i++)
            {
                if (objects.Count == 0)
                {
                    isCollectingMoney = false;
                    break;
                }

                // �I�u�W�F�N�g����菜���A���ڏ������ɉ��Z
                objects.Pop();
                GameManager.Instance.AdjustMoney(1);
            }

            if (collectRate > 1)
                yield return null;
            else
                yield return new WaitForSeconds(0.03f);
        }

        isCollectingMoney = false;
    }

    /// <summary>
    /// ����ǉ�����ہA�O���[�o�����W���A�N�e�B�u�ł���Α����ɉ��Z���A
    /// �����łȂ���Ώ]���ʂ菈���i�e�ʓ��Ȃ璼�ډ��Z�A���t�Ȃ� hiddenMoney �ɒ~����j���܂��B
    /// �܂��A���I�u�W�F�N�g���X�|�[���������ɁA���ڒ����������s���܂��B
    /// </summary>
    public void AddMoney()
    {
        if (GameManager.Instance.GlobalData.IsMoneyCollectionActive)
        {
            // ���W���A�N�e�B�u�Ȃ�A�V���ȋ��͑����ɏ������ɉ��Z�i�X�|�[�����Ȃ��j
            GameManager.Instance.AdjustMoney(1);
        }
        else
        {
            if (objects.Count < maxPile)
            {
                var moneyObj = PoolManager.Instance.SpawnObject("Money"); // Spawn a new money object.
                AddObject(moneyObj);
            }
            else
            {
                // ���ɍő吔�ɒB���Ă���ꍇ�� hiddenMoney �ɉ��Z
                hiddenMoney++;
            }
        }

        if (!isCollectingMoney && player != null)
            StartCoroutine(CollectMoney());
    }
}
