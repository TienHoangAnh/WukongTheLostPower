using UnityEngine;

public class IdleState : IMovementState
{
    public void EnterState(PlayerMovementContext context)
    {
        // Debug.Log("Chuyển sang trạng thái ĐỨNG YÊN");
        context.velocity = Vector3.zero;
        context.animator.SetFloat("Speed", 0f); // Set animation speed to idle
    }

    public void UpdateState(PlayerMovementContext context)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Nếu có input di chuyển
        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            // Nếu giữ shift thì chuyển sang RunState
            if (Input.GetKey(KeyCode.LeftShift))
            {
                context.SwitchState(new RunState());
            }
            else
            {
                context.SwitchState(new WalkState());
            }
            return;
        }

        // Gravity để nhân vật không bay
        context.velocity.y += context.gravity * Time.deltaTime;
        context.characterController.Move(context.velocity * Time.deltaTime);
    }
}
