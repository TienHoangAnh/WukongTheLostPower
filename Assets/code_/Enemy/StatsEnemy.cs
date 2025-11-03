using UnityEngine;
using System;

[RequireComponent(typeof(OnDeathNotify))]
public class StatsEnemy : MonoBehaviour, IDamageable
{
    [Header("Config")]
    public EnemySimpleConfig config;

    [Header("Runtime")]
    public int currentHP;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    Animator anim;
    bool dead;

    void Awake()
    {
        anim = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
    }

    void Start()
    {
        if (!config) { Debug.LogError($"[{name}] Missing EnemySimpleConfig"); enabled = false; return; }
        currentHP = Mathf.Max(1, config.maxHP);
        RaiseHP();
    }

    // IDamageable (pipeline đơn)
    public void ApplyDamage(float amount) => TakeDamage(amount);
    public void TakeDamage(float amount)
    {
        if (dead) return;
        float mitigated = Mathf.Max(1f, amount - Mathf.Max(0f, config.armor));
        currentHP = Mathf.Max(0, currentHP - Mathf.RoundToInt(mitigated));
        anim?.SetTrigger("TakeDamage");
        RaiseHP();
        if (currentHP <= 0) Die();
    }
    public void TakeDamage(int amount) => TakeDamage((float)amount);

    public float GetAttackDamage(float coef = 1f) => config ? Mathf.Max(1f, config.baseDamage * coef) : 1f;
    public float GetAttackCooldown() => config ? config.attackCooldown : 1.0f;

    void Die()
    {
        if (dead) return; dead = true;
        anim?.SetTrigger("Die");
        GetComponent<OnDeathNotify>()?.NotifyDeath();
        OnDied?.Invoke();
        Destroy(gameObject, 0.1f);
    }

    void RaiseHP() => OnHealthChanged?.Invoke(currentHP, config ? config.maxHP : currentHP);
}
