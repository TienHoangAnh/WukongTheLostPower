using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour, ICharacter, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;

    public float maxMana = 100f;
    public float currentMana;

    public float maxStamina = 100f;
    public float currentStamina;

    public float baseDamage = 10f;

    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Player nhận sát thương: " + amount + ". Máu còn lại: " + currentHealth);
        if (currentHealth <= 0) Die();
    }

    public void TakeDamage(int amount)
    {
        TakeDamage((float)amount);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"💚 Player hồi {amount} máu. HP: {currentHealth}");
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina < amount) return false;
        currentStamina -= amount;
        Debug.Log($"Player sử dụng {amount} stamina. Stamina còn lại: {currentStamina}");
        return true;
    }

    public void RecoverStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " chết!");
        Destroy(gameObject);
        SceneManager.LoadScene("MainMenu");
    }

    public void UseMana(float amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Max(currentMana, 0);
        Debug.Log("Player sử dụng mana: " + amount + ". Mana còn lại: " + currentMana);
    }
}
