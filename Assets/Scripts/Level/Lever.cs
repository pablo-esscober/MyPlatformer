using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject target;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

    private bool isOn;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateVisual();
    }

    public void Interact()
    {
        isOn = !isOn;
        if (target != null)
            target.SetActive(isOn);

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        if (offSprite != null && onSprite != null)
            spriteRenderer.sprite = isOn ? onSprite : offSprite;
        else
            spriteRenderer.flipX = isOn;
    }
}
