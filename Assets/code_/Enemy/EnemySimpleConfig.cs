using UnityEngine;

[CreateAssetMenu(menuName = "Wukong/EnemySimpleConfig", fileName = "EnemySimpleConfig")]
public class EnemySimpleConfig : ScriptableObject
{
    [Header("Identity")]
    public string enemyId = "kim_giap_anh";
    public string displayName = "Kim Giap Anh";

    [Header("Core Stats")]
    [Min(1)] public int maxHP = 150;
    [Min(0)] public float armor = 4f;
    [Min(1)] public float baseDamage = 12f;

    [Header("Attack")]
    [Tooltip("Cooldown between 2 hits (requirement: 1.0s)")]
    [Min(0.05f)] public float attackCooldown = 1.0f;
}
