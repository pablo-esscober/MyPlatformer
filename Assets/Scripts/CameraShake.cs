using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 basePosition;
    private Coroutine current;

    private void Awake() => basePosition = transform.localPosition;

    public void Shake(float duration = 0.12f, float strength = 0.15f)
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float damping = 1f - Mathf.Clamp01(elapsed / duration);
            Vector2 offset = Random.insideUnitCircle * (strength * damping);
            transform.localPosition = basePosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }

        transform.localPosition = basePosition;
        current = null;
    }
}
