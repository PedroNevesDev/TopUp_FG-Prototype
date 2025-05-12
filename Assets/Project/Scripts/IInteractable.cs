using UnityEngine;

public interface IInteractable
{
    string InteractName { get; }  // Property instead of a field
    void OnInteract();
    void OnInterrupt();
}
