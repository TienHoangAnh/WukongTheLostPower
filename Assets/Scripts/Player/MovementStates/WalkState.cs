using UnityEngine;

public class WalkState : IMovementState
{
    public void EnterState(PlayerMovementContext context)
    {
        // An toàn null cho Animator
        if (context.animator != null)
            context.animator.SetFloat("Speed", 0.5f); // gợi ý blend, không cần cứng 1f
    }

    public void UpdateState(PlayerMovementContext context)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool hasInput = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        // Không có input → Idle
        if (!hasInput)
        {
            context.SwitchState(new IdleState());
            return;
        }

        // Shift + có input → Run
        if (Input.GetKey(KeyCode.LeftShift))
        {
            context.SwitchState(new RunState());
            return;
        }

        // Fallback camera nếu thiếu
        Transform cam = context.cameraDirection != null ? context.cameraDirection : context.transform;

        // Hướng theo camera
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        Vector3 camForward = cam.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = cam.right; camRight.y = 0f; camRight.Normalize();

        Vector3 moveDir = camForward * inputDir.z + camRight * inputDir.x;

        // Di chuyển
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            context.characterController.Move(moveDir * context.moveSpeed * Time.deltaTime);

            // Xoay Player theo hướng di chuyển
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            context.transform.rotation = Quaternion.Slerp(context.transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        // Cập nhật Animator theo cường độ input (blend mượt)
        if (context.animator != null)
            context.animator.SetFloat("Speed", inputDir.magnitude * 0.5f); // 0..0.5 ~ walk

        // Gravity
        context.velocity.y += context.gravity * Time.deltaTime;
        context.characterController.Move(context.velocity * Time.deltaTime);

        if (context.isGrounded && context.velocity.y < 0f)
            context.velocity.y = -2f;
    }
}
