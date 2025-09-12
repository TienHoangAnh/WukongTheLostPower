using UnityEditor.SceneManagement;
using UnityEngine;

public class EnemyStats : MonoBehaviour, ICharacter
{

    public GameObject[] dropItems; // Gán prefab item rớt trong Inspector   
    public float maxHealth = 100f;
    private float currentHealth;

    void Start() => currentHealth = maxHealth;

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " trúng đòn: " + amount + ". Máu còn lại: " + currentHealth);
        if (currentHealth <= 0) 
            Die();
    }

    public void Heal(float amount) { }

    public void Die()
    {
        Debug.Log(gameObject.name + " chết!");
        dropItem();
        Destroy(gameObject);
    }

    public void dropItem()
    { 
        foreach (var item in dropItems)
        {
            Instantiate(item, transform.position, Quaternion.identity);
        }
    }
}
