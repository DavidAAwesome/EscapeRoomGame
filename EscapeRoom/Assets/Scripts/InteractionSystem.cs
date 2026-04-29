using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    public const int SLOT_COUNT = 3;

    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactPromptText;

    private HashSet<string> inventory = new HashSet<string>();
    private string[] slots = new string[SLOT_COUNT];

    private int selectedSlot;
    private IInteractable currentTarget;

    public int SelectedSlot => selectedSlot;
    public string SelectedItem => slots[selectedSlot];
    
    public event System.Action<string[]> OnInventoryChanged;
    public event System.Action<int> OnSelectionChanged;

    public string[] Slots => slots;

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        DetectInteractable();
    }

    public void Interact()
    {
        if (currentTarget != null)
            currentTarget.OnInteract(this);
    }

    void DetectInteractable()
    {
        if (playerCamera == null)
            return;

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

    public void SelectSlot(int index)
    {
        index = Mathf.Clamp(index, 0, SLOT_COUNT - 1);

        if (selectedSlot == index)
            return;

        selectedSlot = index;
        Debug.Log("Selected slot: " + (selectedSlot + 1));

        OnSelectionChanged?.Invoke(selectedSlot);
    }

    public void CycleSlot(int direction)
    {
        int nextSlot = selectedSlot + direction;

        if (nextSlot < 0)
            nextSlot = SLOT_COUNT - 1;
        else if (nextSlot >= SLOT_COUNT)
            nextSlot = 0;

        SelectSlot(nextSlot);
    }

    public bool AddItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return false;

        if (inventory.Contains(itemID))
            return false;

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (string.IsNullOrEmpty(slots[i]))
            {
                slots[i] = itemID;
                inventory.Add(itemID);
                OnInventoryChanged?.Invoke(slots);

                Debug.Log("Picked up: " + itemID);
                GameManager.Instance?.OnItemPickedUp(itemID);
                return true;
            }
        }

        Debug.Log("Inventory is full");
        return false;
    }

    public bool HasItem(string itemID)
    {
        return inventory.Contains(itemID);
    }

    public bool HasSelectedItem(string itemID)
    {
        return SelectedItem == itemID;
    }

    public bool ConsumeItem(string itemID)
    {
        if (!inventory.Contains(itemID))
            return false;

        inventory.Remove(itemID);
        OnInventoryChanged?.Invoke(slots);

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (slots[i] == itemID)
            {
                slots[i] = null;
                break;
            }
        }

        Debug.Log("Used: " + itemID);
        return true;
    }

    public bool ConsumeSelectedItem()
    {
        string itemID = SelectedItem;

        if (string.IsNullOrEmpty(itemID))
            return false;

        return ConsumeItem(itemID);
    }

    public void ClearInventory()
    {
        inventory.Clear();
        OnInventoryChanged?.Invoke(slots);
        OnSelectionChanged?.Invoke(selectedSlot);

        for (int i = 0; i < SLOT_COUNT; i++)
            slots[i] = null;

        selectedSlot = 0;
    }

    void ShowPrompt(string text)
    {
        if (interactPromptText == null)
            return;

        bool show = !string.IsNullOrEmpty(text);
        interactPromptText.gameObject.SetActive(show);

        if (show)
            interactPromptText.text = text;
    }
}
