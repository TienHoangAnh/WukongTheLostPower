using UnityEngine;

public class BossAttackState : IBossState
{
    public void EnterState(BossContext context)
    {
        context.agent.isStopped = true;
        Debug.Log("⚔ Boss chuyển sang trạng thái tấn công.");
    }

    public void UpdateState(BossContext context)
    {
        float dist = Vector3.Distance(context.transform.position, context.player.position);

        if (dist > context.stats.attackRange)
        {
            context.SwitchState(new BossChaseState());
            return;
        }

        if (context.attackTimer >= context.stats.attackCooldown)
        {
            context.attackTimer = 0f;
            context.PerformAttack();
        }
    }

    public void ExitState(BossContext context) { }
}
