using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    public void EnterState(EnemyAIContext context)
    {
        context.agent.isStopped = false;
    }

    public void UpdateState(EnemyAIContext context)
    {
        context.agent.SetDestination(context.player.position);

        if (context.IsPlayerInRange(context.attackRange))
        {
            context.SwitchState(new EnemyAttackState());
        }

        if (!context.IsPlayerInRange(context.chaseRange))
        {
            context.SwitchState(new EnemyIdleState());
        }
    }
    public void ExitState(EnemyAIContext context)
    {
        Debug.Log("Enemy rời trạng thái CHASE");
    }
}
