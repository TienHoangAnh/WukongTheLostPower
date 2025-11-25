using UnityEngine;

public class EnemyStats : MonoBehaviour, ICharacter, IDamageable
{
    [Header("Drops")]
    [Tooltip("Items dropped when enemies die (prefab)")]
    public GameObject[] dropItems;

    [Header("Stats")]
    public float maxHealth = 1000f;
    public float armor = 10f;

    private float currentHealth;
    private Animator animator;
    private bool isDead = false;

    [Header("Config (Optional)")]
    public EnemySimpleConfig config;

    // --------------------------------------------------------------
    // 🧩 INIT
    // --------------------------------------------------------------
    private void Start()
    {
        if (config != null)
        {
            maxHealth = config.maxHP;
            armor = config.armor;
        }

        currentHealth = maxHealth;
        animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
    }

    // --------------------------------------------------------------
    // ⚔️ COMBAT HELPERS
    // --------------------------------------------------------------
    public float GetAttackDamage(float coef = 1f)
    {
        if (config != null)
            return Mathf.Max(1f, config.baseDamage * Mathf.Max(0.05f, coef));

        return 5f;
    }

    public float GetAttackCooldown()
    {
        if (config != null)
            return Mathf.Max(0.05f, config.attackCooldown);

        return 1.0f;
    }

    // --------------------------------------------------------------
    // 💥 DAMAGE HANDLING
    // --------------------------------------------------------------
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(amount - armor, 1f);
        currentHealth -= finalDamage;

        Debug.Log($"{gameObject.name} hit: {amount} (armor {armor}) => {finalDamage}. Remaining Health: {currentHealth}");
        if (animator != null)
            animator.SetTrigger("TakeDamage");

        if (currentHealth <= 0)
            Die();
    }

    public void TakeDamage(int amount) => TakeDamage((float)amount);
    public void Heal(float amount) { }

    // --------------------------------------------------------------
    // ☠️ DEATH
    // --------------------------------------------------------------
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} die!");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        gameObject.tag = "Untagged";

        DropItems();

        var idComp = GetComponent<EnemyId>();
        string eid = idComp != null ? idComp.id : null;

        if (!string.IsNullOrEmpty(eid) && SaveRuntime.Current != null)
        {
            if (SaveRuntime.Current.deadEnemies == null)
                SaveRuntime.Current.deadEnemies = new System.Collections.Generic.List<string>();

            if (!SaveRuntime.Current.deadEnemies.Contains(eid))
            {
                SaveRuntime.Current.deadEnemies.Add(eid);
                _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
            }
        }

        var ai = GetComponent<EnemyAIContext>();
        if (ai != null)
        {
            ai.OnDeathFromHealth();
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }

    private void DropItems()
    {
        if (dropItems == null || dropItems.Length == 0) return;

        var prefab = dropItems[0];
        if (prefab == null) return;

        Vector3 dropPos = transform.position + Vector3.up * 1f;
        var drop = Instantiate(prefab, dropPos, Quaternion.identity);

        drop.SetActive(true);

        var col = drop.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        var rend = drop.GetComponent<Renderer>();
        if (rend != null) rend.enabled = true;

        var rb = drop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.up * 2f;
        }

        Debug.Log($"[EnemyStats] Dropped item: {drop.name} tại {dropPos}");
    }
}
