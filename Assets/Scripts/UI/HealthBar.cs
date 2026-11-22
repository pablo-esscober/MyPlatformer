using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private IntEventSO healthChanged;
    [SerializeField] private IntEventSO maxHealthChanged;
    [SerializeField] private Image fill;

    private int maxHP = 1;

    private void OnEnable()
    {
        healthChanged.Register(OnHealthChanged);
        maxHealthChanged.Register(OnMaxHealthChanged);
    }

    private void OnDisable()
    {
        healthChanged.Unregister(OnHealthChanged);
        maxHealthChanged.Unregister(OnMaxHealthChanged);
    }

    private void OnMaxHealthChanged(int value) => maxHP = Mathf.Max(1, value);

    private void OnHealthChanged(int value)
    {
        if (fill != null)
            fill.fillAmount = Mathf.Clamp01((float)value / maxHP);
    }
}
