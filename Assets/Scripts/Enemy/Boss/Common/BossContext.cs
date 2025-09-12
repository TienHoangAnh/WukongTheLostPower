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

    void Start()
    {
        // Kiểm tra null cho stats
        if (stats == null)
        {
            Debug.LogError("❌ BossStats chưa được gán!");
            enabled = false;
            return;
        }

        // Kiểm tra null cho NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("❌ NavMeshAgent chưa được gán!");
            enabled = false;
            return;
        }

        agent.speed = stats.moveSpeed;
        currentHealth = stats.maxHealth;

        // Kiểm tra null cho player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("❌ Không tìm thấy Player!");
            enabled = false;
            return;
        }

        // Kiểm tra null cho bossDropItems
        if (bossDropItems == null || bossDropItems.Length == 0)
        {
            Debug.LogWarning("⚠️ bossDropItems chưa được gán hoặc rỗng!");
        }

        currentState = new BossIdleState();
        currentState.EnterState(this);
    }

    void Update()
    {
        attackTimer += Time.deltaTime;
        currentState.UpdateState(this);
    }

    public void SwitchState(IBossState newState)
    {
        currentState = newState;
        newState.EnterState(this);
    }

    public void SetStrategy(IBossAttackStrategy strategy)
    {
        attackStrategy = strategy;
        strategy.Configure(this); // gán chỉ số tương ứng
    }

    public void PerformAttack()
    {
        attackStrategy?.Attack(this);
    }

    public void TakeDamage(float amount) {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(float amount) { 
        currentHealth += amount;
        if (currentHealth > stats.maxHealth)
        {
            currentHealth = stats.maxHealth;
        }
    }
    public void Die() {
        Debug.Log("💀 Boss đã chết!");
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
