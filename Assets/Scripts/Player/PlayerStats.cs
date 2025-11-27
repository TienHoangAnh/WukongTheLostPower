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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

                // Restore position/rotation if available
                if (SaveRuntime.Current.player.pos != null)
                {
                    try
                    {
                        var v = SaveRuntime.Current.player.pos.ToVector3();
                        transform.position = v;
                        transform.rotation = Quaternion.Euler(0f, SaveRuntime.Current.player.rotY, 0f);
                    }
                    catch { }
                }

                // If the loaded save has HP ==0, treat this as an accidental dead-state save and restore to full HP
                if (currentHealth <=0f)
                {
                    Debug.LogWarning("[PlayerStats] Loaded save contains HP=0 — restoring to maxHealth to avoid starting dead.");
                    currentHealth = maxHealth;
                    // update runtime save so subsequent continues don't re-load HP=0
                    UpdateSaveRuntime();
                    try
                    {
                        _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
                    }
                    catch { }
                }

                // If loaded stamina is zero or negative, restore to max stamina as well so player can act immediately
                if (currentStamina <=0f)
                {
                    Debug.LogWarning("[PlayerStats] Loaded save contains Stamina=0 — restoring to maxStamina to avoid starting exhausted.");
                    currentStamina = maxStamina;
                    // update runtime save so subsequent continues don't re-load stamina=0
                    UpdateSaveRuntime();
                    try
                    {
                        _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
                    }
                    catch { }
                }
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reposition persistent player to scene anchor if present
        // Prefer PlayerAnchor.Current (scene placed), otherwise find GameObject named "TransitionAnchor"
        if (PlayerAnchor.Current != null)
        {
            transform.position = PlayerAnchor.Current.position;
            transform.rotation = PlayerAnchor.Current.rotation;
        }
        else
        {
            var anchor = GameObject.Find("TransitionAnchor");
            if (anchor != null)
            {
                transform.position = anchor.transform.position;
                transform.rotation = anchor.transform.rotation;
            }
        }

        // After reposition, update runtime pos so UI/Save reflect correct location
        UpdateSaveRuntime();
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
        SaveRuntime.Current.player.pos = new Vector3DTO(transform.position);
        SaveRuntime.Current.player.rotY = transform.eulerAngles.y;
    }
}
