using UnityEngine;

public class MeleeBossAttackStrategy : IBossAttackStrategy
{
    public void Configure(BossContext context)
    {
        context.stats.maxHealth += 200f;
        context.stats.damage += 20f;
        context.stats.attackRange = 3f;
        context.agent.speed = context.stats.moveSpeed + 1f;
        context.currentHealth = context.stats.maxHealth;
    }

    public void Attack(BossContext context)
    {
        float dist = Vector3.Distance(context.transform.position, context.player.position);
        if (dist <= context.stats.attackRange)
        {
            var player = context.player.GetComponent<ICharacter>();
            if (player != null)
            {
                player.TakeDamage(context.stats.damage);
                Debug.Log($"👊 Melee Boss gây {context.stats.damage} sát thương!");
            }
        }
    }
}
