using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;


public class ObjectStack :Interactable
{
    [SerializeField, Tooltip("The type of stack (e.g., food, package, etc.)")]
    private MaterialType materialType;

    [SerializeField, Tooltip("Time interval between each stack operation")]
    private float stackInterval = 0.05f;

    // �� �ǉ��F���I�u�W�F�N�g���C���X�y�N�^�[�Őݒ�ł���悤SerializeField��ǉ�
    [SerializeField, Tooltip("Arrow object displayed when stack is ready to receive items")]
    private GameObject arrowObj;

    public MaterialType MaterialType => materialType;
    public int MaxStack { get; set; }
    public int Count => objects.Count;
    public bool IsFull => Count >= MaxStack;

    private Stack<GameObject> objects = new Stack<GameObject>();
    private float stackOffset;
    private float stackTimer;

    // �� �ǉ��FIsShow�Ǘ��p�t���O
    private bool isShow;

    void Start()
    {
        stackOffset = GameManager.Instance.GetStackOffset(materialType);

        // �K�v�ɉ����ď����\�����I�t�ɂ��Ă����ꍇ�͂�����
        if (arrowObj != null)
            arrowObj.SetActive(false);
    }

    void Update()
    {
        stackTimer += Time.deltaTime;

        if (stackTimer >= stackInterval)
        {
            stackTimer = 0f;

            if (player == null)
                return;
            if (player.Stack.MaterialType != materialType)
                return;
            if (player.Stack.Count == 0)
                return;

            if (objects.Count >= MaxStack)
                return;

            var objToStack = player.Stack.RemoveFromStack();
            if (objToStack == null)
                return;

            AddToStack(objToStack.gameObject);
            VibrationManager.PatternVibration();
            AudioManager.Instance.PlaySFX(AudioID.Pop);
        }
    }

    // �� �V�K�FArrow��\�����ď㉺��������
    public void ShowArrow()
    {
        isShow = true;
        if (arrowObj == null)
            return;

        arrowObj.SetActive(true);
        // �J��Ԃ��A�j���[�V���������Z�b�g���邽�߂Ɉ�xKill
        DOTween.Kill(arrowObj.transform);

        // ���_�ʒu���m��
        Vector3 startPos = arrowObj.transform.position;

        // �����㉺�ɓ�����������
        arrowObj.transform
            .DOMoveY(startPos.y + 0.3f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }

    // �I�u�W�F�N�g���X�^�b�N�ɉ�����
    public void AddToStack(GameObject obj)
    {
        // �� AddToStack�Ăяo������Arrow���\��
        isShow = false;
        if (arrowObj != null)
        {
            DOTween.Kill(arrowObj.transform);  // �㉺���A�j���[�V�������~
            arrowObj.SetActive(false);
        }

        objects.Push(obj);

        var heightOffset = new Vector3(0f, (Count - 1) * stackOffset, 0f);
        Vector3 targetPos = transform.position + heightOffset;

        obj.transform.DOJump(targetPos, 5f, 1, 0.3f);
    }

    // �X�^�b�N����I�u�W�F�N�g���O����Transform��Ԃ�
    public Transform RemoveFromStack()
    {
        Transform removed = objects.Pop().transform;
        DOTween.Kill(removed);
        return removed;
    }
}
