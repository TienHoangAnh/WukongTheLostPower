using UnityEngine;

public class RangedBossAttackStrategy : IBossAttackStrategy
{
    public void Configure(BossContext context)
    {
        context.stats.maxHealth += 300f;
        context.stats.damage += 10f;
        context.stats.attackRange = 15f;
        context.agent.speed = context.stats.moveSpeed;
        context.currentHealth = context.stats.maxHealth;
    }

    public void Attack(BossContext context)
    {
        float dist = Vector3.Distance(context.transform.position, context.player.position);
        if (dist <= context.stats.attackRange)
        {
            GameObject proj = GameObject.Instantiate(context.projectilePrefab, context.transform.position + Vector3.up, Quaternion.identity);
            proj.GetComponent<RangedProjectile>()?.SetTarget(context.player);
            Debug.Log("🏹 Ranged Boss tấn công từ xa!");
        }
    }
}
