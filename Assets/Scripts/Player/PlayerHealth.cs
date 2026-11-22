using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private PlayerRespawn respawn;

    protected override void Awake()
    {
        base.Awake();
        if (respawn == null) respawn = GetComponent<PlayerRespawn>();
    }

    protected override void Die()
    {
        if (respawn != null) respawn.Respawn();
        ResetHealth();
    }
}
