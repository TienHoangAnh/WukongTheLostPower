using UnityEngine;

public class RunState : IMovementState
{
    public void EnterState(PlayerMovementContext context)
    {
        // Để Animator tự blend theo input, không set cứng 5f.
        if (context.animator != null)
            context.animator.SetFloat("Speed", 1f); // đánh dấu đang di chuyển nhanh
    }

    public void UpdateState(PlayerMovementContext context)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool hasInput = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
        bool holdingShift = Input.GetKey(KeyCode.LeftShift);

        // Rời Run nếu không giữ shift hoặc không có input
        if (!holdingShift || !hasInput)
        {
            if (!hasInput) context.SwitchState(new IdleState());
            else context.SwitchState(new WalkState());
            return;
        }

        // Camera fallback
        Transform cam = context.cameraDirection != null ? context.cameraDirection : context.transform;

        // Hướng theo camera
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;
        Vector3 camForward = cam.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = cam.right; camRight.y = 0f; camRight.Normalize();

        Vector3 moveDir = camForward * inputDir.z + camRight * inputDir.x;

        // Di chuyển & xoay
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            context.characterController.Move(moveDir * context.runSpeed * Time.deltaTime);
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            context.transform.rotation = Quaternion.Slerp(context.transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        // Cập nhật Animator theo cường độ input (blend mượt)
        if (context.animator != null)
            context.animator.SetFloat("Speed", inputDir.magnitude); // 0..1

        // Gravity
        context.velocity.y += context.gravity * Time.deltaTime;
        context.characterController.Move(context.velocity * Time.deltaTime);

        // Nếu chạm đất và rơi xuống → reset y nhỏ để dính đất (tuỳ ý)
        if (context.isGrounded && context.velocity.y < 0f)
            context.velocity.y = -2f;
    }
}
