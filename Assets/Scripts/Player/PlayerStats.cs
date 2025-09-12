using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour, ICharacter
{
    public float maxHealth = 100f;
    public float currentHealth;

    public float maxMana = 100f;
    public float currentMana;

    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Player nhận sát thương: " + amount + ". Máu còn lại: " + currentHealth);
        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"💚 Player hồi {amount} máu. HP: {currentHealth}");
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " chết!");
        //Destroy(gameObject);
    }

    public void UseMana(float amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Max(currentMana, 0);
        Debug.Log("Player sử dụng mana: " + amount + ". Mana còn lại: " + currentMana);
    }
}
