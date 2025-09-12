using UnityEngine;

public class FlyKickState : IMovementState
{
    public void EnterState(PlayerMovementContext context)
    {
        if (context.animator != null)
            context.animator.SetTrigger("FlyKick");
        Debug.Log("👟 FlyKickState: tung cước bay (animation lo di chuyển).");

        // Có thể set velocity.y nhẹ để hòa nhập với animation, nếu muốn
        // context.velocity.y = 2f;
    }

    public void UpdateState(PlayerMovementContext context)
    {
        // Ở đây chỉ để cho gravity vẫn áp dụng (nếu animation không root motion Y)
        context.velocity.y += context.gravity * Time.deltaTime;
        context.characterController.Move(context.velocity * Time.deltaTime);

        // Khi animation kết thúc, bạn có thể về WalkState hoặc IdleState.
        // Thường dùng Animation Event gọi từ clip để SwitchState an toàn hơn.
    }
}
