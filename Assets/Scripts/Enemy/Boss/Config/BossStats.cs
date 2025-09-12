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
    public float maxMana; // Thêm mana cho boss
    public float currentHealth; // Thêm currentHealth ?? qu?n lý máu hi?n t?i c?a boss
    public float currentMana; // Thêm currentMana ?? qu?n lý mana hi?n t?i c?a boss
}
