using UnityEngine;

public class HybridBossAttackStrategy : IBossAttackStrategy
{
    public void Configure(BossContext context)
    {
        context.stats.maxHealth += 400f;
        context.stats.damage += 15f;
        context.stats.attackRange = 10f;
        context.agent.speed = context.stats.moveSpeed + 0.5f;
        context.currentHealth = context.stats.maxHealth;
    }

    public void Attack(BossContext context)
    {
        float dist = Vector3.Distance(context.transform.position, context.player.position);

        if (dist <= 4f)
        {
            var player = context.player.GetComponent<ICharacter>();
            if (player != null)
            {
                player.TakeDamage(context.stats.damage);
                Debug.Log($"⚔ Hybrid Boss đánh gần gây {context.stats.damage} sát thương!");
            }
        }
        else if (dist <= context.stats.attackRange)
        {
            GameObject proj = GameObject.Instantiate(context.projectilePrefab, context.transform.position + Vector3.up, Quaternion.identity);
            proj.GetComponent<RangedProjectile>()?.SetTarget(context.player);
            Debug.Log("🎯 Hybrid Boss bắn tầm xa!");
        }
    }
}
