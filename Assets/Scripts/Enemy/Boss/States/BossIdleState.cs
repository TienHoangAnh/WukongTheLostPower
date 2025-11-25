using UnityEngine;

public class BossIdleState : IBossState
{
    public void EnterState(BossContext context)
    {
        context.agent.isStopped = true;
        Debug.Log("🛑 Boss enters Idle state.");
    }

    public void UpdateState(BossContext context)
    {
        if (context.player == null) return; // guard: player may not be available yet

        float dist = Vector3.Distance(context.transform.position, context.player.position);
        if (dist <= context.stats.detectionRange)
        {
            context.SwitchState(new BossChaseState());
        }
    }

    public void ExitState(BossContext context) { }
}
