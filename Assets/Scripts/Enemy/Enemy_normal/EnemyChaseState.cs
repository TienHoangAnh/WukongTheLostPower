using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    public void EnterState(EnemyAIContext context)
    {
        if (context.agent != null)
        {
            context.agent.isStopped = false;
            context.agent.speed = context.chaseSpeed;
        }
        if (context.animator != null)
            context.animator.SetBool(Animator.StringToHash("IsChasing"), true);
    }

    public void UpdateState(EnemyAIContext context)
    {
        if (!context.HasValidTarget())
        {
            context.SwitchState(new EnemyIdleState());
            return;
        }

        var target = context.GetPlayerTransform();

        // Hết vùng detect → Idle
        if (!context.IsPlayerInRange(context.detectDistance))
        {
            context.SwitchState(new EnemyIdleState());
            return;
        }

        // Trong tầm đánh → chuyển Attack
        if (context.IsPlayerInRange(context.attackDistance))
        {
            if (context.agent != null) context.agent.isStopped = true;
            context.SwitchState(new EnemyAttackState());
            return;
        }

        // Tiếp tục đuổi
        if (context.agent != null && context.agent.isOnNavMesh)
        {
            context.agent.isStopped = false;
            context.agent.SetDestination(target.position);
        }
    }

    public void ExitState(EnemyAIContext context) { }
}
