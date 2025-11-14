using UnityEngine;

[System.Serializable]
public class AttackStep
{
    public string skillName;
    public float bonusPercent;
    public float staminaCost;
    public float cooldown;
    public string animationName;
    public float damage;
    public float duration;
    public float inputWindow;

    public AttackStep(float damage, float duration, float inputWindow)
    {
        this.damage = damage;
        this.duration = duration;
        this.inputWindow = inputWindow;
    }
}
