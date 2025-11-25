public interface IBossAttackStrategy
{
    void Configure(BossContext context);
    void Attack(BossContext context);
}