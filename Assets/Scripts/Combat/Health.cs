using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 30;
    [SerializeField, Range(0f, 2f)]  private float invulnerableTime = 0.6f;
    [SerializeField, Range(0f, 20f)] private float knockbackForce   = 8f;

    protected int current;
    private float invulnerableUntil;
    private Rigidbody2D body;

    public int Current => current;
    public int Max => maxHealth;
    public float InvulnerableTime => invulnerableTime;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDamagedEvent;

    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        current = maxHealth;
    }

    public void Configure(int max)
    {
        maxHealth = Mathf.Max(1, max);
        current = maxHealth;
        OnHealthChanged?.Invoke(current, maxHealth);
    }

    public void ResetHealth()
    {
        current = maxHealth;
        OnHealthChanged?.Invoke(current, maxHealth);
    }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (current <= 0 || Time.time < invulnerableUntil) return;

        invulnerableUntil = Time.time + invulnerableTime;
        current = Mathf.Max(0, current - amount);

        Vector2 dir = ((Vector2)transform.position - sourcePosition).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.up;
        dir = new Vector2(dir.x, Mathf.Max(dir.y, 0.35f)).normalized;
        body.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        OnHealthChanged?.Invoke(current, maxHealth);
        OnDamagedEvent?.Invoke();

        if (current == 0) Die();
        else              OnDamaged();
    }

    protected virtual void OnDamaged() { }
    protected virtual void Die() { }
}
