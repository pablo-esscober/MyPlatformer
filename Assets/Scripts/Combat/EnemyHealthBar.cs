using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Transform fillPivot;
    [SerializeField] private float fullWidth = 0.9f;

    private void Awake()
    {
        if (health == null)
            health = GetComponentInParent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnHealthChanged += UpdateBar;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= UpdateBar;
    }

    private void Start()
    {
        if (health != null)
            UpdateBar(health.Current, health.Max);
    }

    private void LateUpdate()
    {
        // Keep bar orientation unflipped when parent enemy flips X scale
        if (transform.parent != null)
        {
            float parentSign = transform.parent.lossyScale.x < 0f ? -1f : 1f;
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * parentSign;
            transform.localScale = s;
        }
    }

    private void UpdateBar(int current, int max)
    {
        if (current <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        if (fillPivot != null)
        {
            Vector3 s = fillPivot.localScale;
            s.x = fullWidth * ratio;
            fillPivot.localScale = s;
            Vector3 p = fillPivot.localPosition;
            p.x = -fullWidth * 0.5f * (1f - ratio);
            fillPivot.localPosition = p;
        }
    }
}
