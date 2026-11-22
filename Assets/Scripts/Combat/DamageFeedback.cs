using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageFeedback : MonoBehaviour
{
    [Header("Візуальний спалах")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField, Range(0.03f, 0.3f)] private float flashDuration = 0.12f;
    [SerializeField] private bool blinkDuringInvulnerability = false;

    [Header("Тряска камери (опційно)")]
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField, Range(0f, 1f)] private float damageImpulseForce = 0.25f;

    private Health health;
    private Color baseColor;
    private bool baseColorCaptured;
    private Coroutine flashRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (targetRenderer != null)
        {
            baseColor = targetRenderer.color;
            baseColorCaptured = true;
        }
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDamagedEvent += HandleDamaged;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDamagedEvent -= HandleDamaged;
    }

    private void HandleDamaged()
    {
        if (impulseSource != null && damageImpulseForce > 0f)
            impulseSource.GenerateImpulseWithForce(damageImpulseForce);

        if (targetRenderer == null) return;
        if (!baseColorCaptured)
        {
            baseColor = targetRenderer.color;
            baseColorCaptured = true;
        }

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        targetRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);

        if (blinkDuringInvulnerability && health != null && health.Current > 0)
        {
            float remaining = Mathf.Max(0f, health.InvulnerableTime - flashDuration);
            float elapsed = 0f;
            bool faded = false;
            while (elapsed < remaining)
            {
                faded = !faded;
                Color c = baseColor;
                c.a = faded ? 0.35f : baseColor.a;
                targetRenderer.color = c;
                yield return new WaitForSeconds(0.08f);
                elapsed += 0.08f;
            }
        }

        targetRenderer.color = baseColor;
        flashRoutine = null;
    }
}
