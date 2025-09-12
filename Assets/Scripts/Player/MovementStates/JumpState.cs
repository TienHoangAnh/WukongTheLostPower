using UnityEngine;

public class JumpState : IMovementState
{
    public void EnterState(PlayerMovementContext context)
    {
        float jumpVelocity = Mathf.Sqrt(context.jumpHeight * -2f * context.gravity);
        context.velocity.y = jumpVelocity;
        context.animator.SetBool("IsJumping", true); // Trigger jump animation
        Debug.Log("🪂 JumpState: Nhảy lên");
    }

    public void UpdateState(PlayerMovementContext context)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = context.transform.right * h + context.transform.forward * v;
        context.characterController.Move(move * context.moveSpeed * Time.deltaTime);

        context.velocity.y += context.gravity * Time.deltaTime;
        context.characterController.Move(context.velocity * Time.deltaTime);

        if (context.isGrounded && context.velocity.y < 0)
        {
            context.animator.SetBool("IsJumping", false); // End jump animation
            context.SwitchState(new WalkState());
        }
    }
}
