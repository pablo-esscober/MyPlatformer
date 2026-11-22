using UnityEngine;

public class EnemyGate : MonoBehaviour
{
    [Header("Ворог-охоронець, після знищення якого зникає бар'єр")]
    [SerializeField] private Health guardian;

    private void OnEnable()
    {
        if (guardian != null)
            guardian.OnHealthChanged += HandleGuardianHealthChanged;
    }

    private void OnDisable()
    {
        if (guardian != null)
            guardian.OnHealthChanged -= HandleGuardianHealthChanged;
    }

    private void Update()
    {
        if (guardian == null || guardian.Current <= 0)
            gameObject.SetActive(false);
    }

    private void HandleGuardianHealthChanged(int current, int max)
    {
        if (current <= 0)
            gameObject.SetActive(false);
    }
}
