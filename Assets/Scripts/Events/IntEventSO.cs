using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Events/Int Event", fileName = "NewIntEvent")]
public class IntEventSO : ScriptableObject
{
    private event Action<int> listeners;

    public void Raise(int value) => listeners?.Invoke(value);
    public void Register(Action<int> callback) => listeners += callback;
    public void Unregister(Action<int> callback) => listeners -= callback;

    private void OnDisable() => listeners = null;
}
