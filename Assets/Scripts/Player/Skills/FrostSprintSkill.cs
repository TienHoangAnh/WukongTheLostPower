using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Skills/FrostSprintSkill")]
public class FrostSprintSkill : ScriptableObject, ISkill
{
    [Header("Buff")]
    public float speedMultiplier = 1.45f;   // +45% tốc chạy
    public float accelMultiplier = 1.20f;   // +20% gia tốc (tùy bạn dùng)
    public float duration = 4f;

    [Header("Cost & Cooldown")]
    public float manaCost = 20f;
    public float cooldown = 8f;

    private float _lastUsed = -999f;

    public void Use(PlayerMovementContext context)
    {
        if (context == null) return;

        if (Time.time < _lastUsed + cooldown)
        {
            Debug.Log("⏳ FrostSprint đang cooldown!");
            return;
        }

        var stats = context.GetComponent<PlayerStats>();
        if (stats == null) return;

        // Tiêu hao năng lượng
        if (!stats.UseStamina(manaCost))
        {
            Debug.Log($"❌ Không đủ Mana để dùng FrostSprint (-{manaCost})");
            return;
        }

        // Lấy (hoặc tự gắn) controller quản lý buff tốc độ
        var buff = context.GetComponent<SpeedBuffController>();
        if (!buff) buff = context.gameObject.AddComponent<SpeedBuffController>();

        buff.Apply(speedMultiplier, accelMultiplier, duration);
        _lastUsed = Time.time;

        Debug.Log($"❄️ FrostSprint ON | x{speedMultiplier:0.##} speed trong {duration:0.0}s | -{manaCost} MP");

        // (optional) Lưu cooldown còn lại
        if (SaveRuntime.Current != null)
        {
            float remain = Mathf.Max(0, (_lastUsed + cooldown) - Time.time);
            SaveRuntime.Current.skillCooldowns ??= new System.Collections.Generic.Dictionary<string, float>();
            SaveRuntime.Current.skillCooldowns["frost_sprint"] = remain;
            _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
        }
    }

    public string GetName() => "Frost Sprint";
}
