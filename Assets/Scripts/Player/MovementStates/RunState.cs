using UnityEngine;

public class RunState : IMovementState
{
    public void EnterState(PlayerMovementContext context)
    {
        if (context.animator != null)
            context.animator.SetFloat("Speed", 1f);
        Debug.Log("[RunState] Entered running state.");
    }

    public void UpdateState(PlayerMovementContext context)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool hasInput = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
        bool holdingShift = Input.GetKey(KeyCode.LeftShift);

        // Exit Run if not holding Shift or no input
        if (!holdingShift || !hasInput)
        {
            if (!hasInput)
            {
                Debug.Log("[RunState] No player detected — switching to IdleState.");
                context.SwitchState(new IdleState());
            }
            else
            {
                Debug.Log("[RunState] Shift released — switching to WalkState.");
                context.SwitchState(new WalkState());
            }
            return;
        }

        // Camera-based movement direction
        Transform cam = context.cameraDirection != null ? context.cameraDirection : context.transform;
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;
        Vector3 camForward = cam.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = cam.right; camRight.y = 0f; camRight.Normalize();
        Vector3 moveDir = camForward * inputDir.z + camRight * inputDir.x;

        // Movement & rotation
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            context.characterController.Move(moveDir * context.runSpeed * Time.deltaTime);
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            context.transform.rotation = Quaternion.Slerp(context.transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        // Update animator blend
        if (context.animator != null)
            context.animator.SetFloat("Speed", inputDir.magnitude);

        // Apply gravity
        context.velocity.y += context.gravity * Time.deltaTime;
        context.characterController.Move(context.velocity * Time.deltaTime);

        if (context.isGrounded && context.velocity.y < 0f)
            context.velocity.y = -2f;
    }

    public void ExitState(PlayerMovementContext ctx)
    {
        Debug.Log("[RunState] Exited running state.");
    }
}
