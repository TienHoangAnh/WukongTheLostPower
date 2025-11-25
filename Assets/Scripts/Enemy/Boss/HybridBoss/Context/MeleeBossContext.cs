using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MeleeBossContext : MonoBehaviour, ICharacter
{
    public HybridBossAnimationManager animationManager;
    public BossStats stats; // assign ScriptableObject in inspector
    public Transform player;
    public NavMeshAgent agent; // optional, assign on prefab

    // runtime
    private float currentHealth;
    private float attackTimer = 0f;
    private float stateTimer = 0f;
    private bool hasBeenKnocked = false;

    private enum State
    {
        IdleSit,
        Standing,
        Chase,
        ChaseLoop, // ChasePlayer1
        Attack360,
        Scream,
        KnockFallingBack,
        StandUp,
        TakeDamage,
        Die
    }

    private State state = State.IdleSit;

    [Header("Behavior settings")]
    public float idleToStandingDelay = 1.0f;
    public float standingDuration = 0.8f;
    public float knockDuration = 1.2f;
    public float standUpDuration = 0.6f;
    public float attackCooldown = 3f;
    public float detectionRange = 15f;

    void Awake()
    {
        if (animationManager == null) animationManager = GetComponent<HybridBossAnimationManager>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (stats == null)
        {
            Debug.LogError("HybridBossContext: BossStats not assigned!");
            enabled = false;
            return;
        }

        currentHealth = stats.maxHealth;
        attackCooldown = stats.attackCooldown > 0f ? stats.attackCooldown : attackCooldown;
        detectionRange = stats.detectionRange > 0f ? stats.detectionRange : detectionRange;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        ChangeState(State.IdleSit);
    }

    void Update()
    {
        if (state == State.Die) return;

        attackTimer += Time.deltaTime;
        stateTimer += Time.deltaTime;

        // Keep agent chasing target if available
        if ((state == State.Chase || state == State.ChaseLoop) && player != null && agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        // State machine update
        switch (state)
        {
            case State.IdleSit:
                UpdateIdleSit();
                break;
            case State.Standing:
                UpdateStanding();
                break;
            case State.Chase:
                // quick transition to loop
                ChangeState(State.ChaseLoop);
                break;
            case State.ChaseLoop:
                UpdateChaseLoop();
                break;
            case State.Attack360:
                // handled by coroutine; nothing every-frame required here
                break;
            case State.Scream:
                // handled by coroutine
                break;
            case State.KnockFallingBack:
                if (stateTimer >= knockDuration) ChangeState(State.StandUp);
                break;
            case State.StandUp:
                if (stateTimer >= standUpDuration) ChangeState(State.Scream);
                break;
            case State.TakeDamage:
                // short flinch then return to chase loop or other appropriate state
                if (stateTimer >= 0.5f)
                {
                    // After flinch, if dead handled elsewhere; else resume chasing
                    if (currentHealth <= 0) ChangeState(State.Die);
                    else ChangeState(State.ChaseLoop);
                }
                break;
        }

        // Update health float to animator if available
        animationManager?.UpdateHealth(currentHealth / stats.maxHealth);
    }

    private void UpdateIdleSit()
    {
        // If player comes close or idle timer elapsed -> Standing
        if (player != null)
        {
            float d = Vector3.Distance(transform.position, player.position);
            if (d <= detectionRange) { ChangeState(State.Standing); return; }
        }

        if (stateTimer >= idleToStandingDelay)
        {
            ChangeState(State.Standing);
        }
    }

    private void UpdateStanding()
    {
        if (stateTimer >= standingDuration)
        {
            ChangeState(State.Chase);
        }
    }

    private void UpdateChaseLoop()
    {
        if (player == null) return;

        float d = Vector3.Distance(transform.position, player.position);

        // Always chase (per requirement)
        // If in attack range and cooldown ready, pick an attack
        if (d <= stats.attackRange && attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            // If been knocked and in standup flow prefer scream
            if (hasBeenKnocked)
            {
                StartCoroutine(DoScream());
            }
            else
            {
                // random choose between360do and Scream
                if (Random.value < 0.5f) StartCoroutine(Do360do()); else StartCoroutine(DoScream());
            }
        }

        // also check life threshold to trigger knock
        if (!hasBeenKnocked && currentHealth <= stats.maxHealth * 0.5f)
        {
            hasBeenKnocked = true;
            ChangeState(State.KnockFallingBack);
            animationManager?.PlayKnockFallingBack();
            // stop agent for knock
            if (agent != null) agent.isStopped = true;
        }
    }

    private void ChangeState(State newState)
    {
        // Exit actions
        switch (state)
        {
            case State.Chase:
            case State.ChaseLoop:
                if (agent != null) agent.isStopped = true;
                break;
        }

        state = newState;
        stateTimer = 0f;

        // Enter actions
        switch (state)
        {
            case State.IdleSit:
                // Idle animation is assumed to be the default in animator
                break;
            case State.Standing:
                animationManager?.PlayStanding();
                break;
            case State.Chase:
                animationManager?.PlayChasePlayer();
                break;
            case State.ChaseLoop:
                animationManager?.PlayChasePlayer();
                break;
            case State.Attack360:
                animationManager?.Play360do();
                break;
            case State.Scream:
                animationManager?.PlayScream();
                break;
            case State.KnockFallingBack:
                // handled where triggered
                break;
            case State.StandUp:
                animationManager?.PlayStandUp();
                break;
            case State.TakeDamage:
                animationManager?.PlayTakeDamage();
                break;
            case State.Die:
                animationManager?.PlayDie();
                StartCoroutine(HandleDie());
                break;
        }
    }

    private IEnumerator Do360do()
    {
        ChangeState(State.Attack360);
        // allow animation to play (assume1s)
        yield return new WaitForSeconds(1.0f);

        // apply damage if player is within short radius
        if (player != null)
        {
            float d = Vector3.Distance(transform.position, player.position);
            float radius = 4f; // AOE radius for360
            if (d <= radius)
            {
                var dmgTarget = player.GetComponent<ICharacter>();
                dmgTarget?.TakeDamage(stats.damage);
            }
        }

        // back to chase loop
        ChangeState(State.ChaseLoop);
    }

    private IEnumerator DoScream()
    {
        ChangeState(State.Scream);
        // assume scream animation length
        yield return new WaitForSeconds(1.2f);

        // Scream effect: mid-range damage
        if (player != null)
        {
            float d = Vector3.Distance(transform.position, player.position);
            float range = Mathf.Max(6f, stats.attackRange);
            if (d <= range)
            {
                var dmgTarget = player.GetComponent<ICharacter>();
                dmgTarget?.TakeDamage(stats.damage);
            }
        }

        // after scream, return to chase
        ChangeState(State.ChaseLoop);
    }

    public void TakeDamage(float amount)
    {
        // immediate health update
        currentHealth -= amount;
        animationManager?.PlayTakeDamage();

        if (currentHealth <= 0f)
        {
            ChangeState(State.Die);
            return;
        }

        // If recently knocked and now below half, handled elsewhere
        // interrupt to flinch state briefly
        ChangeState(State.TakeDamage);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, stats.maxHealth);
    }

    public void Die()
    {
        ChangeState(State.Die);
    }

    private IEnumerator HandleDie()
    {
        // wait for die animation then destroy object
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}