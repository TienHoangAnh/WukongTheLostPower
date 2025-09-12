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
    public float maxMana; // Th�m mana cho boss
    public float currentHealth; // Th�m currentHealth ?? qu?n l� m�u hi?n t?i c?a boss
    public float currentMana; // Th�m currentMana ?? qu?n l� mana hi?n t?i c?a boss
}
