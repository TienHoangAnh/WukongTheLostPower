//using UnityEngine;

//public class MeleeBossAttackStrategy : IBossAttackStrategy
//{
//    // instance-only modifiers (do not change ScriptableObject asset)
//    private readonly float damageBonus = 0f; // keep0; change if you want strategy-specific boost
//    private readonly float speedBonus = 1f; // melee boss moves slightly faster

//    public void Configure(BossContext context)
//    {
//        if (context == null || context.stats == null) return;

//        // Do NOT modify context.stats (ScriptableObject). Use runtime/instance adjustments only.
//        context.agent.speed = context.stats.moveSpeed + speedBonus;
//        context.currentHealth = context.stats.maxHealth;

//        Debug.Log("[MeleeBossAttackStrategy] Configured melee boss (using BossStats values, no mutation)");
//    }

//    public void Attack(BossContext context)
//    {
//        if (context == null || context.player == null || context.stats == null) return;

//        float dist = Vector3.Distance(context.transform.position, context.player.position);
//        if (dist <= context.stats.attackRange)
//        {
//            var player = context.player.GetComponent<ICharacter>();
//            if (player != null)
//            {
//                float finalDamage = context.stats.damage + damageBonus;
//                player.TakeDamage(finalDamage);
//                Debug.Log($"👊 Melee Boss deals {finalDamage} damage!");
//            }
//        }
//    }
//}
