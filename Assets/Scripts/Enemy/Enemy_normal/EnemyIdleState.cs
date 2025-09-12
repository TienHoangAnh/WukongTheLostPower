using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    public void EnterState(EnemyAIContext context)
    {
        context.agent.isStopped = true;
    }

    public void UpdateState(EnemyAIContext context)
    {
        if (context.IsPlayerInRange(context.chaseRange))
        {
            context.SwitchState(new EnemyChaseState());
        }
    }
    public void ExitState(EnemyAIContext context)
    {
        Debug.Log("Enemy rời trạng thái CHASE");
    }
}
