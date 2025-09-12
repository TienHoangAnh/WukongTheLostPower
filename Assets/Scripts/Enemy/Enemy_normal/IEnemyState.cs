public interface IEnemyState
{
    void EnterState(EnemyAIContext context);
    void UpdateState(EnemyAIContext context);
    void ExitState(EnemyAIContext context);
}
