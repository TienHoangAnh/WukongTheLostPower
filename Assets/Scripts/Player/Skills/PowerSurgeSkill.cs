using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Skills/PowerSurgeSkill")]
public class PowerSurgeSkill : ScriptableObject, ISkill
{
    public float duration = 6f;
    public float bonusPercent = 0.25f; // +25% damage
    public float cooldown = 8f;

    private float _lastUsedTime = -999f;

    public void Use(PlayerMovementContext context)
    {
        if (Time.time < _lastUsedTime + cooldown)
        {
            Debug.Log("⏳ PowerSurge đang cooldown!");
            return;
        }

        var stats = context.GetComponent<PlayerStats>();
        if (stats == null) return;

        context.StartCoroutine(BuffRoutine(stats));
        _lastUsedTime = Time.time;
        Debug.Log($"⚡ Power Surge kích hoạt! +{bonusPercent * 100}% Damage trong {duration}s");

        // Save remaining cooldown to runtime and cloud so it persists
        try
        {
            if (SaveRuntime.Current != null)
            {
                float remain = Mathf.Max(0f, (_lastUsedTime + cooldown) - Time.time);
                SaveRuntime.Current.skillCooldowns ??= new System.Collections.Generic.Dictionary<string, float>();
                SaveRuntime.Current.skillCooldowns["power_surge"] = remain;
                _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[PowerSurgeSkill] Failed to save cooldown: {ex.Message}");
        }
    }

    private IEnumerator BuffRoutine(PlayerStats stats)
    {
        stats.baseDamage *= (1f + bonusPercent);
        yield return new WaitForSeconds(duration);
        stats.baseDamage /= (1f + bonusPercent);
        Debug.Log("⏳ Power Surge hết hiệu lực");

        // When buff ends we should also update remaining cooldown (it will be cooldown - elapsed since use).
        try
        {
            if (SaveRuntime.Current != null)
            {
                float remain = Mathf.Max(0f, (_lastUsedTime + cooldown) - Time.time);
                SaveRuntime.Current.skillCooldowns ??= new System.Collections.Generic.Dictionary<string, float>();
                SaveRuntime.Current.skillCooldowns["power_surge"] = remain;
                _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
            }
        }
        catch { }
    }

    public string GetName() => "Power Surge";
}
