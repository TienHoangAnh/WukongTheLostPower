using UnityEngine;

[CreateAssetMenu(menuName = "Skills/HealSkill")]
public class HealSkill : ScriptableObject, ISkill
{
    public float healAmount = 30f;
    public float cooldown = 5f;   // giây
    private float _lastUsedTime = -999f;

    public void Use(PlayerMovementContext context)
    {
        if (context == null) return;

        // Kiểm tra cooldown
        if (Time.time < _lastUsedTime + cooldown)
        {
            Debug.Log("⏳ HealSkill đang cooldown!");
            return;
        }

        var character = context.GetComponent<ICharacter>();
        if (character != null)
        {
            character.Heal(healAmount);
            Debug.Log($"💚 Hồi máu {healAmount} HP cho Player");
        }

        _lastUsedTime = Time.time;

        // PATCH: nếu muốn lưu cooldown còn lại
        if (SaveRuntime.Current != null)
        {
            float remain = Mathf.Max(0, (_lastUsedTime + cooldown) - Time.time);
            if (SaveRuntime.Current.skillCooldowns == null)
                SaveRuntime.Current.skillCooldowns = new System.Collections.Generic.Dictionary<string, float>();

            SaveRuntime.Current.skillCooldowns["heal"] = remain;
            _ = CloudSaveManager.SaveNow(SaveRuntime.Current); // autosave fire-and-forget
        }
    }

    public string GetName() => "Heal";
}
