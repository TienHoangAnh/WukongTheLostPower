using UnityEngine;

[CreateAssetMenu(menuName = "Wukong/EnemySimpleConfig", fileName = "EnemySimpleConfig")]
public class EnemySimpleConfig : ScriptableObject
{
    [Header("Identity")]
    public string enemyId = "giap_anh";
    public string displayName = "Giáp Ảnh";

    [Header("Core Stats")]
    [Min(1)] public int maxHP = 150;
    [Min(0)] public float armor = 4f;    // giáp phẳng (trừ thẳng)
    [Min(1)] public float baseDamage = 12f;   // damage mỗi đòn trúng

    [Header("Attack")]
    [Tooltip("Hồi chiêu giữa 2 đòn (yêu cầu: 1.0s)")]
    [Min(0.05f)] public float attackCooldown = 1.0f;
}
