using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactPromptText;

    private readonly HashSet<string> inventory = new HashSet<string>();

    private IInteractable currentTarget;

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        DetectInteractable();
    }

    public void Interact()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        if (currentTarget != null)
            currentTarget.OnInteract(this);
    }

    private void DetectInteractable()
    {
        if (playerCamera == null)
        {
            currentTarget = null;
            ShowPrompt(null);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                currentTarget = interactable;
                ShowPrompt(interactable.GetPrompt(this));
                return;
            }
        }

        currentTarget = null;
        ShowPrompt(null);
    }

    public bool HasItem(string itemID)
    {
        return inventory.Contains(itemID);
    }

    public void AddItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return;

        inventory.Add(itemID);
        Debug.Log("Picked up: " + itemID);
        GameManager.Instance?.OnItemPickedUp(itemID);
    }

    public bool ConsumeItem(string itemID)
    {
        if (!inventory.Contains(itemID))
            return false;

        inventory.Remove(itemID);
        Debug.Log("Used: " + itemID);
        return true;
    }

    public IEnumerable<string> GetInventory()
    {
        return inventory;
    }

    private void ShowPrompt(string text)
    {
        if (interactPromptText == null)
            return;

        bool show = !string.IsNullOrEmpty(text);
        interactPromptText.gameObject.SetActive(show);

        if (show)
            interactPromptText.text = text;
    }
}





