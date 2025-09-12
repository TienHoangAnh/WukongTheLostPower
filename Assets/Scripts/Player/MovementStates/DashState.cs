using UnityEngine;

public class DashState : IMovementState
{
    private float dashTime = 0.15f;
    private float elapsed = 0f;

    public void EnterState(PlayerMovementContext context)
    {
        elapsed = 0f;

        // Đánh dấu cooldown
        context.lastDashTime = Time.time;

        // Chỉ trigger animation, không Move
        if (context.animator != null)
            context.animator.SetTrigger("Dash");

        Debug.Log("🏃 DashState: Né (animation xử lý root motion)");
    }

    public void UpdateState(PlayerMovementContext context)
    {
        elapsed += Time.deltaTime;

        // Gravity vẫn áp dụng (tuỳ animation có root motion Y hay không)
        context.velocity.y += context.gravity * Time.deltaTime;
        context.characterController.Move(context.velocity * Time.deltaTime);

        // Kết thúc dash sau thời gian định sẵn
        if (elapsed >= dashTime)
        {
            context.SwitchState(new WalkState());
        }
    }

    public void ExitState(PlayerMovementContext context)
    {
        // Optional: reset flag, effect...
    }
}
