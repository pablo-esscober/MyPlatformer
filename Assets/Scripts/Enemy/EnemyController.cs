using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Дані")]
    [SerializeField] private EnemyStatsSO stats;

    [Header("Точки")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform edgeCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform attackPoint;

    [Header("Шари")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask damageableLayers;

    [Header("Звук")]
    [SerializeField] private AudioSource audioSource;

    [Header("Стрільба")]
    [SerializeField] private Projectile projectilePrefab;

    private const float ProbeRadius = 0.15f;
    private const float EdgeRayStartOffsetY = 0.25f;
    private const float EdgeRayLength = 0.6f;
    private const float FacingDeadZone = 0.15f;

    public EnemyStatsSO Stats => stats;
    public Rigidbody2D Body { get; private set; }
    public Animator Animator { get; private set; }
    public StateMachine Machine { get; private set; }
    public Transform Target { get; private set; }
    public int Facing { get; private set; } = 1;

    public PatrolState PatrolState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public IState AttackState { get; private set; }
    public HurtState HurtState { get; private set; }
    public DeadState DeadState { get; private set; }

    private ObjectPool<Projectile> pool;

    private void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
        Animator = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        Machine = new StateMachine();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) Target = playerObj.transform;

        if (TryGetComponent(out Health health))
            health.Configure(stats.maxHealth);

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = stats.tint;
        ApplyScale();

        PatrolState = new PatrolState(this);
        ChaseState = new ChaseState(this);
        HurtState = new HurtState(this);
        DeadState = new DeadState(this);
        AttackState = stats.ranged
            ? (IState)new RangedAttackState(this)
            : new MeleeAttackState(this);

        if (projectilePrefab != null)
        {
            pool = new ObjectPool<Projectile>(
                createFunc:      () => Instantiate(projectilePrefab),
                actionOnGet:     p => p.gameObject.SetActive(true),
                actionOnRelease: p => p.gameObject.SetActive(false),
                actionOnDestroy: p => Destroy(p.gameObject),
                defaultCapacity: 20);
        }
    }

    private void Start() => Machine.ChangeState(PatrolState);

    private void Update()
    {
        if (Target == null) return;
        Machine.Tick();
    }

    private void FixedUpdate()
    {
        // Hard platform-edge safety guard: neither fast movement nor knockback can push enemy off a ledge
        if (!IsGrounded || edgeCheck == null) return;

        float vx = Body.linearVelocity.x;
        if (Mathf.Abs(vx) <= 0.01f) return;

        float moveDir = Mathf.Sign(vx);
        if (!HasGroundInDirection(moveDir))
            Body.linearVelocity = new Vector2(0f, Body.linearVelocity.y);
    }

    public float DistanceToTarget =>
        Target != null ? Mathf.Abs(Target.position.x - transform.position.x) : float.MaxValue;

    public bool TargetOnSameLevel =>
        Target != null && groundCheck != null &&
        Mathf.Abs(Target.position.y - groundCheck.position.y) <= stats.verticalTolerance;

    public void Move(float speed)
    {
        if (speed <= 0.001f || (IsGrounded && AtEdge()))
        {
            Stop();
            return;
        }
        Body.linearVelocity = new Vector2(Facing * speed, Body.linearVelocity.y);
        if (Animator != null) Animator.SetFloat("Speed", Mathf.Abs(Body.linearVelocity.x));
    }

    public void Stop()
    {
        Body.linearVelocity = new Vector2(0f, Body.linearVelocity.y);
        if (Animator != null) Animator.SetFloat("Speed", 0f);
    }

    public void Flip()
    {
        Facing = -Facing;
        ApplyScale();
    }

    public void FaceTarget()
    {
        if (Target == null) return;
        float dx = Target.position.x - transform.position.x;
        if (Mathf.Abs(dx) < FacingDeadZone) return;
        int dir = dx > 0f ? 1 : -1;
        if (dir != Facing) Flip();
    }

    private void ApplyScale()
    {
        float s = stats != null ? stats.sizeScale : 1f;
        transform.localScale = new Vector3(Facing * s, s, 1f);
    }

    public bool IsGrounded =>
        groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, ProbeRadius, groundLayer);

    public bool AtEdge() =>
        edgeCheck != null && !HasGroundInDirection(Facing);

    private bool HasGroundInDirection(float dirSign)
    {
        float s = stats != null ? stats.sizeScale : 1f;
        float aheadX = Mathf.Abs(edgeCheck.localPosition.x) * s;
        Vector2 rayOrigin = (Vector2)transform.position + new Vector2(dirSign * aheadX, EdgeRayStartOffsetY * s);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, EdgeRayLength * s, groundLayer);
        return hit.collider != null;
    }

    public bool AtWall()
    {
        if (wallCheck == null) return false;
        float s = stats != null ? stats.sizeScale : 1f;
        float dist = Mathf.Abs(wallCheck.localPosition.x) * s;
        Vector2 origin = (Vector2)transform.position + new Vector2(0f, wallCheck.localPosition.y * s);
        RaycastHit2D hit = Physics2D.Raycast(origin, new Vector2(Facing, 0f), dist, groundLayer);
        return hit.collider != null;
    }

    public void PlayTelegraph()
    {
        if (audioSource != null && stats.telegraphSfx != null)
            audioSource.PlayOneShot(stats.telegraphSfx);
    }

    public void DealDamage()
    {
        if (attackPoint == null) return;
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, stats.attackRadius, damageableLayers);
        if (hit != null && hit.TryGetComponent(out IDamageable target))
            target.TakeDamage(stats.damage, transform.position);
    }

    public void Shoot()
    {
        if (pool == null || attackPoint == null) return;
        Projectile p = pool.Get();
        p.transform.position = attackPoint.position;
        p.OnFinished = Release;
        p.Launch(new Vector2(Facing, 0f), stats.damage);
    }

    private void Release(Projectile p)
    {
        p.OnFinished = null;
        pool.Release(p);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, stats != null ? stats.attackRadius : 0.6f);
        }
        Gizmos.color = Color.cyan;
        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, ProbeRadius);
        if (edgeCheck != null)
        {
            float s = stats != null ? stats.sizeScale : 1f;
            float aheadX = Mathf.Abs(edgeCheck.localPosition.x) * s;
            Vector3 start = transform.position + new Vector3(Facing * aheadX, EdgeRayStartOffsetY * s, 0f);
            Gizmos.DrawLine(start, start + Vector3.down * (EdgeRayLength * s));
        }
        if (wallCheck != null)
        {
            float s = stats != null ? stats.sizeScale : 1f;
            float dist = Mathf.Abs(wallCheck.localPosition.x) * s;
            Vector3 start = transform.position + new Vector3(0f, wallCheck.localPosition.y * s, 0f);
            Gizmos.DrawLine(start, start + new Vector3(Facing * dist, 0f, 0f));
        }
    }
}
