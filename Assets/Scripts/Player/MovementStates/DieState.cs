using UnityEngine;

public class DieState : IMovementState
{
    public void EnterState(PlayerMovementContext context)
    {
        if (context.animator != null)
            context.animator.SetTrigger("Die");

        if (context.characterController != null)
            context.characterController.enabled = false;

        if (SaveRuntime.Current != null)
        {
            SaveRuntime.Current.player.hp = 0;
            SaveRuntime.Current.worldFlags["playerDead"] = true;
            _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
        }

        Debug.Log("💀 Player chết! Đã ghi vào save.");
    }

    public void UpdateState(PlayerMovementContext context) { }
}