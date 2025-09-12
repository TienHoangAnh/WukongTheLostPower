using UnityEngine;

public interface ISkill
{
    void Use(PlayerMovementContext context);
    string GetName();
}
