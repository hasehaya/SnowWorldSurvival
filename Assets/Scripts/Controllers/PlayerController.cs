using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController :MonoBehaviour
{
    [SerializeField] private float baseSpeed = 3.0f;
    [SerializeField] private float rotateSpeed = 360f;
    [SerializeField] private int baseCapacity = 5;
    [SerializeField] private AudioClip[] footsteps;
    [SerializeField] private WobblingStack stack;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    // 起動時は非表示にする
    [SerializeField] private GameObject maxTextObj;

    public WobblingStack Stack => stack;
    public int Capacity { get; private set; }

    private Animator animator;
    private CharacterController controller;
    private AudioSource audioSource;
    private GlobalData globalData;

    private float moveSpeed;
    private Vector3 movement;
    private Vector3 velocity;
    private bool isGrounded;
    private float IK_Weight;

    private bool previousIsMax;
    private const float gravityValue = -9.81f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        // スタート時に非表示
        if (maxTextObj != null)
        {
            maxTextObj.SetActive(false);
        }
    }

    void Start()
    {
        globalData = GameManager.Instance.GlobalData;
        baseSpeed *= globalData.SpeedUpRate();
        baseCapacity += globalData.CapacityUpCount();
        moveSpeed = baseSpeed;
        Capacity = baseCapacity;
    }

    void Update()
    {
        moveSpeed = globalData.IsPlayerSpeedActive ? baseSpeed * 2 : baseSpeed;
        Capacity = globalData.IsPlayerCapacityActive ? baseCapacity * 2 : baseCapacity;

        // スタックが満タンかどうか
        bool isMax = (Stack.Count >= Capacity);

        // 状態が切り替わった時にだけ UI を切り替え
        if (isMax != previousIsMax)
        {
            if (maxTextObj != null)
            {
                maxTextObj.SetActive(isMax);
            }

            if (isMax)
            {
                var objectStackList = FindObjectsOfType<ObjectStack>();
                for (int i = 0; i < objectStackList.Length; i++)
                {
                    if (objectStackList[i].MaterialType == stack.MaterialType)
                    {
                        objectStackList[i].ShowArrow();
                    }
                }
            }
            else
            {
                // 満タンが解除された場合に必要があればここで矢印などを消す処理
            }
        }

        // isMax 中は毎フレーム LookAt でカメラを向かせる
        // 後ろ向きになってしまう場合は LookAt 後に180度回転させる
        if (isMax && maxTextObj != null && maxTextObj.activeSelf)
        {
            if (Camera.main != null)
            {
                maxTextObj.transform.LookAt(Camera.main.transform);
                // 180度反転
                maxTextObj.transform.Rotate(0f, 180f, 0f);
            }
        }

        previousIsMax = isMax;

        // 以下、移動やアニメーションの処理
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = 0f;

        movement.x = SimpleInput.GetAxis("Horizontal");
        movement.z = SimpleInput.GetAxis("Vertical");
        movement = (Quaternion.Euler(0, 45, 0) * movement).normalized;
        controller.Move(movement * Time.deltaTime * moveSpeed);

        if (movement != Vector3.zero)
        {
            var lookRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
        }

        velocity.y += gravityValue * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        animator.SetBool("IsMoving", movement != Vector3.zero);
    }

    public void OnStep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight < 0.5f)
            return;
        audioSource.clip = footsteps[Random.Range(0, footsteps.Length)];
        audioSource.Play();
    }

    void OnAnimatorIK()
    {
        IK_Weight = Mathf.MoveTowards(IK_Weight, Mathf.Clamp01(stack.Count), Time.deltaTime * 3.5f);

        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, IK_Weight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, IK_Weight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }

        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, IK_Weight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, IK_Weight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }
    }
}
