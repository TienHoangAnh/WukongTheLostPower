using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    public void EnterState(EnemyAIContext context)
    {
        if (context.agent != null) context.agent.isStopped = true;
        if (context.animator != null)
            context.animator.SetBool(Animator.StringToHash("IsChasing"), false);
    }

    public void UpdateState(EnemyAIContext context)
    {
        if (!context.HasValidTarget()) return;

        if (context.IsPlayerInRange(context.detectDistance))
        {
            context.SwitchState(new EnemyChaseState());
        }
    }

    public void ExitState(EnemyAIContext context) { }
}
