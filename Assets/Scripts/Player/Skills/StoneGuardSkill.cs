using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Skills/StoneGuardSkill")]
public class StoneGuardSkill : ScriptableObject, ISkill
{
    public float duration = 5f;
    public float damageReduction = 0.4f; // 40%
    public float cooldown = 12f;
    private float _lastUsedTime = -999f;

    public void Use(PlayerMovementContext context)
    {
        if (Time.time < _lastUsedTime + cooldown)
        {
            Debug.Log("⏳ Stone Guard cooldown!");
            return;
        }

        context.StartCoroutine(GuardRoutine(context.GetComponent<PlayerStats>()));
        _lastUsedTime = Time.time;
        Debug.Log("🪨 Stone Guard kích hoạt! Giảm sát thương 40%");
    }

    private IEnumerator GuardRoutine(PlayerStats stats)
    {
        stats.armor += 50f; // tạm thời buff armor
        yield return new WaitForSeconds(duration);
        stats.armor -= 50f;
        Debug.Log("🛡️ Stone Guard hết hiệu lực");
    }

    public string GetName() => "Stone Guard";
}
