using UnityEngine;

public class HurtState : IState
{
    private readonly EnemyController ctx;
    private float timer;

    public HurtState(EnemyController ctx) => this.ctx = ctx;

    public void Enter()
    {
        timer = ctx.Stats.hurtTime;
        ctx.Animator.SetTrigger("Hurt");
    }

    public void Tick()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) ctx.Machine.ChangeState(ctx.ChaseState);
    }

    public void Exit() => ctx.Animator.ResetTrigger("Hurt");
}
