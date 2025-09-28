using UnityEngine;

[CreateAssetMenu(fileName = "BossStats", menuName = "Boss/BossStats")]
public class BossStats : ScriptableObject
{
    public float maxHealth;
    public float damage;
    public float moveSpeed;
    public float attackRange;
    public float detectionRange;
    public float attackCooldown;
    public float maxMana; 
    public float currentHealth; 
    public float currentMana; 
}
