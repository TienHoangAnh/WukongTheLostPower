using UnityEngine;

[CreateAssetMenu(fileName = "BossStats", menuName = "Boss/BossStats")]
public class BossStats : ScriptableObject
{
    public float maxHealth;
    public float currentHealth;
    public float damage;
    public float moveSpeed;
    public float attackRange;
    public float detectionRange;
    public float attackCooldown;
}
