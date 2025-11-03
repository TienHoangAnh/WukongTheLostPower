//using UnityEngine;

///// <summary>
///// Đóng gói thông tin về một lần gây damage, dùng cho hệ thống combat.
///// </summary>
//public struct DamageContext
//{
//    public float Amount;
//    public DamageType Type;
//    public bool IsCritical;
//    public Vector3 HitPoint;
//    public Object Source; // ví dụ: EnemyAIContext hoặc PlayerSkillManager

//    public DamageContext(float amount, DamageType type = DamageType.Physical, bool crit = false, Vector3 point = default, Object src = null)
//    {
//        Amount = amount;
//        Type = type;
//        IsCritical = crit;
//        HitPoint = point;
//        Source = src;
//    }
//}

///// <summary>
///// Phân loại damage nếu sau này bạn muốn mở rộng.
///// </summary>
//public enum DamageType
//{
//    Physical,
//    Magic,
//    Fire,
//    Ice,
//    True
//}
