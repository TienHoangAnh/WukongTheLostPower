using UnityEngine;

public class HybridBossAttackStrategy : IBossAttackStrategy
{
    private readonly float damageBonus = 0f;
    private readonly float speedBonus = 0.5f;

    public void Configure(BossContext context)
    {
        if (context == null || context.stats == null) return;

        context.agent.speed = context.stats.moveSpeed + speedBonus;
        context.currentHealth = context.stats.maxHealth;

        Debug.Log("[HybridBossAttackStrategy] Configured hybrid boss (using BossStats values, no mutation)");
    }

    public void Attack(BossContext context)
    {
        if (context == null || context.player == null || context.stats == null) return;

        float dist = Vector3.Distance(context.transform.position, context.player.position);

        if (dist <= 4f)
        {
            var player = context.player.GetComponent<ICharacter>();
            if (player != null)
            {
                float finalDamage = context.stats.damage + damageBonus;
                player.TakeDamage(finalDamage);
                Debug.Log($"⚔ Hybrid Boss melee attacks deal {finalDamage} damage!");
            }
        }
        else if (dist <= context.stats.attackRange)
        {
            GameObject proj = GameObject.Instantiate(context.projectilePrefab, context.transform.position + Vector3.up, Quaternion.identity);
            proj.GetComponent<RangedProjectile>()?.SetTarget(context.player);
            Debug.Log("🎯 Hybrid Boss shoots long range!");
        }
    }
}
