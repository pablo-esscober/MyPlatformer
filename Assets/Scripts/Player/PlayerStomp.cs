using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStomp : MonoBehaviour
{
    [SerializeField] private Transform stompPoint;
    [SerializeField] private Vector2 stompBoxSize = new Vector2(0.85f, 0.35f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Range(1, 100)]   private int damage = 30;
    [SerializeField, Range(3f, 20f)]  private float bounceForce = 10f;
    [SerializeField, Range(0.1f, 1f)] private float stompInvulnerabilityTime = 0.35f;
    [SerializeField] private PlayerHealth playerHealth;

    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (stompPoint == null || body.linearVelocity.y > 0.1f) return;

        Collider2D hit = Physics2D.OverlapBox(stompPoint.position, stompBoxSize, 0f, enemyLayer);
        if (hit == null) return;

        // Only count as Mario-style stomp when player's feet are above the enemy's center
        if (transform.position.y < hit.bounds.center.y - 0.1f) return;

        if (hit.TryGetComponent(out IDamageable target))
        {
            if (playerHealth != null)
                playerHealth.GrantTemporaryInvulnerability(stompInvulnerabilityTime);

            target.TakeDamage(damage, transform.position);
            body.linearVelocity = new Vector2(body.linearVelocity.x, bounceForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (stompPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(stompPoint.position, stompBoxSize);
    }
}
