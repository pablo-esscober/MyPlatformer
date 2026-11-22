using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] private EnemyController controller;
    [SerializeField] private Collider2D hitbox;

    protected override void Awake()
    {
        base.Awake();
        if (controller == null) controller = GetComponent<EnemyController>();
        if (hitbox == null) hitbox = GetComponent<Collider2D>();
    }

    protected override void OnDamaged()
    {
        if (controller != null)
            controller.Machine.ChangeState(controller.HurtState);
    }

    protected override void Die()
    {
        if (hitbox != null) hitbox.enabled = false;
        if (controller != null)
        {
            controller.Machine.ChangeState(controller.DeadState);
            controller.enabled = false;
        }
        Destroy(gameObject, 1f);
    }
}
