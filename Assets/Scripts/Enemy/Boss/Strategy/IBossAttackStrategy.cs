public interface IBossAttackStrategy
{
    void Configure(BossContext context);    // Gán lại thông số
    void Attack(BossContext context);       // Gọi khi boss tấn công
}

//MeleeBossAttackStrategy.cs: tấn công gần.

//RangedBossAttackStrategy.cs: bắn projectile.

//HybridBossAttackStrategy.cs: dùng cả 2.