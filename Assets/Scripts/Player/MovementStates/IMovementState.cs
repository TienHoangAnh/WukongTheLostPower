using UnityEngine;

public interface IMovementState
{
    void EnterState(PlayerMovementContext context);
    void UpdateState(PlayerMovementContext context);
    void ExitState(PlayerMovementContext context);
}
