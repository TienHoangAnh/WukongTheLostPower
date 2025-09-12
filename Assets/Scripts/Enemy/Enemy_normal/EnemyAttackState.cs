using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    float attackCooldown = 1.5f;
    float timer = 0.5f;

    public void EnterState(EnemyAIContext context)
    {
        if (context.agent != null)
            context.agent.isStopped = true;

        timer = attackCooldown; // Cho phép đánh ngay
        //Debug.Log("Enemy vào trạng thái ATTACK");
    }

    public void UpdateState(EnemyAIContext context)
    {
        if (!context.HasValidTarget())
        {
            context.SwitchState(new EnemyIdleState());
            return;
        }

        if (!context.IsPlayerInRange(context.attackRange))
        {
            context.SwitchState(new EnemyChaseState());
            return;
        }

        timer += Time.deltaTime;

        if (timer >= attackCooldown)
        {
            timer = 0f;

            Transform playerTf = context.GetPlayerTransform();
            if (playerTf == null) return;

            ICharacter player = playerTf.GetComponent<ICharacter>();
            if (player != null)
            {
                player.TakeDamage(context.damage);
                Debug.Log("Enemy đã tấn công Player!");
            }

            if (context.animator != null)
            {
                context.animator.SetTrigger("Attack");
            }
        }
    }

    public void ExitState(EnemyAIContext context)
    {
        if (context.agent != null)
            context.agent.isStopped = false;

        //Debug.Log("Enemy rời trạng thái ATTACK");
    }
}
