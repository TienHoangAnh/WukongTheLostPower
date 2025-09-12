public interface IBossState
{
    void EnterState(BossContext context);
    void UpdateState(BossContext context);
    void ExitState(BossContext context);
}
//BossIdleState: đứng yên, chờ phát hiện player.

//BossChaseState: đuổi theo player.

//BossAttackState: chuyển sang đánh (gọi PerformAttack() theo Strategy).