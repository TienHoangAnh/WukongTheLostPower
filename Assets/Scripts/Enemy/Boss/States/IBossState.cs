public interface IBossState
{
    void EnterState(BossContext context);
    void UpdateState(BossContext context);
    void ExitState(BossContext context);
}