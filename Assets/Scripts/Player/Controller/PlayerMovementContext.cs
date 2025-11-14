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
    // ===================== MOVEMENT =====================
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float runSpeed = 10f;
    public float gravity = -9.8f;
    public float jumpHeight = 2f;
    public float dashDistance = 5f;
    public float dashCooldown = 1f;
    public Vector3 velocity;
    [HideInInspector] public float lastDashTime = -999f;

    // ===================== CAMERA =====================
    [Header("Camera")]
    public Transform cameraDirection;

    // ===================== GROUND CHECK =====================
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public bool isGrounded;

    [HideInInspector] public CharacterController characterController;

    // ===================== COMBAT =====================
    [Header("Combat")]
    public ComboData comboData;
    public LayerMask enemyLayer;
    public bool isAttacking = false;

    private IMovementState currentState;

    // ===================== ANIMATION =====================
    [Header("Animation")]
    public Animator animator;

    // ===================== SAVE RUNTIME =====================
    [Header("Save Runtime (optional)")]
    [Tooltip("Update player position to SaveRuntime every X seconds (0 = every frame)")]
    [SerializeField] private float saveRuntimeInterval = 0.5f;
    private float _saveRuntimeTimer;

    // ================================================================
    // LIFECYCLE
    // ================================================================
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        SwitchState(new WalkState());

        // Initialize default SaveRuntime if none exists
        if (SaveRuntime.Current == null)
        {
            SaveRuntime.Current = new SaveSlotDTO
            {
                currentMap = 1,
                player = new PlayerStateDTO
                {
                    hp = 100,
                    stamina = 100,
                    flask = 3,
                    pos = new Vector3DTO(transform.position),
                    rotY = transform.eulerAngles.y
                }
            };
            Debug.Log("[PlayerMovementContext] SaveRuntime initialized.");
        }
    }

    void Update()
    {
        // Grounded check
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        else
            isGrounded = characterController.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -5f;

        // Update current state
        currentState?.UpdateState(this);

        // State transitions
        if (!isAttacking && Input.GetMouseButtonDown(0))
            SwitchState(new CombatState());

        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= lastDashTime + dashCooldown)
            SwitchState(new DashState());

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            SwitchState(new JumpState());

        Patch_UpdateSaveRuntimeTick();
    }

    // ================================================================
    // STATE MACHINE API
    // ================================================================
    public void SwitchState(IMovementState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState?.EnterState(this);
        Debug.Log($"[State] Switched to {newState.GetType().Name}");
    }

    public bool IsIn<T>() where T : IMovementState => currentState is T;

    public void TakeDamage()
    {
        // If currently dead, stunned or already in take-damage reaction, ignore
        if (IsIn<DieState>() || IsIn<StunState>() || IsIn<TakeDamageState>()) return;
        if (Time.time < lastDashTime + dashCooldown) return;

        Debug.Log("[PlayerMovementContext] TakeDamage triggered due to hit reaction.");

        // Trigger take-damage animation (if animator assigned) instead of dashing
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        // Enter take-damage state to handle movement/controls while hurt
        SwitchState(new TakeDamageState());
    }

    // ================================================================
    // ACTION HELPERS
    // ================================================================
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

    public void OnKickHit()
    {
        Vector3 origin = transform.position + transform.forward * 1f;
        float range = 1.5f;
        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayer);
        foreach (var hit in hits)
        {
            ICharacter target = hit.GetComponent<ICharacter>();
            if (target != null)
            {
                target.TakeDamage(10f);
                Debug.Log($"[Attack] Kick hit {hit.name} for 10 damage.");
            }
        }
    }

    // Gizmo helper
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    // ================================================================
    // SAVE-RUNTIME TICK
    // ================================================================
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
            Debug.Log("[SaveRuntime] Player position updated.");
        }
    }

    // ================================================================
    // COMBAT DAMAGE HANDLER
    // ================================================================
    public void PerformAttack(float damage)
    {
        Vector3 origin = transform.position + transform.forward * 1f;
        float range = 2f;

        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayer);
        foreach (var hit in hits)
        {
            var target = hit.GetComponent<ICharacter>();
            if (target != null)
            {
                target.TakeDamage(damage);
                Debug.Log($"[Attack] Player hit {hit.name} for {damage} damage.");
            }
        }
    }
}
