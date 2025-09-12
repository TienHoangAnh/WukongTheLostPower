using UnityEngine;

public class BossIdleState : IBossState
{
    public void EnterState(BossContext context)
    {
        context.agent.isStopped = true;
        Debug.Log("🛑 Boss vào trạng thái Idle.");
    }

    public void UpdateState(BossContext context)
    {
        float dist = Vector3.Distance(context.transform.position, context.player.position);
        if (dist <= context.stats.detectionRange)
        {
            context.SwitchState(new BossChaseState());
        }
    }

    public void ExitState(BossContext context) { }
}
