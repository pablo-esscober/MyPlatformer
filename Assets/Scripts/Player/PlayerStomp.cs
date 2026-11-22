using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStomp : MonoBehaviour
{
    [SerializeField] private Transform stompPoint;
    [SerializeField, Range(0.1f, 1f)] private float radius = 0.3f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int damage = 15;
    [SerializeField] private float bounceForce = 9f;

    private Rigidbody2D body;

    private void Awake() => body = GetComponent<Rigidbody2D>();

    private void Update()
    {
        if (stompPoint == null || body.linearVelocity.y >= 0f) return;

        Collider2D hit = Physics2D.OverlapCircle(stompPoint.position, radius, enemyLayer);
        if (hit == null) return;

        if (hit.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(damage, transform.position);
            body.linearVelocity = new Vector2(body.linearVelocity.x, bounceForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (stompPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(stompPoint.position, radius);
    }
}
