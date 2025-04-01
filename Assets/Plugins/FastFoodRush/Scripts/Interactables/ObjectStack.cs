using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;


public class ObjectStack :Interactable
{
    [SerializeField, Tooltip("The type of stack (e.g., food, package, etc.)")]
    private MaterialType materialType;

    [SerializeField, Tooltip("Time interval between each stack operation")]
    private float stackInterval = 0.05f;

    // ★ 追加：矢印オブジェクトをインスペクターで設定できるようSerializeFieldを追加
    [SerializeField, Tooltip("Arrow object displayed when stack is ready to receive items")]
    private GameObject arrowObj;

    public MaterialType MaterialType => materialType;
    public int MaxStack { get; set; }
    public int Count => objects.Count;
    public bool IsFull => Count >= MaxStack;

    private Stack<GameObject> objects = new Stack<GameObject>();
    private float stackOffset;
    private float stackTimer;

    // ★ 追加：IsShow管理用フラグ
    private bool isShow;

    void Start()
    {
        stackOffset = GameManager.Instance.GetStackOffset(materialType);

        // 必要に応じて初期表示をオフにしておく場合はこちら
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

    // ★ 新規：Arrowを表示して上下動させる
    public void ShowArrow()
    {
        isShow = true;
        if (arrowObj == null)
            return;

        arrowObj.SetActive(true);
        // 繰り返しアニメーションをリセットするために一度Kill
        DOTween.Kill(arrowObj.transform);

        // 原点位置を確保
        Vector3 startPos = arrowObj.transform.position;

        // 矢印を上下に動かし続ける
        arrowObj.transform
            .DOMoveY(startPos.y + 0.3f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }

    // オブジェクトをスタックに加える
    public void AddToStack(GameObject obj)
    {
        // ★ AddToStack呼び出し時にArrowを非表示
        isShow = false;
        if (arrowObj != null)
        {
            DOTween.Kill(arrowObj.transform);  // 上下動アニメーションを停止
            arrowObj.SetActive(false);
        }

        objects.Push(obj);

        var heightOffset = new Vector3(0f, (Count - 1) * stackOffset, 0f);
        Vector3 targetPos = transform.position + heightOffset;

        obj.transform.DOJump(targetPos, 5f, 1, 0.3f);
    }

    // スタックからオブジェクトを外してTransformを返す
    public Transform RemoveFromStack()
    {
        Transform removed = objects.Pop().transform;
        DOTween.Kill(removed);
        return removed;
    }
}
