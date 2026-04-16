using UnityEngine;

public interface IInteractable
{
    string GetPrompt(InteractionSystem player);
    void OnInteract(InteractionSystem player);
}