using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask targetLayers;

    public Action<Projectile> OnFinished;

    private int damage;
    private float dieAt;
    private Rigidbody2D body;

    private void Awake() => body = GetComponent<Rigidbody2D>();

    public void Launch(Vector2 direction, int dmg)
    {
        damage = dmg;
        dieAt = Time.time + lifetime;
        transform.right = direction;
        body.linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        if (Time.time >= dieAt) OnFinished?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(damage, transform.position);

        OnFinished?.Invoke(this);
    }
}
