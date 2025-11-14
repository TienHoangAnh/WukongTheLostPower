using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour, ICharacter, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxStamina = 100f;
    public float currentStamina;
    public float baseDamage = 20f;
    public float armor = 5f;

    private PlayerMovementContext _moveCtx;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        _moveCtx = GetComponent<PlayerMovementContext>()
                   ?? FindFirstObjectByType<PlayerMovementContext>();
    }

    public void TakeDamage(float amount)
    {
        float finalDamage = Mathf.Max(1f, amount - armor);
        currentHealth -= finalDamage;

        Debug.Log($"[PlayerStats] Player took {amount} damage (armor {armor}) → final {finalDamage}. Current HP: {currentHealth}");

        _moveCtx?.TakeDamage();

        if (currentHealth <= 0) Die();
    }

    public void TakeDamage(int amount) => TakeDamage((float)amount);

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"[PlayerStats] Player healed {amount} HP. Current HP: {currentHealth}");
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina < amount)
        {
            Debug.Log($"[PlayerStats] Not enough stamina ({currentStamina}/{amount}) to perform action.");
            return false;
        }

        currentStamina -= amount;
        Debug.Log($"[PlayerStats] Used {amount} stamina. Remaining stamina: {currentStamina}");
        return true;
    }

    public void RecoverStamina(float amount)
    {
        if (currentStamina == maxStamina)
        {
            Debug.Log("[PlayerStats] Stamina is already full. No recovery needed.");
            return;
        }
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        //Debug.Log($"[PlayerStats] Recovered {amount} stamina. Current stamina: {currentStamina}");
    }

    public void Die()
    {
        Debug.Log($"[PlayerStats] {gameObject.name} has died. Returning to Main Menu...");
        Destroy(gameObject);
        SceneManager.LoadScene("MainMenu");
    }

    public void SetStats(int hp, int stamina)
    {
        currentHealth = Mathf.Clamp(hp, 0, (int)maxHealth);
        currentStamina = Mathf.Clamp(stamina, 0, (int)maxStamina);
        Debug.Log($"[PlayerStats] Stats restored → HP: {currentHealth}/{maxHealth}, Stamina: {currentStamina}/{maxStamina}");
    }
}
