using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Рух")]
    [SerializeField, Range(1f, 20f)]  private float maxSpeed = 8f;
    [SerializeField, Range(5f, 200f)] private float acceleration = 70f;
    [SerializeField, Range(5f, 200f)] private float deceleration = 90f;

    [Header("Земля")]
    [SerializeField] private Transform groundCheck;
    [SerializeField, Range(0.05f, 0.5f)] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Стрибок")]
    [SerializeField, Range(0.5f, 8f)] private float jumpHeight = 3f;
    [SerializeField, Range(0.5f, 8f)] private float baseGravity = 3f;

    [Header("Прийоми тюнінгу")]
    [SerializeField, Range(0f, 0.4f)] private float coyoteTime = 0.12f;
    [SerializeField, Range(0f, 0.4f)] private float jumpBufferTime = 0.15f;
    [SerializeField, Range(1f, 6f)]   private float fallMultiplier = 2.2f;
    [SerializeField, Range(1f, 8f)]   private float lowJumpMultiplier = 3.5f;

    [Header("Вигляд")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Зворотний зв'язок")]
    [SerializeField, Range(1f, 20f)] private float hardLandingSpeed = 9f;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Звук")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] footsteps;
    [SerializeField, Range(0f, 0.3f)]   private float pitchVariance = 0.12f;
    [SerializeField, Range(0.1f, 0.8f)] private float stepInterval = 0.32f;

    [Header("Взаємодія")]
    [SerializeField] private Transform interactPoint;
    [SerializeField, Range(0.1f, 2f)] private float interactRadius = 0.6f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Ближня атака (E)")]
    [SerializeField, Range(1, 100)]     private int meleeDamage = 15;
    [SerializeField, Range(0.2f, 2f)]   private float attackRadius = 0.85f;
    [SerializeField, Range(0.2f, 1.5f)] private float attackOffsetX = 0.7f;
    [SerializeField, Range(0.1f, 1f)]   private float attackCooldown = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private Rigidbody2D body;
    private PlayerControls controls;
    private float moveInput;
    private int facing = 1;
    private bool isGrounded;
    private bool wasGrounded;
    private float fallSpeed;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private float stepTimer;
    private float nextAttackTime;

    private float JumpVelocity =>
        Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * baseGravity * jumpHeight);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();

        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (source == null) source = GetComponent<AudioSource>();
        if (impulseSource == null) impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()  => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();
    private void OnDestroy() => controls.Dispose();

    private void Update()
    {
        if (PauseController.IsPaused) return;

        isGrounded = CheckGround();

        bool canRefill = isGrounded && body.linearVelocity.y <= 0.01f;
        coyoteCounter = canRefill ? coyoteTime : coyoteCounter - Time.deltaTime;

        jumpBufferCounter = controls.Player.Jump.WasPressedThisFrame()
            ? jumpBufferTime
            : jumpBufferCounter - Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        if (controls.Player.Interact.WasPressedThisFrame())
        {
            TryInteract();
            TryMeleeAttack();
        }

        moveInput = controls.Player.Move.ReadValue<Vector2>().x;

        CheckLanding();
        UpdateFootsteps();
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        float target = moveInput * maxSpeed;
        float rate = Mathf.Abs(target) > 0.01f ? acceleration : deceleration;
        float x = Mathf.MoveTowards(body.linearVelocity.x, target, rate * Time.fixedDeltaTime);
        body.linearVelocity = new Vector2(x, body.linearVelocity.y);

        if (body.linearVelocity.y < 0f)
            body.gravityScale = baseGravity * fallMultiplier;
        else if (body.linearVelocity.y > 0f && !controls.Player.Jump.IsPressed())
            body.gravityScale = baseGravity * lowJumpMultiplier;
        else
            body.gravityScale = baseGravity;
    }

    private bool CheckGround() =>
        groundCheck != null &&
        Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    private void Jump()
    {
        body.gravityScale = baseGravity;
        body.linearVelocity = new Vector2(body.linearVelocity.x, JumpVelocity);
    }

    private void TryInteract()
    {
        if (interactPoint == null) return;
        Collider2D hit = Physics2D.OverlapCircle(interactPoint.position, interactRadius, interactableLayer);
        if (hit != null && hit.TryGetComponent(out IInteractable target))
            target.Interact();
    }

    private void TryMeleeAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        Vector2 origin = interactPoint != null
            ? (Vector2)interactPoint.position
            : (Vector2)transform.position + Vector2.up * 0.6f;
        Vector2 hitCenter = origin + new Vector2(facing * attackOffsetX, 0f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, attackRadius, enemyLayer);
        bool landed = false;
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(meleeDamage, transform.position);
                landed = true;
            }
        }

        if (landed && impulseSource != null)
            impulseSource.GenerateImpulseWithForce(0.15f);
    }

    private void CheckLanding()
    {
        if (!isGrounded)
            fallSpeed = Mathf.Max(fallSpeed, -body.linearVelocity.y);

        if (isGrounded && !wasGrounded)
            OnLanded(fallSpeed);

        if (isGrounded)
            fallSpeed = 0f;

        wasGrounded = isGrounded;
    }

    private void OnLanded(float speed)
    {
        if (speed < hardLandingSpeed) return;
        if (impulseSource != null)
            impulseSource.GenerateImpulseWithForce(0.3f);
    }

    private void UpdateVisuals()
    {
        if (animator != null)
            animator.SetFloat(SpeedHash, Mathf.Abs(body.linearVelocity.x));

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            facing = moveInput < 0f ? -1 : 1;
            if (spriteRenderer != null)
                spriteRenderer.flipX = facing < 0;
        }
    }

    private void UpdateFootsteps()
    {
        bool running = isGrounded && Mathf.Abs(body.linearVelocity.x) > 0.5f;
        if (!running)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = stepInterval;
        }
    }

    private void PlayFootstep()
    {
        if (source == null || footsteps == null || footsteps.Length == 0) return;
        AudioClip clip = footsteps[Random.Range(0, footsteps.Length)];
        if (clip == null) return;
        source.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        source.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Application.isPlaying && isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (interactPoint != null)
        {
            Gizmos.color = Color.magenta;
            Vector2 hitCenter = (Vector2)interactPoint.position + new Vector2(facing * attackOffsetX, 0f);
            Gizmos.DrawWireSphere(hitCenter, attackRadius);
        }
    }
}
