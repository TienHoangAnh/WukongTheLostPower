using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Skills/StoneGuardSkill")]
public class StoneGuardSkill : ScriptableObject, ISkill, ICooldownSkill
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

        // Persist cooldown remaining to save runtime and cloud (same approach as InfernoBurst)
        try
        {
            if (SaveRuntime.Current != null)
            {
                float remain = Mathf.Max(0f, (_lastUsedTime + cooldown) - Time.time);
                SaveRuntime.Current.skillCooldowns ??= new System.Collections.Generic.Dictionary<string, float>();
                SaveRuntime.Current.skillCooldowns["stone_guard"] = remain;
                _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[StoneGuardSkill] Failed to save cooldown: {ex.Message}");
        }
    }

    private IEnumerator GuardRoutine(PlayerStats stats)
    {
        stats.armor += 50f; // tạm thời buff armor
        yield return new WaitForSeconds(duration);
        stats.armor -= 50f;
        Debug.Log("🛡️ Stone Guard hết hiệu lực");
    }

    public string GetName() => "Stone Guard";

    public void RestoreCooldown(float remainingSeconds)
    {
        // restore _lastUsedTime so that remaining matches saved value
        _lastUsedTime = Time.time - (cooldown - remainingSeconds);
    }
}
