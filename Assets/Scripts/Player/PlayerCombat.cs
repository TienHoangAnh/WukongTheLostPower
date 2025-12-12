using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;
    public float spawnRadius = 5f;
    public int numberOfProjectiles = 3;
    public float launchDelay = 0.5f;
    public float attackDistance = 15f;
    public float rangedDamage = 20f;
    public float projectileSpeed = 6f;
    public float rangedStaminaCost = 5f;

    [Range(0f, 720f)] public float turnRate = 360f;
    public float impactRadius = 1.0f;
    public float knockbackForce = 8f;
    public float rangedCooldown = 1.5f;
    public AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Layers/Tags")]
    public LayerMask enemyLayer;
    public LayerMask obstacleMask;
    public string projectileLayerName = "PlayerProjectile";
    public string enemyTag = "Enemy";

    [Header("Combo Settings (Melee Skills Q/E/R)")]
    public float comboRange = 4f;

    private PlayerBehaviorTracker behaviorTracker;
    private bool rangedOnCooldown;

    public ComboData comboData;
    private float[] comboCooldowns;
    private Animator animator;
    private PlayerStats stats;

    private float _rangedCooldownEndTime = -999f;

    void Start()
    {
        behaviorTracker = FindFirstObjectByType<PlayerBehaviorTracker>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();

        if (comboData != null && comboData.comboSteps != null)
            comboCooldowns = new float[comboData.comboSteps.Count];
        else
            comboCooldowns = new float[0];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            TryFireRanged();

        if (Input.GetKeyDown(KeyCode.Q)) TryUseCombo(0);
        if (Input.GetKeyDown(KeyCode.E)) TryUseCombo(1);
        if (Input.GetKeyDown(KeyCode.R)) TryUseCombo(2);

        if (stats != null)
            stats.RecoverStamina(Time.deltaTime * 2f);
    }

    //ranged attack (J)
    void TryFireRanged()
    {
        if (!rangedOnCooldown)
            StartCoroutine(FireRangedAttack());
    }

    IEnumerator FireRangedAttack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[Combat] Missing projectilePrefab in Inspector!");
            yield break;
        }

        Collider[] colHits = Physics.OverlapSphere(transform.position, attackDistance, enemyLayer);
        if (colHits.Length == 0)
        {
            Debug.Log($"[Combat] No enemies in ranged attack distance ({attackDistance}m).");
            yield break;
        }

        if (!stats || !stats.UseStamina(rangedStaminaCost))
        {
            Debug.Log($"[Combat] Not enough stamina for ranged attack ({rangedStaminaCost}).");
            yield break;
        }

        var enemies = colHits
            .Select(h => h.transform)
            .Where(t => t != null && t.gameObject.activeInHierarchy)
            .OrderBy(t => (t.position - transform.position).sqrMagnitude)
            .ToList();

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
                impactVFX = null,
                impactSFX = null
            });

            spawned.Add(go);
        }

        float t = 0f;
        while (t < launchDelay)
        {
            float k = Mathf.Clamp01(t / launchDelay);
            float eased = riseCurve != null ? riseCurve.Evaluate(k) : k;

            for (int i = 0; i < spawned.Count; i++)
                if (spawned[i])
                    spawned[i].transform.position = Vector3.Lerp(startPos[i], endPos[i], eased);

            t += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < spawned.Count; i++)
        {
            if (!spawned[i]) continue;

            var clone = spawned[i].GetComponent<RangedClone>();
            if (!clone) continue;

            Transform target = enemies[i % enemies.Count];
            clone.SetTarget(target, transform);
        }

        behaviorTracker?.RecordRangedAttack();

        // set cooldown state and track absolute end time
        rangedOnCooldown = true;
        _rangedCooldownEndTime = Time.time + rangedCooldown;
        Debug.Log($"[PlayerCombat] Ranged started cooldown. duration={rangedCooldown:F2}s, endsAt={_rangedCooldownEndTime:F2}, now={Time.time:F2}");

        yield return new WaitForSeconds(rangedCooldown);
        rangedOnCooldown = false;
        _rangedCooldownEndTime = -999f;
        Debug.Log("[PlayerCombat] Ranged cooldown ended.");
    }

    // COMBO (Q/E/R)
    void TryUseCombo(int index)
    {
        if (comboData == null || comboData.comboSteps == null) return;
        if (index < 0 || index >= comboData.comboSteps.Count) return;

        AttackStep step = comboData.comboSteps[index];

        if (Time.time < comboCooldowns[index])
        {
            Debug.Log($"[Combat] Skill {step.skillName} is still on cooldown! " + Time.time);
            return;
        }

        EnemyStats enemy = FindNearestEnemyInRange(comboRange);
        if (enemy == null)
        {
            Debug.Log($"[Combat] {step.skillName} failed: no enemy in range ({comboRange}m).");
            return;
        }

        if (!stats.UseStamina(step.staminaCost))
        {
            Debug.Log($"[Combat] Not enough stamina for {step.skillName}!");
            return;
        }

        float damage = stats.baseDamage * (1f + step.bonusPercent);

        if (animator != null && !string.IsNullOrEmpty(step.animationName))
            animator.SetTrigger(step.animationName);

        behaviorTracker?.RecordMeleeAttack();

        enemy.TakeDamage(damage);
        Debug.Log($"[Combat] Used {step.skillName} → {damage} damage to {enemy.gameObject.name}");

        comboCooldowns[index] = Time.time + step.cooldown;
        Debug.Log($"[PlayerCombat] Combo skill '{step.skillName}' started cooldown. index={index}, duration={step.cooldown:F2}s, endsAt={comboCooldowns[index]:F2}, now={Time.time:F2}");
    }

    EnemyStats FindNearestEnemyInRange(float maxRange)
    {
        EnemyStats nearest = null;
        float minDist = maxRange;

        foreach (var enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (!enemyObj.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, enemyObj.transform.position);
            if (dist <= maxRange && dist < minDist)
            {
                minDist = dist;
                nearest = enemyObj.GetComponent<EnemyStats>();
            }
        }
        return nearest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, comboRange);

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    // Public accessors to expose cooldown remaining values for UI
    public float GetComboCooldownRemaining(int index)
    {
        if (comboCooldowns == null || index < 0 || index >= comboCooldowns.Length) return 0f;
        float rem = comboCooldowns[index] - Time.time;
        return rem > 0f ? rem : 0f;
    }

    public float GetRangedCooldownRemaining()
    {
        if (_rangedCooldownEndTime < 0f) return 0f;
        float rem = _rangedCooldownEndTime - Time.time;
        return rem > 0f ? rem : 0f;
    }

    public float GetComboMaxCooldown(int index)
    {
        if (comboData == null || comboData.comboSteps == null || index < 0 || index >= comboData.comboSteps.Count) return 0f;
        return comboData.comboSteps[index].cooldown;
    }
    public float GetRangedMaxCooldown() => rangedCooldown;

    public bool IsRangedOnCooldown() => rangedOnCooldown;
}

public class SomeOtherClass : MonoBehaviour
{
    public PlayerCombat playerCombat;
    public Text qText;
    public Image qFillImage;

    void Update()
    {
        float rem = playerCombat.GetComboCooldownRemaining(0);
        float max = playerCombat.GetComboMaxCooldown(0);
        qText.text = rem > 0 ? rem.ToString("F1") + "s" : "";
        if (qFillImage != null) qFillImage.fillAmount = max > 0 ? rem / max : 0f;
    }
}
