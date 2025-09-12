using UnityEngine;
using UnityEngine.AI;


public class EnemyAIContext : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float chaseRange = 20f;
    public float moveSpeed = 3.5f;
    public float damage = 10f;  

    private IEnemyState currentState;

    public GameObject[] dropItems;

    void Start()
    {
        if (player == null)
        {
            player = PlayerReference.Instance;
            if (player == null)
            {
                Debug.LogError("❌ Không tìm thấy PlayerReference.Instance!");
            }
        }

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        SwitchState(new EnemyIdleState());
    }

    void Update()
    {
        // Chỉ cần kiểm tra các hành vi AI, không cần animationManager hay bossStats
        if (IsPlayerInRange(chaseRange))
        {
            // Ví dụ: chuyển sang trạng thái chase
            if (!(currentState is EnemyChaseState))
            {
                SwitchState(new EnemyChaseState());
            }
        }

        currentState?.UpdateState(this);
    }

    public void SwitchState(IEnemyState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState?.EnterState(this);
    }

    public bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }

    public Transform GetPlayerTransform()
    {
        return player;
    }

    public bool HasValidTarget()
    {
        return player != null;
    }
}
