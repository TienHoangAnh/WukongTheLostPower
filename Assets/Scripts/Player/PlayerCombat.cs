using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Settings")]
    public float attackRange = 5f;
    public float attackDamage = 10f;
    public LayerMask enemyLayer;

    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;
    public float spawnRadius = 1.5f;
    public int numberOfProjectiles = 3;
    public float launchDelay = 1.5f; // Thời gian trồi lên
    public float attackDistance = 15f; // Phạm vi tìm enemy

    private PlayerBehaviorTracker behaviorTracker;

    void Start()
    {
        behaviorTracker = FindFirstObjectByType<PlayerBehaviorTracker>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AttackMelee();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(FireRangedAttack());
        }
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

        if (hitEnemy)
        {
            behaviorTracker?.RecordMeleeAttack();
        }
        else
        {
            Debug.Log("🛡 Cận chiến không trúng kẻ địch nào");
        }
    }

    IEnumerator FireRangedAttack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("❌ projectilePrefab chưa được gán trong Inspector!");
            yield break;
        }

        Transform player = transform;

        GameObject[] spawned = new GameObject[numberOfProjectiles];
        Vector3[] startPositions = new Vector3[numberOfProjectiles];
        Vector3[] endPositions = new Vector3[numberOfProjectiles];

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            Vector3 offset = Quaternion.Euler(0, i * (360 / numberOfProjectiles), 0) * Vector3.forward;
            startPositions[i] = player.position + offset * spawnRadius + Vector3.up * 1.5f; // Sửa lại spawn phía trên mặt đất
            endPositions[i] = startPositions[i] + Vector3.up * 2.5f;

            spawned[i] = Instantiate(projectilePrefab, startPositions[i], Quaternion.identity);
        }

        float elapsed = 0f;
        while (elapsed < launchDelay)
        {
            for (int i = 0; i < spawned.Length; i++)
            {
                if (spawned[i] != null)
                {
                    spawned[i].transform.position = Vector3.Lerp(startPositions[i], endPositions[i], elapsed / launchDelay);
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        Collider[] hits = Physics.OverlapSphere(player.position, attackDistance, enemyLayer);
        if (hits.Length > 0)
        {
            Transform target = hits[0].transform;

            foreach (var cube in spawned)
            {
                if (cube != null)
                {
                    RangedProjectile proj = cube.GetComponent<RangedProjectile>();
                    if (proj != null)
                        proj.SetTarget(target);
                }
            }

            Debug.Log("🎯 Đòn tầm xa đã được kích hoạt");
            behaviorTracker?.RecordRangedAttack();
        }
        else
        {
            Debug.Log("❌ Không có enemy trong phạm vi tầm xa");
            foreach (var cube in spawned)
            {
                if (cube != null)
                    Destroy(cube);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, attackRange);
    }
}
