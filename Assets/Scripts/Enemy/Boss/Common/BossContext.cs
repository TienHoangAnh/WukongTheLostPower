using UnityEngine;
using UnityEngine.AI;

public class BossContext : MonoBehaviour, ICharacter
{
    public Transform player;
    public NavMeshAgent agent;
    public GameObject projectilePrefab;
    public BossStats stats;
    public GameObject[] bossDropItems;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public float attackTimer = 0f;

    private IBossState currentState;
    private IBossAttackStrategy attackStrategy;

    // Animation manager (shared EnemyAnimationManager used for all boss types)
    public IEnemyAnimationManager animationManager;

    void Start()
    {
        if (stats == null)
        {
            Debug.LogError("❌ BossStats not assigned yet!");
            enabled = false;
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("❌ NavMeshAgent not assigned!");
            enabled = false;
            return;
        }

        // try to bind animation manager automatically
        if (animationManager == null)
            animationManager = GetComponent<IEnemyAnimationManager>() ?? FindFirstObjectByType<HybridBossAnimationManager>();

        if (animationManager == null)
        {
            Debug.LogWarning("[BossContext] EnemyAnimationManager not found on boss. Animations will not play.");
        }
 
        agent.speed = stats.moveSpeed;
        currentHealth = stats.maxHealth;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("❌ Player not found!");
            enabled = false;
            return;
        }

        if (bossDropItems == null || bossDropItems.Length == 0)
        {
            Debug.LogWarning("⚠️ bossDropItems is not assigned or empty!");
        }

        currentState = new BossIdleState();
        currentState.EnterState(this);
        // trigger idle/standing animation
        animationManager?.PlayStanding();
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (currentState == null)
        {
            Debug.LogWarning("[BossContext] currentState is null in Update(). Skipping state update.");
            return;
        }

        try
        {
            currentState.UpdateState(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BossContext] Exception in state UpdateState: {ex}\nDisabling BossContext to avoid repeated errors.");
            enabled = false;
        }
    }

    public void SwitchState(IBossState newState)
    {
        currentState = newState;
        newState.EnterState(this);

        // Play animations corresponding to state
        if (newState is BossIdleState)
        {
            animationManager?.PlayStanding();
        }
        else if (newState is BossChaseState)
        {
            animationManager?.PlayChasePlayer();
        }
        else if (newState is BossAttackState)
        {
            // Attack state may be followed by strategy-specific animations at PerformAttack time
            animationManager?.PlayBreakLookAround();
        }
    }

    public void SetStrategy(IBossAttackStrategy strategy)
    {
        attackStrategy = strategy;
        strategy.Configure(this);
    }

    public void PerformAttack()
    {
        // trigger animation based on attack strategy type
        if (attackStrategy is MeleeBossAttackStrategy)
        {
            Debug.Log("[BossContext] PerformAttack -> Melee animation (360do)");
            animationManager?.Play360do();
        }
        else if (attackStrategy is RangedBossAttackStrategy)
        {
            Debug.Log("[BossContext] PerformAttack -> Ranged animation (Scream)");
            animationManager?.PlayScream();
        }
        else if (attackStrategy is HybridBossAttackStrategy)
        {
            // choose animation based on distance to player
            if (player != null)
            {
                float d = Vector3.Distance(transform.position, player.position);
                if (d <= 4f)
                {
                    Debug.Log("[BossContext] PerformAttack -> Hybrid melee (360do)");
                    animationManager?.Play360do();
                }
                else
                {
                    Debug.Log("[BossContext] PerformAttack -> Hybrid ranged (Scream)");
                    animationManager?.PlayScream();
                }
            }
            else
            {
                Debug.Log("[BossContext] PerformAttack -> Hybrid default (Scream) - no player ref");
                animationManager?.PlayScream();
            }
        }

        attackStrategy?.Attack(this);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        animationManager?.PlayTakeDamage();

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > stats.maxHealth)
        {
            currentHealth = stats.maxHealth;
        }
    }
    public void Die()
    {
        Debug.Log("💀 Boss is dead!");
        animationManager?.PlayDie();
        DropBossItem();
        Destroy(gameObject);
    }

    void DropBossItem()
    {
        foreach (var item in bossDropItems)
        {
            Instantiate(item, transform.position, Quaternion.identity);
        }
    }
}
