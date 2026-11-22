using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject target;
    private bool isOn;

    public void Interact()
    {
        isOn = !isOn;
        if (target != null)
            target.SetActive(isOn);
    }
}
