using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Settings")]
    public float attackRange = 2f;
    public float attackDamage = 20f;
    public LayerMask enemyLayer;

    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;
    public float spawnRadius = 3f;
    public int numberOfProjectiles = 3;
    public float launchDelay = 0.5f;
    public float attackDistance = 15f;
    public float rangedDamage = 20f;
    public float projectileSpeed = 6f;
    public float rangedStaminaCost = 15f;

    [Range(0f, 720f)] public float turnRate = 360f;
    public float impactRadius = 1.0f;
    public float knockbackForce = 8f;
    public float rangedCooldown = 1.5f;
    public AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("FX (Optional)")]
    public GameObject spawnVFX;
    public GameObject impactVFX;
    public AudioClip spawnSFX;
    public AudioClip impactSFX;

    [Header("Layers/Tags")]
    public LayerMask obstacleMask;
    public string projectileLayerName = "PlayerProjectile";
    public string enemyTag = "Enemy";

    private PlayerBehaviorTracker behaviorTracker;
    private bool rangedOnCooldown;

    public ComboData comboData;
    private float[] comboCooldowns;
    private Animator animator;
    private PlayerStats stats;

    void Start()
    {
        behaviorTracker = FindFirstObjectByType<PlayerBehaviorTracker>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
        comboCooldowns = new float[comboData.comboSteps.Count];
    }

    void Update()
    {
        // Left mouse = Melee
        if (Input.GetMouseButtonDown(0))
            AttackMelee();

        // Ranged = J
        if (Input.GetKeyDown(KeyCode.J))
            TryFireRanged();

        // Combo keys (Q/E/R)
        if (Input.GetKeyDown(KeyCode.Q)) TryUseCombo(0);
        if (Input.GetKeyDown(KeyCode.E)) TryUseCombo(1);
        if (Input.GetKeyDown(KeyCode.R)) TryUseCombo(2);

        stats.RecoverStamina(Time.deltaTime * 2f);
    }

    void TryFireRanged()
    {
        if (rangedOnCooldown) return;
        StartCoroutine(FireRangedAttack());
    }

    void AttackMelee()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward, attackRange, enemyLayer);
        bool hitEnemy = false;

        foreach (var col in hits)
        {
            ICharacter target = col.GetComponent<ICharacter>();
            if (target != null)
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"[Combat] Melee attack dealt {attackDamage} damage to {col.name}");
                hitEnemy = true;
            }
        }

        if (hitEnemy)
            behaviorTracker?.RecordMeleeAttack();
        else
            Debug.Log("[Combat] Melee attack missed all enemies.");
    }

    IEnumerator FireRangedAttack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[Combat] Missing projectilePrefab in Inspector!");
            yield break;
        }

        // 1) Find enemies in range
        Collider[] colHits = Physics.OverlapSphere(transform.position, attackDistance, enemyLayer);
        if (colHits.Length == 0)
        {
            Debug.Log("[Combat] No enemies in ranged attack distance.");
            yield break;
        }

        // Check stamina
        if (!stats || !stats.UseStamina(rangedStaminaCost))
        {
            Debug.Log($"[Combat] Not enough stamina for ranged attack. Required {rangedStaminaCost}, current {stats?.currentStamina:0.0}/{stats?.maxStamina:0.0}");
            yield break;
        }

        var enemies = colHits
            .Select(h => h.transform)
            .Where(t => t != null && t.gameObject.activeInHierarchy)
            .OrderBy(t => (t.position - transform.position).sqrMagnitude)
            .ToList();

        // 2) Spawn projectiles and make them rise up
        Transform player = transform;
        var spawned = new List<GameObject>(numberOfProjectiles);
        var startPos = new Vector3[numberOfProjectiles];
        var endPos = new Vector3[numberOfProjectiles];

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float angle = i * (360f / numberOfProjectiles);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            startPos[i] = player.position + offset * spawnRadius + Vector3.up * 0.25f;
            endPos[i] = startPos[i] + Vector3.up * 2.5f;

            GameObject go = Instantiate(projectilePrefab, startPos[i], Quaternion.identity);
            if (!string.IsNullOrEmpty(projectileLayerName))
            {
                int layer = LayerMask.NameToLayer(projectileLayerName);
                if (layer >= 0) go.layer = layer;
            }

            var clone = go.GetComponent<RangedClone>();
            if (clone == null) clone = go.AddComponent<RangedClone>();

            clone.Init(new RangedClone.Config
            {
                damage = rangedDamage,
                speed = projectileSpeed,
                turnRateDegPerSec = turnRate,
                impactRadius = impactRadius,
                knockbackForce = knockbackForce,
                enemyLayer = enemyLayer,
                obstacleMask = obstacleMask,
                enemyTag = enemyTag,
                impactVFX = impactVFX,
                impactSFX = impactSFX
            });

            spawned.Add(go);

            if (spawnVFX) Instantiate(spawnVFX, startPos[i], Quaternion.identity);
            if (spawnSFX) AudioSource.PlayClipAtPoint(spawnSFX, startPos[i], 0.9f);
        }

        float t = 0f;
        while (t < launchDelay)
        {
            float k = Mathf.Clamp01(t / launchDelay);
            float eased = riseCurve != null ? riseCurve.Evaluate(k) : k;

            for (int i = 0; i < spawned.Count; i++)
                if (spawned[i]) spawned[i].transform.position = Vector3.Lerp(startPos[i], endPos[i], eased);

            t += Time.deltaTime;
            yield return null;
        }

        // 3) Assign targets
        for (int i = 0; i < spawned.Count; i++)
        {
            if (!spawned[i]) continue;
            var clone = spawned[i].GetComponent<RangedClone>();
            if (!clone) continue;

            Transform target = enemies[i % enemies.Count];
            clone.SetTarget(target, transform);
        }

        Debug.Log($"[Combat] Ranged attack fired | -{rangedStaminaCost} stamina | Remaining {stats.currentStamina:0.0}/{stats.maxStamina:0.0} | Cooldown = {rangedCooldown:0.00}s");
        behaviorTracker?.RecordRangedAttack();

        // 4) Cooldown
        rangedOnCooldown = true;
        yield return new WaitForSeconds(rangedCooldown);
        rangedOnCooldown = false;
    }

    void TryUseCombo(int index)
    {
        if (index < 0 || index >= comboData.comboSteps.Count) return;
        AttackStep step = comboData.comboSteps[index];

        if (Time.time < comboCooldowns[index])
        {
            Debug.Log($"[Combat] Skill {step.skillName} is still on cooldown!");
            return;
        }

        if (!stats.UseStamina(step.staminaCost))
        {
            Debug.Log($"[Combat] Not enough stamina for {step.skillName}!");
            return;
        }

        float damage = stats.baseDamage * (1f + step.bonusPercent);

        // Trigger animation
        if (animator != null && !string.IsNullOrEmpty(step.animationName))
            animator.SetTrigger(step.animationName);

        // Deal damage to nearest enemy
        EnemyStats enemy = FindNearestEnemy();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"[Combat] Used {step.skillName} and dealt {damage} damage to {enemy.gameObject.name}");
        }

        comboCooldowns[index] = Time.time + step.cooldown;
    }

    EnemyStats FindNearestEnemy()
    {
        EnemyStats nearest = null;
        float minDist = float.MaxValue;
        foreach (var enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float dist = Vector3.Distance(transform.position, enemyObj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemyObj.GetComponent<EnemyStats>();
            }
        }
        return nearest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, attackRange);

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
