using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour, ICharacter, IDamageable
{
    public static PlayerStats Instance { get; private set; }

    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxStamina = 100f;
    public float currentStamina;
    public float baseDamage = 20f;
    public float armor = 5f;

    private PlayerMovementContext _moveCtx;
    private bool _initialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Initialize current values only once (prevent reset when scene reloads)
        if (!_initialized)
        {
            // If there is saved runtime player state, prefer that
            if (SaveRuntime.Current != null && SaveRuntime.Current.player != null
                && (SaveRuntime.Current.playTimeSeconds > 0f || SaveRuntime.Current.player.hp > 0 || SaveRuntime.Current.player.stamina > 0))
            {
                currentHealth = Mathf.Clamp(SaveRuntime.Current.player.hp, 0, (int)maxHealth);
                currentStamina = Mathf.Clamp(SaveRuntime.Current.player.stamina, 0, (int)maxStamina);
            }
            else
            {
                if (currentHealth <= 0f)
                    currentHealth = maxHealth;
                if (currentStamina <= 0f)
                    currentStamina = maxStamina;
            }

            _initialized = true;

            // Ensure runtime reflects initial values
            UpdateSaveRuntime();
        }

        _moveCtx = GetComponent<PlayerMovementContext>()
                   ?? FindFirstObjectByType<PlayerMovementContext>();
    }

    public void TakeDamage(float amount)
    {
        float finalDamage = Mathf.Max(1f, amount - armor);
        currentHealth -= finalDamage;

        Debug.Log($"[PlayerStats] Player took {amount} damage (armor {armor}) → final {finalDamage}. Current HP: {currentHealth}");

        _moveCtx?.TakeDamage();

        UpdateSaveRuntime();

        if (currentHealth <= 0) Die();
    }

    public void TakeDamage(int amount) => TakeDamage((float)amount);

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"[PlayerStats] Player healed {amount} HP. Current HP: {currentHealth}");
        UpdateSaveRuntime();
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
        UpdateSaveRuntime();
        return true;
    }

    public void RecoverStamina(float amount)
    {
        if (currentStamina == maxStamina)
        {
            //Debug.Log("[PlayerStats] Stamina is already full. No recovery needed.");
            return;
        }
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        UpdateSaveRuntime();
    }

    public void Die()
    {
        Debug.Log($"[PlayerStats] {gameObject.name} has died. Returning to Main Menu...");

        // Increment death count in save runtime and persist
        try
        {
            if (SaveRuntime.Current == null) SaveRuntime.Current = new SaveSlotDTO();
            SaveRuntime.Current.deathCount = (SaveRuntime.Current.deathCount) +1;
            // Fire-and-forget save
            _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[PlayerStats] Failed to update/save death count: {ex.Message}");
        }

        Destroy(gameObject);
        SceneManager.LoadScene("MainMenu");
    }

    public void SetStats(int hp, int stamina)
    {
        currentHealth = Mathf.Clamp(hp, 0, (int)maxHealth);
        currentStamina = Mathf.Clamp(stamina, 0, (int)maxStamina);
        Debug.Log($"[PlayerStats] Stats restored → HP: {currentHealth}/{maxHealth}, Stamina: {currentStamina}/{maxStamina}");
        UpdateSaveRuntime();
    }

    private void UpdateSaveRuntime()
    {
        if (SaveRuntime.Current == null) return;
        if (SaveRuntime.Current.player == null) SaveRuntime.Current.player = new PlayerStateDTO();

        SaveRuntime.Current.player.hp = Mathf.RoundToInt(currentHealth);
        SaveRuntime.Current.player.stamina = Mathf.RoundToInt(currentStamina);
    }
}
