using UnityEngine;

public class EnemyStats : MonoBehaviour, ICharacter, IDamageable
{
    [Header("Drops")]
    [Tooltip("Các vật phẩm rơi ra khi enemy chết (prefab)")]
    public GameObject[] dropItems;

    [Header("Stats")]
    public float maxHealth = 1000f;
    public float armor = 10f;

    private float currentHealth;
    private Animator animator;
    private bool isDead = false; // ✅ Ngăn drop hoặc Die() nhiều lần

    [Header("Config (tùy chọn)")]
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
        if (isDead) return; // ✅ Bỏ qua nếu đã chết

        float finalDamage = Mathf.Max(amount - armor, 1f);
        currentHealth -= finalDamage;

        Debug.Log($"{gameObject.name} trúng đòn: {amount} (giáp {armor}) => {finalDamage}. Máu còn lại: {currentHealth}");

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

        Debug.Log($"{gameObject.name} chết!");

        // Ngăn enemy bị đánh tiếp
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        gameObject.tag = "Untagged";

        // Drop vật phẩm 1 lần
        DropItems();

        // Ghi lại trạng thái vào save
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

        // Gọi logic AI chết
        var ai = GetComponent<EnemyAIContext>();
        if (ai != null)
        {
            ai.OnDeathFromHealth();
        }
        else
        {
            Destroy(gameObject, 2f); // ⏳ chờ anim death kết thúc
        }
    }

    private void DropItems()
    {
        if (dropItems == null || dropItems.Length == 0) return;

        foreach (var item in dropItems)
        {
            if (item == null) continue;

            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 1.0f, Random.Range(-0.5f, 0.5f));
            var drop = Instantiate(item, transform.position + offset, Quaternion.identity);

            // ép hiển thị
            drop.SetActive(true);
            drop.transform.localScale = Vector3.one;                 // ✅ chống scale 0
            var r = drop.GetComponentInChildren<Renderer>(true);
            if (r != null) r.enabled = true;

            var rb = drop.GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = false; rb.useGravity = true; rb.linearVelocity = Vector3.up * 2f; }
        }
    }
}
