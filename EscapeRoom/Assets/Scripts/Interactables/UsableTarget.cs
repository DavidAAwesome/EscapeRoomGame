using UnityEngine;

public class UsableTarget : MonoBehaviour, IInteractable
{
    [SerializeField] private string requiredItem = "";
    [SerializeField] private string puzzleID = "";
    [SerializeField] private string usePrompt = "Press E to use";
    [SerializeField] private string missingItemPrompt = "You need something to use this";
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private bool consumeItem = true;

    private bool used;
    
    public void GameStarting()
    {
        used = false;
    }

    public string GetPrompt(InteractionSystem player)
    {
        if (used)
            return null;

        if (!string.IsNullOrEmpty(requiredItem) && !player.HasItem(requiredItem))
            return missingItemPrompt;

        return usePrompt;
    }

    public void OnInteract(InteractionSystem player)
    {
        if (used)
            return;

        if (!string.IsNullOrEmpty(requiredItem))
        {
            if (!player.HasItem(requiredItem))
            {
                Debug.Log("Missing item: " + requiredItem);
                return;
            }

            if (consumeItem)
                player.ConsumeItem(requiredItem);
        }

        used = true;

        if (unlockSound != null)
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);

        if (!string.IsNullOrEmpty(puzzleID))
            PuzzleManager.Instance?.OnPuzzleSolved(puzzleID);

        Debug.Log("Activated: " + gameObject.name);
        SendMessage("OnUnlocked", SendMessageOptions.DontRequireReceiver);
    }
}
