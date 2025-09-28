using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class EnemyAIContext : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Config")]
    [Tooltip("Distance to start chasing the player")]
    public float detectDistance = 20f;
    [Tooltip("Distance to start attacking (stop agent)")]
    public float attackDistance = 2f;
    public float chaseSpeed = 3.5f;
    public float attackCooldown = 1.5f;
    [Tooltip("Damage for attack type 1")]
    public int attackDamage1 = 10;
    [Tooltip("Damage for attack type 2")]
    public int attackDamage2 = 18;
    [Range(0f, 1f)] public float chanceAttack2 = 0.25f;

    [Tooltip("Layer của Player để OverlapSphere gây damage chính xác")]
    public LayerMask playerLayer = 1 << 8; // sửa trong Inspector

    [Header("Attack Hitbox")]
    [Tooltip("Tâm hitbox khi Animation Event xảy ra (nếu null dùng vị trí enemy).")]
    public Transform hitOrigin;
    [Tooltip("Bán kính hitbox khi tung đòn.")]
    public float hitRadius = 0.9f;

    [Header("Debug")]
    public bool drawGizmos = true;

    // Animator hashes
    static readonly int HashIsChasing = Animator.StringToHash("IsChasing");
    static readonly int HashIsDead = Animator.StringToHash("IsDead");
    static readonly int HashAttack = Animator.StringToHash("Attack");
    static readonly int HashAttackIndex = Animator.StringToHash("AttackIndex");

    // runtime
    private IEnemyState currentState;
    private float attackTimer = 0f;
    private int lastAttackIndex = 1;
    private bool isDead = false;

    // rebind control
    private float _nextRebindAt = 0f;
    private const float REBIND_INTERVAL = 0.5f; // rebind mỗi 0.5s khi mất player

    void OnValidate()
    {
        detectDistance = Mathf.Max(0.1f, detectDistance);
        attackDistance = Mathf.Max(0.1f, attackDistance);
        hitRadius = Mathf.Max(0.05f, hitRadius);

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        // Nghe sự kiện scene load để rebind khi qua chapter
        SceneManager.sceneLoaded += OnSceneLoadedTryRebind;
        TryBindPlayer(true);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedTryRebind;
    }

    private void OnSceneLoadedTryRebind(Scene s, LoadSceneMode m)
    {
        // Scene mới → thử bind lại ngay và cho phép rebind liên tục 1 lát
        _nextRebindAt = 0f;
        TryBindPlayer(true);
    }

    void Start()
    {
        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.updateRotation = true;
            agent.stoppingDistance = Mathf.Max(0f, attackDistance - 0.1f);
        }

        // Nếu Start mà vẫn chưa có → state vẫn chạy nhưng HasValidTarget() sẽ false
        SwitchState(new EnemyIdleState());
    }

    void Update()
    {
        if (isDead) return;

        // Nếu mất player (spawn trễ, bị replace…), thử rebind định kỳ
        if (player == null && Time.time >= _nextRebindAt)
        {
            TryBindPlayer(false);
            _nextRebindAt = Time.time + REBIND_INTERVAL;
        }

        currentState?.UpdateState(this);
    }

    /// <summary>
    /// Cơ chế bind chắc chắn:
    /// 1) Ưu tiên PlayerAnchor.Current (đặt trên prefab Player).
    /// 2) Fallback FindWithTag("Player").
    /// </summary>
    private void TryBindPlayer(bool log)
    {
        if (player != null) return;

        Transform p = PlayerAnchor.Current;
        if (p == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) p = go.transform;
        }

        if (p != null)
        {
            player = p;
            if (log) Debug.Log($"[EnemyAIContext] Bound Player = {player.name}", this);
        }
        else if (log)
        {
            Debug.LogWarning("[EnemyAIContext] Player not found. Ensure Player has PlayerAnchor and/or tag 'Player'.", this);
        }
    }

    public void SwitchState(IEnemyState newState)
    {
        if (animator != null)
        {
            bool chasing = newState is EnemyChaseState;
            animator.SetBool(HashIsChasing, chasing);
        }
        currentState?.ExitState(this);
        currentState = newState;
        currentState?.EnterState(this);
    }

    public bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }

    public Transform GetPlayerTransform() => player;
    public bool HasValidTarget() => player != null;

    // === Animation Event ===
    public void PerformAttackHit()
    {
        if (isDead) return;

        Vector3 center = (hitOrigin != null ? hitOrigin.position : transform.position)
                         + transform.forward * (attackDistance * 0.5f);

        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerLayer, QueryTriggerInteraction.Ignore);
        int dmg = (lastAttackIndex == 2) ? attackDamage2 : attackDamage1;

        bool applied = false;
        foreach (var col in hits)
        {
            if (col == null) continue;

            var dmgable = col.GetComponentInParent<IDamageable>();
            if (dmgable != null) { dmgable.TakeDamage(dmg); applied = true; Debug.Log($"[EnemyAI] Hit(IDamageable) dmg={dmg}, atk={lastAttackIndex}"); break; }

            var ch = col.GetComponentInParent<ICharacter>();
            if (ch != null) { ch.TakeDamage(dmg); applied = true; Debug.Log($"[EnemyAI] Hit(ICharacter) dmg={dmg}, atk={lastAttackIndex}"); break; }
        }

        if (!applied && player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackDistance + 0.5f)
            {
                var dmgable = player.GetComponent<IDamageable>();
                if (dmgable != null) { dmgable.TakeDamage(dmg); applied = true; }
                else
                {
                    var ch = player.GetComponent<ICharacter>();
                    if (ch != null) { ch.TakeDamage(dmg); applied = true; }
                }
            }
            if (applied) Debug.Log($"[EnemyAI] Fallback hit dmg={dmg}, atk={lastAttackIndex}");
            else Debug.Log($"[EnemyAI] PerformAttackHit aborted: no target in hitbox (dist={dist:F2}).");
        }
    }

    // EnemyHealth gọi khi máu về 0
    public void OnDeathFromHealth()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("[EnemyAI] OnDeathFromHealth → play death, stop agent.");

        if (animator != null)
        {
            animator.SetBool(HashIsChasing, false);
            animator.SetBool(HashIsDead, true);
        }
        if (agent != null) agent.isStopped = true;

        var cols = GetComponentsInChildren<Collider>();
        StartCoroutine(DisableCollidersDelayed(cols, 1.2f));

        Destroy(gameObject, 3.0f);
    }

    private System.Collections.IEnumerator DisableCollidersDelayed(Collider[] cols, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var c in cols) if (c != null) c.enabled = false;
    }

    // state gọi để phát attack + ghi index
    public void TriggerAttackAnimationAndRegisterIndex(int attackIndex)
    {
        FaceTargetFlat();

        lastAttackIndex = attackIndex;
        if (animator != null)
        {
            animator.ResetTrigger(HashAttack);
            animator.SetInteger(HashAttackIndex, attackIndex);
            animator.SetTrigger(HashAttack);
        }
        MarkAttackStarted();
        Debug.Log($"[EnemyAI] Triggered Attack index={attackIndex}");
    }

    // Cooldown dùng chung
    public bool CanAttackNow() => Time.time >= attackTimer;
    public void MarkAttackStarted() => attackTimer = Time.time + attackCooldown;

    // quay mặt theo mặt phẳng ngang
    public void FaceTargetFlat(float turnSpeed = 12f)
    {
        var t = GetPlayerTransform();
        if (t == null) return;
        Vector3 to = t.position - transform.position; to.y = 0f;
        if (to.sqrMagnitude < 0.001f) return;
        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        Gizmos.color = Color.red;
        Vector3 center = (hitOrigin != null ? hitOrigin.position : transform.position)
                         + transform.forward * (attackDistance * 0.5f);
        Gizmos.DrawWireSphere(center, hitRadius);
    }
}

// Relay nếu Animator ở child
public class AnimationEventRelay : MonoBehaviour
{
    public EnemyAIContext owner;
    public void PerformAttackHit()
    {
        if (owner != null) owner.PerformAttackHit();
        else Debug.LogWarning("[AnimationEventRelay] Missing owner.");
    }
}
