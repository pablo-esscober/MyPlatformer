using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private PlayerRespawn respawn;

    [Header("Регенерація здоров'я")]
    [SerializeField, Range(0.5f, 10f)] private float regenDelay = 3f;
    [SerializeField, Range(1f, 30f)]   private float regenRate  = 6f;

    private float lastDamageTime = -999f;
    private float regenAccumulator;

    protected override void Awake()
    {
        base.Awake();
        if (respawn == null) respawn = GetComponent<PlayerRespawn>();
    }

    private void Update()
    {
        if (current <= 0 || current >= Max)
        {
            regenAccumulator = 0f;
            return;
        }

        if (Time.time < lastDamageTime + regenDelay) return;

        regenAccumulator += regenRate * Time.deltaTime;
        int healAmount = Mathf.FloorToInt(regenAccumulator);
        if (healAmount > 0)
        {
            regenAccumulator -= healAmount;
            Heal(healAmount);
        }
    }

    protected override void OnDamaged()
    {
        lastDamageTime = Time.time;
        regenAccumulator = 0f;
    }

    protected override void Die()
    {
        if (respawn != null) respawn.Respawn();
        ResetHealth();
        lastDamageTime = -999f;
        regenAccumulator = 0f;
    }
}
