using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Settings")]
    public float attackRange = 5f;
    public float attackDamage = 10f;
    public LayerMask enemyLayer;

    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;          // Prefab phân thân (có RangedClone)
    public float spawnRadius = 1.5f;
    public int numberOfProjectiles = 3;
    public float launchDelay = 1.0f;             // thời gian “trồi lên”
    public float attackDistance = 15f;           // phạm vi tìm enemy
    public float rangedDamage = 14f;             // damage khi va chạm
    public float projectileSpeed = 12f;          // tốc độ lao
    [Range(0f, 720f)] public float turnRate = 360f; // tốc độ quay đầu homing (deg/s)
    public float impactRadius = 1.0f;            // bán kính nổ khi chạm
    public float knockbackForce = 8f;            // lực hất
    public float rangedCooldown = 2.0f;          // CD phím J
    public AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("FX (Optional)")]
    public GameObject spawnVFX;
    public GameObject impactVFX;
    public AudioClip spawnSFX;
    public AudioClip impactSFX;

    [Header("Layers/Tags")]
    public LayerMask obstacleMask;               // nếu muốn kiểm LOS
    public string projectileLayerName = "PlayerProjectile";
    public string enemyTag = "Enemy";            // nếu muốn lọc tag

    private PlayerBehaviorTracker behaviorTracker;
    private bool rangedOnCooldown;

    void Start()
    {
        behaviorTracker = FindFirstObjectByType<PlayerBehaviorTracker>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            AttackMelee();

        if (Input.GetKeyDown(KeyCode.J))
            TryFireRanged();
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
                Debug.Log($"🗡 Cận chiến gây {attackDamage} sát thương lên {col.name}");
                hitEnemy = true;
            }
        }

        if (hitEnemy) behaviorTracker?.RecordMeleeAttack();
        else Debug.Log("🛡 Cận chiến không trúng kẻ địch nào");
    }

    IEnumerator FireRangedAttack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("❌ projectilePrefab chưa được gán trong Inspector!");
            yield break;
        }

        // 1) Tìm enemy trong phạm vi, sắp xếp theo khoảng cách (gần nhất trước)
        Collider[] colHits = Physics.OverlapSphere(transform.position, attackDistance, enemyLayer);
        if (colHits.Length == 0)
        {
            Debug.Log("❌ Không có enemy trong phạm vi tầm xa");
            yield break;
        }

        var enemies = colHits
            .Select(h => h.transform)
            .Where(t => t != null && t.gameObject.activeInHierarchy)
            .OrderBy(t => (t.position - transform.position).sqrMagnitude)
            .ToList();

        // 2) Spawn phân thân xung quanh người chơi + pha trồi lên
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

            // đảm bảo có RangedClone
            var clone = go.GetComponent<RangedClone>();
            if (clone == null) clone = go.AddComponent<RangedClone>();

            // cấu hình cơ bản cho clone (chưa set target lúc này)
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

        // Pha trồi lên (clone CHỜ target, KHÔNG tự hủy)
        float t = 0f;
        while (t < launchDelay)
        {
            float k = Mathf.Clamp01(t / launchDelay);
            float eased = riseCurve != null ? riseCurve.Evaluate(k) : k;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i])
                    spawned[i].transform.position = Vector3.Lerp(startPos[i], endPos[i], eased);
            }
            t += Time.deltaTime;
            yield return null;
        }

        // 3) Gán mục tiêu cho từng phân thân (nếu enemy ít, sẽ lặp lại)
        for (int i = 0; i < spawned.Count; i++)
        {
            if (!spawned[i]) continue;
            var clone = spawned[i].GetComponent<RangedClone>();
            if (!clone) continue;

            Transform target = enemies[i % enemies.Count];
            clone.SetTarget(target, transform);
        }

        Debug.Log("🎯 Đòn tầm xa đã được kích hoạt");
        behaviorTracker?.RecordRangedAttack();

        // 4) Cooldown
        rangedOnCooldown = true;
        yield return new WaitForSeconds(rangedCooldown);
        rangedOnCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, attackRange);

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
