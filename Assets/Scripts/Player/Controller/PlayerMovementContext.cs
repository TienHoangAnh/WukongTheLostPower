//using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
//public class PlayerMovementContext : MonoBehaviour
//{
//    [Header("Movement")]
//    public float moveSpeed = 5f;
//    public float runSpeed = 15f;
//    public float gravity = -9.8f;
//    public float jumpHeight = 2f;
//    public float dashDistance = 5f;
//    public float dashCooldown = 1f;
//    public Vector3 velocity;
//    [HideInInspector] public float lastDashTime = -999f;

//    [Header("Camera")]
//    public Transform cameraDirection;

//    [Header("Ground Check")]
//    public Transform groundCheck;
//    public float groundDistance = 0.4f;
//    public LayerMask groundMask;
//    public bool isGrounded;

//    [HideInInspector] public CharacterController characterController;

//    [Header("Combat")]
//    public ComboData comboData;
//    public LayerMask enemyLayer;
//    public bool isAttacking = false;

//    private IMovementState currentState;

//    [Header("Animation")]
//    public Animator animator; 

//    [Header("Save Runtime (optional)")]
//    [Tooltip("Cập nhật vị trí vào SaveRuntime mỗi X giây (0 = mỗi khung hình)")]
//    [SerializeField] private float saveRuntimeInterval = 0.5f;
//    private float _saveRuntimeTimer;

//    void Start()
//    {
//        characterController = GetComponent<CharacterController>();
//        SwitchState(new WalkState());

//        if (SaveRuntime.Current == null)
//        {
//            SaveRuntime.Current = new SaveSlotDTO
//            {
//                chapterIndex = 1,
//                player = new PlayerStateDTO { hp = 100, stamina = 100, flask = 3, pos = new Vector3DTO(transform.position), rotY = transform.eulerAngles.y }
//            };
//        }
//    }

//    void Update()
//    {
//        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
//        if (isGrounded && velocity.y < 0)
//        {
//            velocity.y = -5f;
//        }

//        currentState?.UpdateState(this);

//        if (!isAttacking && Input.GetMouseButtonDown(0))
//        {
//            SwitchState(new CombatState());
//        }

//        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= lastDashTime + dashCooldown)
//        {
//            SwitchState(new DashState());
//        }

//        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
//        {
//            SwitchState(new JumpState());
//        }

//        Patch_UpdateSaveRuntimeTick();
//    }

//    public void SwitchState(IMovementState newState)
//    {
//        currentState = newState;
//        newState.EnterState(this);
//    }

//    public void PerformAttack(float damage)
//    {
//        Vector3 origin = transform.position + transform.forward * 1f;
//        float range = 2f;

//        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayer);
//        foreach (var hit in hits)
//        {
//            ICharacter target = hit.GetComponent<ICharacter>();
//            if (target != null)
//            {
//                target.TakeDamage(damage);
//                Debug.Log($"Gây {damage} sát thương lên {hit.name}");
//            }
//        }
//    }

//    public void HandleMovement()
//    {
//        float h = Input.GetAxis("Horizontal");
//        float v = Input.GetAxis("Vertical");

//        Vector3 move = transform.right * h + transform.forward * v;
//        characterController.Move(move * moveSpeed * Time.deltaTime);

//        float currentSpeed = move.magnitude;

//        if (animator != null) 
//            animator.SetFloat("Speed", currentSpeed);

//        velocity.y += gravity * Time.deltaTime;
//        characterController.Move(velocity * Time.deltaTime);
//    }

//    void OnDrawGizmosSelected()
//    {
//        if (groundCheck != null)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
//        }
//    }

//    // ===================== PATCH UTILS =====================

//    private void Patch_UpdateSaveRuntimeTick()
//    {
//        if (SaveRuntime.Current == null) return;
//        if (SaveRuntime.Current.player == null)
//            SaveRuntime.Current.player = new PlayerStateDTO();

//        if (saveRuntimeInterval <= 0f)
//        {
//            SaveRuntime.Current.player.pos = new Vector3DTO(transform.position);
//            SaveRuntime.Current.player.rotY = transform.eulerAngles.y;
//            return;
//        }

//        _saveRuntimeTimer += Time.unscaledDeltaTime;
//        if (_saveRuntimeTimer >= saveRuntimeInterval)
//        {
//            _saveRuntimeTimer = 0f;
//            SaveRuntime.Current.player.pos = new Vector3DTO(transform.position);
//            SaveRuntime.Current.player.rotY = transform.eulerAngles.y;
//        }
//    }

//    public void OnKickHit()
//    {
//        Vector3 origin = transform.position + transform.forward * 1f;
//        float range = 1.5f;
//        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayer);
//        foreach (var hit in hits)
//        {
//            ICharacter target = hit.GetComponent<ICharacter>();
//            if (target != null) target.TakeDamage(10f);
//        }
//    }
//}



using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementContext : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 15f;
    public float gravity = -9.8f;
    public float jumpHeight = 2f;
    public float dashDistance = 5f;
    public float dashCooldown = 1f;
    public Vector3 velocity;
    [HideInInspector] public float lastDashTime = -999f;

    [Header("Camera")]
    public Transform cameraDirection;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public bool isGrounded;

    [HideInInspector] public CharacterController characterController;

    [Header("Combat")]
    public ComboData comboData;
    public LayerMask enemyLayer;
    public bool isAttacking = false;

    private IMovementState currentState;

    [Header("Animation")]
    public Animator animator;

    [Header("Save Runtime (optional)")]
    [Tooltip("Cập nhật vị trí vào SaveRuntime mỗi X giây (0 = mỗi khung hình)")]
    [SerializeField] private float saveRuntimeInterval = 0.5f;
    private float _saveRuntimeTimer;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        SwitchState(new WalkState());

        if (SaveRuntime.Current == null)
        {
            SaveRuntime.Current = new SaveSlotDTO
            {
                currentMap = 1,
                player = new PlayerStateDTO { hp = 100, stamina = 100, flask = 3, pos = new Vector3DTO(transform.position), rotY = transform.eulerAngles.y }
            };
        }
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5f;
        }

        currentState?.UpdateState(this);

        if (!isAttacking && Input.GetMouseButtonDown(0))
        {
            SwitchState(new CombatState());
        }

        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= lastDashTime + dashCooldown)
        {
            SwitchState(new DashState());
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            SwitchState(new JumpState());
        }

        Patch_UpdateSaveRuntimeTick();
    }

    public void SwitchState(IMovementState newState)
    {
        currentState = newState;
        newState.EnterState(this);
    }

    public void PerformAttack(float damage)
    {
        Vector3 origin = transform.position + transform.forward * 1f;
        float range = 2f;

        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayer);
        foreach (var hit in hits)
        {
            ICharacter target = hit.GetComponent<ICharacter>();
            if (target != null)
            {
                target.TakeDamage(damage);
                Debug.Log($"Gây {damage} sát thương lên {hit.name}");
            }
        }
    }

    public void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        float currentSpeed = move.magnitude;

        if (animator != null)
            animator.SetFloat("Speed", currentSpeed);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    // ===================== PATCH UTILS =====================

    private void Patch_UpdateSaveRuntimeTick()
    {
        if (SaveRuntime.Current == null) return;
        if (SaveRuntime.Current.player == null)
            SaveRuntime.Current.player = new PlayerStateDTO();

        if (saveRuntimeInterval <= 0f)
        {
            SaveRuntime.Current.player.pos = new Vector3DTO(transform.position);
            SaveRuntime.Current.player.rotY = transform.eulerAngles.y;
            return;
        }

        _saveRuntimeTimer += Time.unscaledDeltaTime;
        if (_saveRuntimeTimer >= saveRuntimeInterval)
        {
            _saveRuntimeTimer = 0f;
            SaveRuntime.Current.player.pos = new Vector3DTO(transform.position);
            SaveRuntime.Current.player.rotY = transform.eulerAngles.y;
        }
    }

    public void OnKickHit()
    {
        Vector3 origin = transform.position + transform.forward * 1f;
        float range = 1.5f;
        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayer);
        foreach (var hit in hits)
        {
            ICharacter target = hit.GetComponent<ICharacter>();
            if (target != null) target.TakeDamage(10f);
        }
    }
}