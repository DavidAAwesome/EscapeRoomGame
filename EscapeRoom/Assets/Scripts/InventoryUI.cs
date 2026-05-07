using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private InteractionSystem inventorySource;

    [Header("Slots")]
    [SerializeField] private RectTransform[] slotRoots;
    [SerializeField] private TMP_Text[] slotLabels;
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private TMP_Text[] slotItemNames;

    [Header("Selection")]
    [SerializeField] private RectTransform selectionFrame;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] private Color filledColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private Color selectedColor = new Color(1f, 0.9f, 0.35f, 1f);

    private int selectedSlot;

    void Start()
    {
        if (inventorySource == null)
            inventorySource = FindObjectOfType<InteractionSystem>();

        if (inventorySource == null)
        {
            Debug.LogWarning("InventoryUI could not find InteractionSystem.");
            enabled = false;
            return;
        }

        inventorySource.OnInventoryChanged += RefreshSlots;
        inventorySource.OnSelectionChanged += SelectSlot;

        for (int i = 0; i < slotLabels.Length; i++)
        {
            if (slotLabels[i] != null)
                slotLabels[i].text = (i + 1).ToString();
        }

        RefreshSlots(inventorySource.Slots);
        SelectSlot(inventorySource.SelectedSlot);
    }

    void OnDestroy()
    {
        if (inventorySource == null)
            return;

        inventorySource.OnInventoryChanged -= RefreshSlots;
        inventorySource.OnSelectionChanged -= SelectSlot;
    }

    void RefreshSlots(string[] slots)
    {
        for (int i = 0; i < slotRoots.Length; i++)
        {
            string itemID = "";

            if (slots != null && i < slots.Length)
                itemID = slots[i];

            bool hasItem = !string.IsNullOrEmpty(itemID);

            Image bg = slotRoots[i] != null ? slotRoots[i].GetComponent<Image>() : null;
            if (bg != null)
                bg.color = hasItem ? filledColor : emptyColor;
            
            if (i < slotIcons.Length && slotIcons[i] != null)
            {
                Sprite icon = inventorySource.SlotIcons[i];

                slotIcons[i].sprite = icon;
                slotIcons[i].enabled = icon != null;
            }

            if (i < slotItemNames.Length && slotItemNames[i] != null)
            {
                slotItemNames[i].text = hasItem ? itemID : "";
                slotItemNames[i].enabled = hasItem;
            }
        }

        SelectSlot(selectedSlot);
    }

    void SelectSlot(int index)
    {
        selectedSlot = index;

        for (int i = 0; i < slotRoots.Length; i++)
        {
            Image bg = slotRoots[i] != null ? slotRoots[i].GetComponent<Image>() : null;

            if (bg == null)
                continue;

            string itemID = "";

            if (inventorySource != null && inventorySource.Slots != null && i < inventorySource.Slots.Length)
                itemID = inventorySource.Slots[i];

            bool hasItem = !string.IsNullOrEmpty(itemID);

            if (i == selectedSlot)
                bg.color = selectedColor;
            else
                bg.color = hasItem ? filledColor : emptyColor;
        }

        if (selectionFrame != null && index >= 0 && index < slotRoots.Length && slotRoots[index] != null)
        {
            selectionFrame.anchoredPosition = slotRoots[index].anchoredPosition;
        }
    }
}