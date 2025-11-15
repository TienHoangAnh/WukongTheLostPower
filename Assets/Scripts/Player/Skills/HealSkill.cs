using UnityEngine;

[CreateAssetMenu(menuName = "Skills/HealSkill")]
public class HealSkill : ScriptableObject, ISkill
{
    public float healAmount = 30f;
    public float cooldown = 10f;
    public float staminaCost = 35f;
    private float _lastUsedTime = -999f;

    public void Use(PlayerMovementContext context)
    {
        if (context == null) return;

        if (Time.time < _lastUsedTime + cooldown)
        {
            Debug.Log("⏳ HealSkill đang cooldown!");
            return;
        }

        var stats = context.GetComponent<PlayerStats>();
        if (stats == null) return;

        // ⚠️ Kiểm tra đủ năng lượng
        if (!stats.UseStamina(staminaCost))
        {
            Debug.Log($"❌ Không đủ năng lượng để dùng Heal ({staminaCost} MP)!");
            return;
        }

        var character = context.GetComponent<ICharacter>();
        if (character != null)
        {
            character.Heal(healAmount);
            Debug.Log($"💚 Hồi {healAmount} HP (-{staminaCost} Mana)");
        }

        _lastUsedTime = Time.time;

        // Lưu cooldown nếu cần
        if (SaveRuntime.Current != null)
        {
            float remain = Mathf.Max(0, (_lastUsedTime + cooldown) - Time.time);
            SaveRuntime.Current.skillCooldowns["heal"] = remain;
            _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
        }
    }

    public string GetName() => "Heal";
}
