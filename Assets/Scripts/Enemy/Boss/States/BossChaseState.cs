using UnityEngine;

public class BossChaseState : IBossState
{
    public void EnterState(BossContext context)
    {
        if (context.agent != null)
            context.agent.isStopped = false;
        Debug.Log("🏃 Boss chases the player.");
    }

    public void UpdateState(BossContext context)
    {
        if (context == null) return;
        if (context.player == null || context.stats == null)
        {
            Debug.LogWarning("[BossChaseState] Missing player or stats, switching to Idle.");
            context.SwitchState(new BossIdleState());
            return;
        }

        float dist = Vector3.Distance(context.transform.position, context.player.position);

        if (dist > context.stats.detectionRange)
        {
            context.SwitchState(new BossIdleState());
        }
        else if (dist <= context.stats.attackRange)
        {
            context.SwitchState(new BossAttackState());
        }
        else
        {
            if (context.agent != null)
                context.agent.SetDestination(context.player.position);
        }
    }

    public void ExitState(BossContext context) { }
}
