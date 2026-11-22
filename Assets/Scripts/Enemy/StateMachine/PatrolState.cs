public class PatrolState : IState
{
    private readonly EnemyController ctx;

    public PatrolState(EnemyController ctx) => this.ctx = ctx;

    public void Enter() { }

    public void Tick()
    {
        if (ctx.IsGrounded && (ctx.AtEdge() || ctx.AtWall()))
        {
            ctx.Stop();
            ctx.Flip();
        }

        ctx.Move(ctx.Stats.patrolSpeed);

        if (ctx.DistanceToTarget <= ctx.Stats.chaseRange && ctx.TargetOnSameLevel)
            ctx.Machine.ChangeState(ctx.ChaseState);
    }

    public void Exit() { }
}
