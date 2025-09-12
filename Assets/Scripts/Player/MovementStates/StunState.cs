using UnityEngine;

public class StunState : IMovementState
{
    public void EnterState(PlayerMovementContext context)
    {
        context.animator.SetTrigger("Stun");
        // Có thể disable di chuyển, tấn công ở đây
        context.characterController.enabled = false; // Tắt CharacterController để không di chuyển
        context.velocity = Vector3.zero; // Đặt vận tốc về 0
        Debug.Log("Chuyển sang trạng thái BỊ CHOÁNG");
    }

    public void UpdateState(PlayerMovementContext context)
    {
        // Chờ animation kết thúc hoặc timer, sau đó chuyển về Idle/Walk
        // Ví dụ: nếu animation stun kéo dài 1 giây
        if (context.animator.GetCurrentAnimatorStateInfo(0).IsName("Stun") &&
            context.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            // Animation đã kết thúc, chuyển về Idle hoặc Walk
            if (Input.GetKey(KeyCode.LeftShift))
            {
                context.SwitchState(new RunState());
            }
            else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
            {
                context.SwitchState(new WalkState());
            }
            else
            {
                context.SwitchState(new IdleState());
            }
        }
    }
}