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
        healthSlider.maxValue = playerStats.maxHealth;
        manaSlider.maxValue = playerStats.maxMana;
    }

    void Update()
    {
        healthSlider.value = playerStats.currentHealth;
        manaSlider.value = playerStats.currentMana;
    }
}