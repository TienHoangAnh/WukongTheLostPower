using UnityEngine;

[System.Serializable]
public class AttackStep
{
    public float damage;
    public float duration;
    public float inputWindow; // thời gian cho phép nhấn tiếp để combo

    public AttackStep(float damage, float duration, float inputWindow)
    {
        this.damage = damage;
        this.duration = duration;
        this.inputWindow = inputWindow;
    }
}
