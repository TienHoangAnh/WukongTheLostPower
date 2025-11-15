using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// PlayerUIManager updates player HUD: health/stamina bars and quick item slots (HP/MP items).
/// - Manages quick-slot counts directly (no global inventory).
/// - Call `PlayerUIManager.I.AddItem(id,count)` when picking up an item.
/// - Call `UseHpItem()` / `UseMpItem()` to consume an item and restore player stats.
/// </summary>
public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager I { get; private set; }

    public Slider healthSlider;
    public Slider manaSlider;
    public Slider staminaSider;
    private PlayerStats playerStats;

    [Header("Item Slots")] 
    [Tooltip("Item id to show on the HP slot (e.g. 'holy_water')")]
    [SerializeField] private string hpItemId = "holy_water";
    [Tooltip("Image component to display HP item icon (child Icon). Do NOT assign the frame)" )]
    [SerializeField] private Image hpItemImage;
    [Tooltip("Text component to display HP item count (use UnityEngine.UI.Text)")]
    [SerializeField] private TMPro.TMP_Text hpItemCountText;

    [Tooltip("Item id to show on the MP slot (e.g. 'elixir')")]
    [SerializeField] private string mpItemId = "elixir";
    [Tooltip("Image component to display MP item icon (child Icon). Do NOT assign the frame)" )]
    [SerializeField] private Image mpItemImage;
    [Tooltip("Text component to display MP item count (use UnityEngine.UI.Text)")]
    [SerializeField] private TMPro.TMP_Text mpItemCountText;

    // Internal counts for quick-slots
    private int hpCount =0;
    private int mpCount =0;

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);

            // Ensure this canvas renders above scene UI by default
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                // use a large sorting order so persistent UI is on top
                canvas.sortingOrder = Mathf.Max(canvas.sortingOrder,100);
            }
        }
        else if (I != this)
        {
            // If another persistent instance already exists, destroy this scene-local one
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Remove any scene-created PlayerUIManager instances to ensure the persistent one stays
        var others = FindObjectsOfType<PlayerUIManager>();
        foreach (var o in others)
        {
            if (o == this) continue;
            // destroy the root gameobject of the duplicate UI
            Destroy(o.gameObject);
        }

        // Rebind PlayerStats when a new scene loads (player object may be recreated per scene)
        playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogWarning("[PlayerUIManager] PlayerStats not found after scene load.");
            return;
        }

        // If this persistent prefab lost slider references (because prefab in scene had them), try to find sliders in scene UI and keep them
        if ((healthSlider == null || staminaSider == null || manaSlider == null))
        {
            var sceneCanvases = FindObjectsOfType<Canvas>(true);
            foreach (var c in sceneCanvases)
            {
                // try to find typical named children
                var hs = c.transform.Find("HUD/HealthSlider")?.GetComponent<Slider>();
                var ss = c.transform.Find("HUD/StaminaSlider")?.GetComponent<Slider>();
                var ms = c.transform.Find("HUD/ManaSlider")?.GetComponent<Slider>();
                if (hs != null) healthSlider = hs;
                if (ss != null) staminaSider = ss;
                if (ms != null) manaSlider = ms;
            }
        }

        // Ensure sliders reflect the new player's values
        if (healthSlider != null)
        {
            healthSlider.maxValue = playerStats.maxHealth;
            healthSlider.value = playerStats.currentHealth;
        }
        if (staminaSider != null)
        {
            staminaSider.maxValue = playerStats.maxStamina;
            staminaSider.value = playerStats.currentStamina;
        }

        // Refresh visuals/icons in case scene has new inventory database or assets
        RefreshIconsFromDatabase();
        UpdateItemSlots();
    }

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("[PlayerUIManager] PlayerStats not found in Start; will attempt to rebind after scene load.");
            // Do not disable here; OnSceneLoaded will try to find PlayerStats when a scene is (re)loaded.
        }

        if (healthSlider == null || manaSlider == null || staminaSider == null)
        {
            Debug.LogError("[PlayerUIManager] ❌ Chưa gán Slider trong Inspector! UI will remain active but sliders must be assigned in prefab.");
            // Do NOT disable component: keep alive so persistent UI can rebind when scene loads.
        }

        if (playerStats != null)
        {
            if (healthSlider != null) healthSlider.maxValue = playerStats.maxHealth;
            if (staminaSider != null) staminaSider.maxValue = playerStats.maxStamina;

            if (healthSlider != null) healthSlider.value = playerStats.currentHealth;
            if (staminaSider != null) staminaSider.value = playerStats.currentStamina;
        }

        // Initial update of slot icons/counts
        RefreshIconsFromDatabase();
        UpdateItemSlots();
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    void Update()
    {
        if (playerStats == null || healthSlider == null || manaSlider == null || staminaSider == null)
            return;

        healthSlider.value = playerStats.currentHealth;
        staminaSider.value = playerStats.currentStamina;
    }

    // Read icon sprites from InventoryDatabase (if present)
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

    /// <summary>
    /// Called by pickup logic when player picks up an item.
    /// Increments quick-slot count if the picked id matches hp/mp ids.
    /// </summary>
    public void AddItem(string id, int count =1)
    {
        if (string.IsNullOrEmpty(id) || count <=0) return;

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

        // Unknown item id: ignore or log
        Debug.Log($"[PlayerUIManager] Picked unknown item id: {id}");
    }

    /// <summary>
    /// Use one HP item: heal player based on InventoryItem.hpRestore or default value.
    /// Returns true if used.
    /// </summary>
    public bool UseHpItem()
    {
        if (hpCount <=0 || playerStats == null) return false;

        float heal =50f; // default
        if (InventoryDatabase.I != null)
        {
            var asset = InventoryDatabase.I.GetById(hpItemId);
            if (asset != null) heal = asset.hpRestore >0 ? asset.hpRestore : heal;
        }

        playerStats.Heal(heal);
        hpCount = Mathf.Max(0, hpCount -1);
        UpdateItemSlots();
        UI_Toasts.Show($"Used HP item. Healed {heal}");
        return true;
    }

    /// <summary>
    /// Use one MP (stamina) item: recover stamina based on InventoryItem.staminaRestore or default.
    /// </summary>
    public bool UseMpItem()
    {
        if (mpCount <=0 || playerStats == null) return false;

        float recover =50f; // default
        if (InventoryDatabase.I != null)
        {
            var asset = InventoryDatabase.I.GetById(mpItemId);
            if (asset != null) recover = asset.staminaRestore >0 ? asset.staminaRestore : recover;
        }

        playerStats.RecoverStamina(recover);
        mpCount = Mathf.Max(0, mpCount -1);
        UpdateItemSlots();
        UI_Toasts.Show($"Used MP item. Recovered {recover}");
        return true;
    }

    /// <summary>
    /// Update UI visuals for quick slots based on internal counts.
    /// Only toggles icon/count visibility (frame remains under UI designer control).
    /// </summary>
    private void UpdateItemSlots()
    {
        if (hpItemImage != null && hpItemCountText != null)
        {
            hpItemImage.gameObject.SetActive(hpCount >0);
            hpItemCountText.gameObject.SetActive(hpCount >0);
            hpItemCountText.text = hpCount >0 ? hpCount.ToString() : "";
        }

        if (mpItemImage != null && mpItemCountText != null)
        {
            mpItemImage.gameObject.SetActive(mpCount >0);
            mpItemCountText.gameObject.SetActive(mpCount >0);
            mpItemCountText.text = mpCount >0 ? mpCount.ToString() : "";
        }
    }

    // Optional getters
    public int GetHpCount() => hpCount;
    public int GetMpCount() => mpCount;
}
