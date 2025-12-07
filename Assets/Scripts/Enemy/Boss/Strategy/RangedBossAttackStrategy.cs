using UnityEngine;

public class RangedBossAttackStrategy : IBossAttackStrategy
{
    private readonly float damageBonus =0f;
    private readonly float speedBonus =0f; // ranged uses base speed

    public void Configure(BossContext context)
    {
        if (context == null || context.stats == null) return;

        // Use stats for runtime values; do not mutate the ScriptableObject asset
        context.agent.speed = context.stats.moveSpeed + speedBonus;
        context.currentHealth = context.stats.maxHealth;

        Debug.Log("[RangedBossAttackStrategy] Configured ranged boss (using BossStats values, no mutation)");
    }

    public void Attack(BossContext context)
    {
        if (context == null || context.player == null || context.stats == null) return;

        float dist = Vector3.Distance(context.transform.position, context.player.position);
        if (dist <= context.stats.attackRange)
        {
            GameObject proj = GameObject.Instantiate(context.projectilePrefab, context.transform.position + Vector3.up, Quaternion.identity);
            proj.GetComponent<RangedProjectile>()?.SetTarget(context.player);
            Debug.Log("🏹 Ranged Boss attacks from a distance!");
        }
    }
}
