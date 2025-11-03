using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    public void EnterState(EnemyAIContext context)
    {
        if (context.agent != null) context.agent.isStopped = true;
        if (context.animator != null)
            context.animator.SetBool(Animator.StringToHash("IsChasing"), false);
        Debug.Log("[EnemyAttackState] Enter ATTACK");
    }

    public void UpdateState(EnemyAIContext context)
    {
        if (!context.HasValidTarget())
        {
            context.SwitchState(new EnemyIdleState());
            return;
        }

        bool inDetect = context.IsPlayerInRange(context.detectDistance);
        bool inAttack = context.IsPlayerInRange(context.attackDistance);

        if (!inDetect)
        {
            context.SwitchState(new EnemyIdleState());
            return;
        }

        if (!inAttack)
        {
            context.SwitchState(new EnemyChaseState());
            return;
        }

        context.FaceTargetFlat();

        if (context.CanAttackNow())
        {
            int attackIndex = (Random.value <= context.chanceAttack2) ? 2 : 1;
            context.TriggerAttackAnimationAndRegisterIndex(attackIndex);
            context.PerformAttackHit();
        }
    }

    public void ExitState(EnemyAIContext context)
    {
        if (context.agent != null) context.agent.isStopped = false;
        Debug.Log("[EnemyAttackState] Exit ATTACK");
    }
}
