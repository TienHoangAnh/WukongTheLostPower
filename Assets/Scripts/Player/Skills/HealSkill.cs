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
            Debug.Log("⏳ HealSkill đang cooldown! : " + Time.time );
            return;
        }

        var stats = context.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (!stats.UseStamina(staminaCost))
        {
            Debug.Log($"❌ Not enough energy to use Heal ({staminaCost} Stamina)!");
            return;
        }

        var character = context.GetComponent<ICharacter>();
        if (character != null)
        {
            character.Heal(healAmount);
            Debug.Log($"💚 Heal {healAmount} HP (-{staminaCost} Stamina)");
        }

        _lastUsedTime = Time.time;

        if (SaveRuntime.Current != null)
        {
            float remain = Mathf.Max(0, (_lastUsedTime + cooldown) - Time.time);
            SaveRuntime.Current.skillCooldowns["heal"] = remain;
            _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
        }
    }

    public string GetName() => "Heal";
}
