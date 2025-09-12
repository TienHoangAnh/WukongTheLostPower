using UnityEngine;

public interface IMovementState
{
    void EnterState(PlayerMovementContext context);
    void UpdateState(PlayerMovementContext context);
}
