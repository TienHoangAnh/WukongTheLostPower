using UnityEngine;

public class BossChaseState : IBossState
{
    public void EnterState(BossContext context)
    {
        context.agent.isStopped = false;
        Debug.Log("🏃 Boss đuổi theo người chơi.");
    }

    public void UpdateState(BossContext context)
    {
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
            context.agent.SetDestination(context.player.position);
        }
    }

    public void ExitState(BossContext context) { }
}
