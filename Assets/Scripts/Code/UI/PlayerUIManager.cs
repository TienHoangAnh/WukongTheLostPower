using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// PlayerUIManager updates player HUD: health/stamina bars and quick item slots (HP/MP items).
/// - Manages quick-slot counts directly (no global inventory).
/// - Call `PlayerUIManager.I.AddItem(id,count)` when picking up an item.
/// - Call `UseHpItem()` / `UseMpItem()` to consume an item and restore player stats.
///
/// NOTE: This component is intended to be scene-local and attached to a HUD Canvas
/// (e.g. `HUD_Canvas` in each scene). The Player GameObject is persistent via
/// DontDestroyOnLoad; the HUD should be recreated per scene and will rebind to the
/// persistent PlayerStats when the scene loads.
/// </summary>
public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager I { get; set; }

    public Slider healthSlider;
    public Slider staminaSider;

    private PlayerStats playerStats;
    private PlayerCombat playerCombat; // query skill cooldowns

    [Header("Item Slots")]
    [Tooltip("Item id to show on the HP slot (e.g. 'holy_water')")]
    [SerializeField] private string hpItemId = "holy_water";

    [Tooltip("Image component to display HP item icon (child Icon). Do NOT assign the frame)")]
    [SerializeField] private Image hpItemImage;

    [Tooltip("Text component to display HP item count (TMP)")]
    [SerializeField] private TMP_Text hpItemCountText;

    [Tooltip("Item id to show on the MP slot (e.g. 'elixir')")]
    [SerializeField] private string mpItemId = "elixir";

    [Tooltip("Image component to display MP item icon (child Icon). Do NOT assign the frame)")]
    [SerializeField] private Image mpItemImage;

    [Tooltip("Text component to display MP item count (TMP)")]
    [SerializeField] private TMP_Text mpItemCountText;

    [Header("Skill Cooldown Texts (Q/E/R/J)")]
    [SerializeField] private TMP_Text qCooldownText;
    [SerializeField] private TMP_Text eCooldownText;
    [SerializeField] private TMP_Text rCooldownText;
    [SerializeField] private TMP_Text jCooldownText;

    private int hpCount = 0;
    private int mpCount = 0;

    // -------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------
    void Awake()
    {
        // Warn if mistakenly attached to the Player GameObject
        if (gameObject.CompareTag("Player"))
        {
            Debug.LogWarning(
                "PlayerUIManager is attached to a GameObject with tag 'Player'.\n" +
                "This component should be attached to a scene-local HUD Canvas."
            );
        }

        if (I == null)
        {
            I = this;

            // No DontDestroyOnLoad: HUD is scene-local
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 100);
            }
        }
        else if (I != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RebindPlayer();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        RebindPlayer();
    }

    // -------------------------------------------------------------
    // Rebinding logic
    // -------------------------------------------------------------
    private void RebindPlayer()
    {
        // Try common ways to find PlayerStats
        var ps = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();

        if (ps == null)
        {
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
                ps = playerGo.GetComponent<PlayerStats>() ?? playerGo.GetComponentInChildren<PlayerStats>();
        }

        if (ps != null)
        {
            playerStats = ps;
            playerCombat = ps.GetComponent<PlayerCombat>() ?? ps.GetComponentInChildren<PlayerCombat>();

            // Bind sliders
            if (healthSlider != null)
            {
                healthSlider.maxValue = ps.maxHealth;
                healthSlider.value = ps.currentHealth;
            }
            if (staminaSider != null)
            {
                staminaSider.maxValue = ps.maxStamina;
                staminaSider.value = ps.currentStamina;
            }

            UpdateHpUi(ps.currentHealth, ps.maxHealth);
            UpdateStaminaUi(ps.currentStamina, ps.maxStamina);

            Debug.Log($"[PlayerUI] Bound to PlayerStats (HP={ps.currentHealth}, Stamina={ps.currentStamina})");
        }
        else
        {
            Debug.LogWarning("[PlayerUI] PlayerStats not found. Will retry...");
            StartCoroutine(CoRetryBindPlayer(5, 0.2f));
        }

        RefreshIconsFromDatabase();

        // Load item counts from save
        if (GameSaveController.I != null)
        {
            hpCount = GameSaveController.I.GetCollectedCount(hpItemId);
            mpCount = GameSaveController.I.GetCollectedCount(mpItemId);
            UpdateItemSlots();
        }
    }

    private System.Collections.IEnumerator CoRetryBindPlayer(int attempts, float delay)
    {
        for (int i = 0; i < attempts; i++)
        {
            yield return new WaitForSeconds(delay);

            var ps = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
            if (ps == null)
            {
                var playerGo = GameObject.FindWithTag("Player");
                if (playerGo != null)
                    ps = playerGo.GetComponent<PlayerStats>() ?? playerGo.GetComponentInChildren<PlayerStats>();
            }

            if (ps != null)
            {
                playerStats = ps;
                playerCombat = ps.GetComponent<PlayerCombat>() ?? ps.GetComponentInChildren<PlayerCombat>();

                if (healthSlider != null)
                {
                    healthSlider.maxValue = ps.maxHealth;
                    healthSlider.value = ps.currentHealth;
                }
                if (staminaSider != null)
                {
                    staminaSider.maxValue = ps.maxStamina;
                    staminaSider.value = ps.currentStamina;
                }

                UpdateHpUi(ps.currentHealth, ps.maxHealth);
                UpdateStaminaUi(ps.currentStamina, ps.maxStamina);

                Debug.Log($"[PlayerUI] Successfully rebound to PlayerStats after retry.");
                yield break;
            }
        }

        Debug.LogWarning("[PlayerUI] Failed to bind PlayerStats after retries.");
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    // -------------------------------------------------------------
    // Update loop
    // -------------------------------------------------------------
    void Update()
    {
        if (playerStats != null)
        {
            if (healthSlider != null)
                healthSlider.value = playerStats.currentHealth;

            if (staminaSider != null)
                staminaSider.value = playerStats.currentStamina;
        }

        UpdateSkillCooldowns();
    }

    // -------------------------------------------------------------
    // Icon refresh
    // -------------------------------------------------------------
    private void RefreshIconsFromDatabase()
    {
        if (InventoryDatabase.I != null)
        {
            var hpAsset = InventoryDatabase.I.GetById(hpItemId);
            if (hpAsset != null && hpAsset.icon != null && hpItemImage != null)
            {
                hpItemImage.sprite = hpAsset.icon;
                hpItemImage.SetNativeSize();
            }

            var mpAsset = InventoryDatabase.I.GetById(mpItemId);
            if (mpAsset != null && mpAsset.icon != null && mpItemImage != null)
            {
                mpItemImage.sprite = mpAsset.icon;
                mpItemImage.SetNativeSize();
            }
        }
    }

    // -------------------------------------------------------------
    // Add / Use Items
    // -------------------------------------------------------------
    public void AddItem(string id, int count = 1)
    {
        if (string.IsNullOrEmpty(id) || count <= 0) return;

        if (id == hpItemId)
        {
            hpCount += count;
            UpdateItemSlots();
            UI_Toasts.Show($"+{count} HP item");
            return;
        }

        if (id == mpItemId)
        {
            mpCount += count;
            UpdateItemSlots();
            UI_Toasts.Show($"+{count} MP item");
            return;
        }

        Debug.Log($"[PlayerUIManager] Picked unknown item id: {id}");
    }

    public bool UseHpItem()
    {
        Debug.Log($"[PlayerUI] UseHpItem called. hpCount={hpCount}");

        if (playerStats == null)
            return false;

        // Prefer using save-system counts
        if (GameSaveController.I != null)
        {
            var available = GameSaveController.I.GetCollectedCount(hpItemId);
            if (available <= 0)
                return false;

            float heal = 50f;
            if (InventoryDatabase.I != null)
            {
                var asset = InventoryDatabase.I.GetById(hpItemId);
                if (asset != null)
                    heal = asset.hpRestore > 0 ? asset.hpRestore : heal;
            }

            playerStats.Heal(heal);
            GameSaveController.I.UseItem(hpItemId, 1);
            hpCount = GameSaveController.I.GetCollectedCount(hpItemId);

            UpdateItemSlots();
            UI_Toasts.Show($"Used HP item. Healed {heal}");
            return true;
        }

        // Fallback to local count
        if (hpCount <= 0)
            return false;

        float fallbackHeal = 50f;
        if (InventoryDatabase.I != null)
        {
            var asset = InventoryDatabase.I.GetById(hpItemId);
            if (asset != null)
                fallbackHeal = asset.hpRestore > 0 ? asset.hpRestore : fallbackHeal;
        }

        playerStats.Heal(fallbackHeal);
        hpCount = Mathf.Max(0, hpCount - 1);

        UpdateItemSlots();
        UI_Toasts.Show($"Used HP item. Healed {fallbackHeal}");
        return true;
    }

    public bool UseMpItem()
    {
        Debug.Log($"[PlayerUI] UseMpItem called. mpCount={mpCount}");

        if (playerStats == null)
            return false;

        if (GameSaveController.I != null)
        {
            var available = GameSaveController.I.GetCollectedCount(mpItemId);
            if (available <= 0)
                return false;

            float recover = 50f;
            if (InventoryDatabase.I != null)
            {
                var asset = InventoryDatabase.I.GetById(mpItemId);
                if (asset != null)
                    recover = asset.staminaRestore > 0 ? asset.staminaRestore : recover;
            }

            playerStats.RecoverStamina(recover);

            GameSaveController.I.UseItem(mpItemId, 1);
            mpCount = GameSaveController.I.GetCollectedCount(mpItemId);

            UpdateItemSlots();
            UI_Toasts.Show($"Used MP item. Recovered {recover}");
            return true;
        }

        if (mpCount <= 0)
            return false;

        float fallbackRecover = 50f;
        if (InventoryDatabase.I != null)
        {
            var asset = InventoryDatabase.I.GetById(mpItemId);
            if (asset != null)
                fallbackRecover = asset.staminaRestore > 0 ? asset.staminaRestore : fallbackRecover;
        }

        playerStats.RecoverStamina(fallbackRecover);
        mpCount = Mathf.Max(0, mpCount - 1);

        UpdateItemSlots();
        UI_Toasts.Show($"Used MP item. Recovered {fallbackRecover}");
        return true;
    }

    public void OnClick_UseHpItem() => UseHpItem();
    public void OnClick_UseMpItem() => UseMpItem();

    // -------------------------------------------------------------
    // UI update
    // -------------------------------------------------------------
    private void UpdateItemSlots()
    {
        // HP
        if (hpItemImage != null && hpItemCountText != null)
        {
            bool active = hpCount > 0;
            hpItemImage.gameObject.SetActive(active);
            hpItemCountText.gameObject.SetActive(active);
            hpItemCountText.text = active ? hpCount.ToString() : "";
        }

        // MP
        if (mpItemImage != null && mpItemCountText != null)
        {
            bool active = mpCount > 0;
            mpItemImage.gameObject.SetActive(active);
            mpItemCountText.gameObject.SetActive(active);
            mpItemCountText.text = active ? mpCount.ToString() : "";
        }
    }

    private void UpdateHpUi(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }

    private void UpdateStaminaUi(float current, float max)
    {
        if (staminaSider != null)
        {
            staminaSider.maxValue = max;
            staminaSider.value = current;
        }
    }

    // -------------------------------------------------------------
    // Skill cooldown UI
    // -------------------------------------------------------------
    private void UpdateSkillCooldowns()
    {
        if (playerCombat == null)
            return;

        UpdateCooldownText(qCooldownText, playerCombat.GetComboCooldownRemaining(0));
        UpdateCooldownText(eCooldownText, playerCombat.GetComboCooldownRemaining(1));
        UpdateCooldownText(rCooldownText, playerCombat.GetComboCooldownRemaining(2));
        UpdateCooldownText(jCooldownText, playerCombat.GetRangedCooldownRemaining());
    }

    private void UpdateCooldownText(TMP_Text field, float remaining)
    {
        if (field == null) return;

        if (remaining > 0f)
        {
            field.gameObject.SetActive(true);
            field.text = remaining.ToString("F1") + "s";
        }
        else
        {
            field.gameObject.SetActive(false);
        }
    }

    // -------------------------------------------------------------
    // Public getters
    // -------------------------------------------------------------
    public int GetHpCount() => hpCount;
    public int GetMpCount() => mpCount;
}
