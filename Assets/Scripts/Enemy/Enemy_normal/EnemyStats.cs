using UnityEngine;

public class EnemyStats : MonoBehaviour, ICharacter, IDamageable
{
    public GameObject[] dropItems; // Gán prefab item rớt trong Inspector
    public float maxHealth = 1000f;
    private float currentHealth;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            // Try to get from children if not on root
            animator = GetComponentInChildren<Animator>();
        }
    }

    // keep existing float-based API for compatibility
    public void TakeDamage(float amount)
    {
        int dmg = Mathf.RoundToInt(amount);
        TakeDamage(dmg);
    }

    // new int-based API
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} trúng đòn: {amount}. Máu còn lại: {currentHealth}");
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
            Debug.Log($"[EnemyStats] Trigger TakeDamage animation");
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount) { /* optional */ }

    public void Die()
    {
        Debug.Log($"{gameObject.name} chết!");

        // Prevent duplicate drops using SaveRuntime deadEnemies (if present) and EnemyId component
        var idComp = GetComponent<EnemyId>();
        string eid = idComp != null ? idComp.id : null;

        if (!string.IsNullOrEmpty(eid) && SaveRuntime.Current != null)
        {
            if (SaveRuntime.Current.deadEnemies == null) SaveRuntime.Current.deadEnemies = new System.Collections.Generic.List<string>();
            if (!SaveRuntime.Current.deadEnemies.Contains(eid))
            {
                DropItems();
                SaveRuntime.Current.deadEnemies.Add(eid);
                _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
            }
            else
            {
                Debug.Log($"[EnemyStats] Enemy {eid} already recorded as dropped - skip drop");
            }
        }
        else
        {
            // fallback drop if no id or no save runtime
            DropItems();
        }

        // Notify AI to play death animation / stop movement and schedule destroy
        var ai = GetComponent<EnemyAIContext>();
        if (ai != null)
        {
            ai.OnDeathFromHealth();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void DropItems()
    {
        foreach (var item in dropItems)
        {
            Instantiate(item, transform.position, Quaternion.identity);
        }
    }
}
