using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemID = "Item";
    [SerializeField] private string promptText = "Press E to pick up";

    public void GameStarting()
    {
        Debug.Log("PickupItem knows game is starting");
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
    }

    public string GetPrompt(InteractionSystem player)
    {
        return promptText;
    }

    public void OnInteract(InteractionSystem player)
    {
        player.AddItem(itemID);
        // gameObject.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
    }
    
}
