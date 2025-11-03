using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public Slider healthSlider;
    public Slider manaSlider;
    private PlayerStats playerStats;

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("[PlayerUIManager] ❌ Không tìm thấy PlayerStats trong scene!");
            enabled = false;
            return;
        }

        if (healthSlider == null || manaSlider == null)
        {
            Debug.LogError("[PlayerUIManager] ❌ Chưa gán Slider trong Inspector!");
            enabled = false;
            return;
        }

        healthSlider.maxValue = playerStats.maxHealth;
        manaSlider.maxValue = playerStats.maxMana;
    }

    void Update()
    {
        if (playerStats == null || healthSlider == null || manaSlider == null)
            return;

        healthSlider.value = playerStats.currentHealth;
        manaSlider.value = playerStats.currentMana;
    }
}
