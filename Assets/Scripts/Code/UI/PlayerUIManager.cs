using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider staminaSider;
    private PlayerStats playerStats;

    [Header("Item Slots")] 
    [Tooltip("Item id to show on the HP slot (e.g. 'holy_water')")]
    [SerializeField] private string hpItemId = "holy_water";
    [Tooltip("Image component to display HP item icon")]
    [SerializeField] private Image hpItemImage;
    [Tooltip("Text component to display HP item count")]
    [SerializeField] private Text hpItemCountText;

    [Tooltip("Item id to show on the MP slot (e.g. 'elixir')")]
    [SerializeField] private string mpItemId = "elixir";
    [Tooltip("Image component to display MP item icon")]
    [SerializeField] private Image mpItemImage;
    [Tooltip("Text component to display MP item count")]
    [SerializeField] private Text mpItemCountText;

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("[PlayerUIManager] ❌ Không tìm thấy PlayerStats trong scene!");
            enabled = false;
            return;
        }

        if (healthSlider == null || manaSlider == null || staminaSider == null)
        {
            Debug.LogError("[PlayerUIManager] ❌ Chưa gán Slider trong Inspector!");
            enabled = false;
            return;
        }

        healthSlider.maxValue = playerStats.maxHealth;
        staminaSider.maxValue = playerStats.maxStamina;

        // Subscribe to inventory updates if available
        if (InventoryManager.I != null)
            InventoryManager.I.OnInventoryChanged += UpdateItemSlots;

        // Initial update
        UpdateItemSlots();
    }

    void OnDestroy()
    {
        if (InventoryManager.I != null)
            InventoryManager.I.OnInventoryChanged -= UpdateItemSlots;
    }

    void Update()
    {
        if (playerStats == null || healthSlider == null || manaSlider == null || staminaSider == null)
            return;

        healthSlider.value = playerStats.currentHealth;
        staminaSider.value = playerStats.currentStamina;
    }

    private void UpdateItemSlots()
    {
        // HP slot
        if (hpItemImage != null && hpItemCountText != null)
        {
            int count = InventoryManager.I != null ? InventoryManager.I.GetCount(hpItemId) :0;
            var asset = InventoryDatabase.I != null ? InventoryDatabase.I.GetById(hpItemId) : null;
            if (asset != null && asset.icon != null)
                hpItemImage.sprite = asset.icon;

            hpItemImage.gameObject.SetActive(count >0);
            hpItemCountText.gameObject.SetActive(count >0);
            hpItemCountText.text = count >0 ? count.ToString() : "";
        }

        // MP slot
        if (mpItemImage != null && mpItemCountText != null)
        {
            int count = InventoryManager.I != null ? InventoryManager.I.GetCount(mpItemId) :0;
            var asset = InventoryDatabase.I != null ? InventoryDatabase.I.GetById(mpItemId) : null;
            if (asset != null && asset.icon != null)
                mpItemImage.sprite = asset.icon;

            mpItemImage.gameObject.SetActive(count >0);
            mpItemCountText.gameObject.SetActive(count >0);
            mpItemCountText.text = count >0 ? count.ToString() : "";
        }
    }
}
