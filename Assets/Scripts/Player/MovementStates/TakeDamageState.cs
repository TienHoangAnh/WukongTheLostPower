using UnityEngine;

public class TakeDamageState : IMovementState
{
    private float duration = 0.15f;
    private float elapsed = 0f;

    public void EnterState(PlayerMovementContext context)
    {
        elapsed = 0f;

        context.lastDashTime = Time.time;

        if (context.animator != null)   
            context.animator.SetTrigger("TakeDamage");

        Debug.Log("🏃 TakeDamageState: Nhận sát thương (animation xử lý root motion)");
    }

    public void UpdateState(PlayerMovementContext context)
    {
        elapsed += Time.deltaTime;

        context.velocity.y += context.gravity * Time.deltaTime;
        context.characterController.Move(context.velocity * Time.deltaTime);

        if (elapsed >= duration)
        {
            context.SwitchState(new WalkState());
        }
    }

    public void ExitState(PlayerMovementContext context)
    {
    }
}
